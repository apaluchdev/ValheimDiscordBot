using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IDiscordBot, DiscordBot>()
                .AddSingleton<ILogger, ConsoleLogger>()
                .BuildServiceProvider();

            try
            {
                IDiscordBot discordBot = serviceProvider.GetRequiredService<IDiscordBot>();

                await discordBot.StartAsync(serviceProvider);
                Console.WriteLine("Connected to Discord");

                do
                {
                    var keyInfo = Console.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("\nShutting down!");

                        await discordBot.StopAsync();
                        return;
                    }
                } while (true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Environment.Exit(-1);
            }
        }
    }
}