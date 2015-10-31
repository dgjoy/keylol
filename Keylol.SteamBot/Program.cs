using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.SteamBot
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                var service = new SteamBotService();
                service.ConsoleStartup(args);
            }
            else
            {
                var servicesToRun = new ServiceBase[]
                {
                    new SteamBotService()
                };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}