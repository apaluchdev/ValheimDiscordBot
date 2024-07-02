using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    public class ValheimLog
    {
        public string? Content { get; set; }
    }

    internal class DiscordBot : IDiscordBot
    {
        private static System.Timers.Timer playerStatusTimer;
        private static readonly HttpClient client = new HttpClient();
        private ServiceProvider? _serviceProvider;
        private readonly string _apiUrl = "http://valheim.apaluchdev.com:9001/readfile";
        private string _lastStartTime = null;
        private int _playerCount = 0;

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        public DiscordBot(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services)
        {
            // Remember to add the discord token as part of your user secrets
            string discordToken = _configuration["DiscordToken"] ?? throw new Exception("Discord token not found");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            playerStatusTimer = new System.Timers.Timer(60000);

            // Subscribe to the Elapsed event
            playerStatusTimer.Elapsed += PlayerStatusTimer_Elapsed;

            // AutoReset set to true means the timer will reset after each elapsed event
            playerStatusTimer.AutoReset = true;

            // Start the timer
            playerStatusTimer.Enabled = true;

            _client.MessageReceived += HandleCommandAsync;
        }

        private async void PlayerStatusTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_client == null) return;

            await SetPlayerCount();
        }

        private async Task SetPlayerCount()
        {
            try
            {
                // Send a GET request to the API endpoint
                HttpResponseMessage response = await client.GetAsync(_apiUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string into a C# object
                ValheimLog apiResponse = JsonSerializer.Deserialize<ValheimLog>(responseBody ?? "", new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new ValheimLog();

                if (apiResponse.Content == null) return;

                var lines = apiResponse.Content.Split('\n');

                var joinLines = lines.Where(l => l.Contains("Got connection SteamID"));
                var exitLines = lines.Where(l => l.Contains("Closing socket"));

                var lastStartTime = lines.LastOrDefault(l => l.Contains("Game server connected")).Split(' ')[3];

                if (_lastStartTime != lastStartTime)
                {
                    _playerCount = 0;
                }

                _playerCount += joinLines.Count();
                _playerCount -= exitLines.Count();

                await _client.SetCustomStatusAsync($"Players online: {_playerCount}/10");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task StopAsync()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Ignore messages from bots
            if (arg is not SocketUserMessage message || message.Author.IsBot)
            {
                return;
            }

            // Check if the message starts with !
            int position = 0;
            bool messageIsCommand = message.HasCharPrefix('/', ref position);

            if (messageIsCommand)
            {
                // Execute the command if it exists in the ServiceCollection
                await _commands.ExecuteAsync(
                    new SocketCommandContext(_client, message),
                    position,
                    _serviceProvider);

                return;
            }
        }
    }
}
