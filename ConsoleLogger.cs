using ValheimDiscordBot.Interfaces;

namespace ValheimDiscordBot
{
    internal class ConsoleLogger : ILogger
    {
        public async Task Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToShortTimeString} - {message}");
        }
    }
}
