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
                Debug.WriteLine("Connected to Discord");

                do
                {
                  await Task.Delay(5000);
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