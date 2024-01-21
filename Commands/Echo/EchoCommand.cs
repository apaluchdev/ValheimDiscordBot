using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimDiscordBot.Commands.Echo
{
    public class EchoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back what was said")]
        public async Task ExecuteAsync([Remainder][Summary("A phrase")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Usage: /echo <phrase>");
                return;
            }

            await ReplyAsync(phrase);
        }
    }
}
