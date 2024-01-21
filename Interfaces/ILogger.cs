using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimDiscordBot.Interfaces
{
    internal interface ILogger
    {
        Task Log(string message);
    }
}
