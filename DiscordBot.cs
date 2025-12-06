using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using SteamQuery;
using System.Reflection;
using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    internal class DiscordBot : IDiscordBot, IDisposable
    {
        private System.Timers.Timer? _playerStatusTimer;
        private bool _disposed;

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;

        public DiscordBot(IConfiguration configuration, ILogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
        }

        public async Task StartAsync()
        {
            string discordToken = _configuration["DiscordToken"] ?? throw new Exception("Discord token not found");

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            _client.Ready += Client_Ready;
            _client.MessageReceived += HandleCommandAsync;

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await _logger.Log("Discord bot started successfully");
        }

        private async Task Client_Ready()
        {
            await _logger.Log("Discord client is ready");

            // Set initial player count now that client is ready
            await SetPlayerCount();

            // Start the timer after the first successful update
            int intervalSeconds = int.TryParse(_configuration["Bot:StatusUpdateIntervalSeconds"], out int interval) ? interval : 60;
            _playerStatusTimer = new System.Timers.Timer(intervalSeconds * 1000);
            _playerStatusTimer.Elapsed += PlayerStatusTimer_Elapsed;
            _playerStatusTimer.AutoReset = true;
            _playerStatusTimer.Enabled = true;
        }

        private async void PlayerStatusTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            await SetPlayerCount();
        }

        private async Task SetPlayerCount()
        {
            try
            {
                string serverHost = _configuration["ValheimServer:Host"] ?? "apaluchdev.com";
                int serverPort = int.TryParse(_configuration["ValheimServer:QueryPort"], out int port) ? port : 2457;

                using var server = new GameServer("localhost:27015")
                {
                    SendTimeout = TimeSpan.FromSeconds(5.0d),
                    ReceiveTimeout = TimeSpan.FromSeconds(5.0d)
                };

                var players = await server.GetPlayersAsync();

                await _client.SetCustomStatusAsync($"{_configuration["ValheimServer:Host"]} - Players: {players.Count()} / 10");
                await _logger.Log($"Updated player status: {players.Count()} / 10");
            }
            catch (Exception ex)
            {
                await _logger.Log($"Error updating player count: {ex.Message}");
                await _logger.Log($"Exception type: {ex.GetType().Name}");
                await _logger.Log($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    await _logger.Log($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public async Task StopAsync()
        {
            await _logger.Log("Stopping Discord bot...");

            if (_playerStatusTimer != null)
            {
                _playerStatusTimer.Enabled = false;
                _playerStatusTimer.Elapsed -= PlayerStatusTimer_Elapsed;
            }

            if (_client != null)
            {
                _client.Ready -= Client_Ready;
                _client.MessageReceived -= HandleCommandAsync;
                await _client.LogoutAsync();
                await _client.StopAsync();
            }

            await _logger.Log("Discord bot stopped");
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot)
            {
                return;
            }

            int position = 0;
            string commandPrefix = _configuration["Bot:CommandPrefix"] ?? "/";
            bool messageIsCommand = message.HasCharPrefix(commandPrefix[0], ref position);

            if (messageIsCommand)
            {
                await _commands.ExecuteAsync(
                    new SocketCommandContext(_client, message),
                    position,
                    _serviceProvider);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _playerStatusTimer?.Dispose();
            _client?.Dispose();

            _disposed = true;
        }
    }
}
