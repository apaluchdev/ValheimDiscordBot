using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    internal class DiscordBot : IDiscordBot
    {
        private ServiceProvider? _serviceProvider;

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

            var playerCount = 2;
            await _client.SetCustomStatusAsync($"Valheim players {playerCount}/10");

            _client.MessageReceived += HandleCommandAsync;
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
