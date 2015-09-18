using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Keylol.SteamBot.CoodinatorService;

namespace Keylol.SteamBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var context = new InstanceContext(new SteamBotCoodinatorCallbackHandler());
                var client = new SteamBotCoodinatorClient(context);
                if (client.ClientCredentials != null)
                {
                    client.ClientCredentials.UserName.UserName = "test1";
                    client.ClientCredentials.UserName.Password = "test";
                }
                var result = await client.TestAsync("my string");
                Console.WriteLine($"Result: {result}");
            }).Wait();
        }
    }

    public class SteamBotCoodinatorCallbackHandler : ISteamBotCoodinatorCallback
    {
        public void DeleteSteamFriend(string botId, long steamId)
        {
            Console.WriteLine($"{botId} {steamId}");
        }

        public void TestCallback(string message)
        {
            Console.WriteLine($"TestCallback: {message}");
        }
    }
}
