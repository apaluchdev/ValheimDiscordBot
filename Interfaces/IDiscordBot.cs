
using Microsoft.Extensions.DependencyInjection;

namespace ValheimDiscordBot.Interfaces
{
    internal interface IDiscordBot
    {
        Task StartAsync(ServiceProvider serviceProvider);

        Task StopAsync();
    }
}
