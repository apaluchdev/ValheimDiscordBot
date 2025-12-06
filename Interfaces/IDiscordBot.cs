namespace ValheimDiscordBot.Interfaces
{
    internal interface IDiscordBot
    {
        Task StartAsync();

        Task StopAsync();
    }
}
