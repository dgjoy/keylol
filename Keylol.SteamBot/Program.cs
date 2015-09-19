using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
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
            var context = new InstanceContext(new SteamBotCoodinatorCallbackHandler());
            var client = new SteamBotCoodinatorClient(context);
            if (client.ClientCredentials != null)
            {
                client.ClientCredentials.UserName.UserName = "test1";
                client.ClientCredentials.UserName.Password = "test";
            }
            var bots = client.AllocateBots();
            foreach (var bot in bots)
            {
                Console.WriteLine($"Bot UserName: {bot.SteamUserName} Password: {bot.SteamPassword}");
            }
            client.UpdateBots(bots.Select(bot => new SteamBotVM {Id = bot.Id, Online = true}).ToArray());

            while (true)
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    var parts = input.Split('|');
                    if (parts.Length < 2)
                        client.BroadcastBotOnFriendAdded(parts[0]);
                    else
                        Console.WriteLine(client.BindSteamUserWithBindingToken(123, parts[0], parts[1])
                            ? "Correct"
                            : "Wrong");
                }
            }
        }
    }

    public class SteamBotCoodinatorCallbackHandler : ISteamBotCoodinatorCallback
    {
        public void DeleteSteamFriend(string botId, long steamId)
        {
            Console.WriteLine($"Bot {botId} should delete this friend: {steamId}");
        }
    }
}