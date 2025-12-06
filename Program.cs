using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ValheimDiscordBot.Interfaces;
using System.Diagnostics;

namespace ValheimDiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Debug.WriteLine("Starting Discord Bot...");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IDiscordBot, DiscordBot>()
                .AddSingleton<ILogger, ConsoleLogger>()
                .BuildServiceProvider();

            using var cancellationTokenSource = new CancellationTokenSource();
            
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            try
            {
                var logger = serviceProvider.GetRequiredService<ILogger>();
                await logger.Log("Starting Discord Bot...");

                var discordBot = serviceProvider.GetRequiredService<IDiscordBot>();

                await discordBot.StartAsync();
                await logger.Log("Connected to Discord");

                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Shutting down gracefully...");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Fatal error: {exception.Message}");
                Environment.Exit(-1);
            }
            finally
            {
                var discordBot = serviceProvider.GetRequiredService<IDiscordBot>();
                await discordBot.StopAsync();

                if (discordBot is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                await serviceProvider.DisposeAsync();
            }
        }
    }
}