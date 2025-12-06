using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    internal class ConsoleLogger : ILogger
    {
        public Task Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
            return Task.CompletedTask;
        }
    }
}
