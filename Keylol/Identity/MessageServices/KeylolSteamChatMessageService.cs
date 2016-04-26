using System.Threading.Tasks;
using System.Timers;
using Keylol.Models.DAL;
using Keylol.Services;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Identity.MessageServices
{
    /// <summary>
    ///     Steam chat message provider for ASP.NET Identity
    /// </summary>
    public class KeylolSteamChatMessageService : IIdentityMessageService
    {
        /// <summary>
        ///     This method should send the message
        /// </summary>
        /// <param name="message" />
        /// <returns />
        public async Task SendAsync(IdentityMessage message)
        {
            var parts = message.Subject.Split(',');
            var botId = parts[0];
            var tempSilence = bool.Parse(parts[1]);
            var dbContext = Startup.Container.GetInstance<KeylolDbContext>();
            var bot = await dbContext.SteamBots.FindAsync(botId);
            if (!bot.IsOnline())
                return;
            if (tempSilence)
            {
                if (SteamBotCoordinator.AutoChatDisabledBots.ContainsKey(botId))
                {
                    var timer = SteamBotCoordinator.AutoChatDisabledBots[botId];
                    timer.Stop();
                    timer.Start();
                }
                else
                {
                    var timer = new Timer(120000) {AutoReset = false};
                    timer.Elapsed +=
                        (sender, args) => { SteamBotCoordinator.AutoChatDisabledBots.TryRemove(botId, out timer); };
                    timer.Start();
                    SteamBotCoordinator.AutoChatDisabledBots[botId] = timer;
                }
            }
            else if (SteamBotCoordinator.AutoChatDisabledBots.ContainsKey(botId))
            {
                Timer timer;
                if (SteamBotCoordinator.AutoChatDisabledBots.TryRemove(botId, out timer))
                {
                    timer.Stop();
                }
            }
            await SteamBotCoordinator.Sessions[bot.SessionId]
                .Client.SendChatMessage(bot.Id, message.Destination, message.Body, true);
        }
    }
}