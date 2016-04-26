using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Keylol.ServiceBase;
using Keylol.SteamBot.ServiceReference;
using log4net;
using SteamKit2;
using SteamKit2.Internal;

namespace Keylol.SteamBot
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SteamBotCoordinatorCallback : ISteamBotCoordinatorCallback
    {
        private readonly ILog _logger;

        public SteamBotCoordinatorCallback(ILogProvider logProvider)
        {
            _logger = logProvider.Logger;
        }

        public SteamBot SteamBot { get; set; }

        public string[] GetAllocatedBots()
        {
            return SteamBot.BotInstances?.Select(b => b.Id).ToArray();
        }

        public string[] RequestReallocateBots(int count)
        {
            _logger.Info($"Received new allocation target: {count} bot(s).");
            if (SteamBot.BotInstances == null) SteamBot.BotInstances = new List<BotInstance>(count);

            if (SteamBot.BotInstances.Count < count)
            {
                var botInfos = SteamBot.Coordinator.Operations.AllocateBots(count - SteamBot.BotInstances.Count);
                var newBots = botInfos.Select(b =>
                {
                    var botInstance = Program.Container.GetInstance<BotInstance>();
                    botInstance.Id = b.Id;
                    Debug.Assert(b.SequenceNumber != null);
                    botInstance.SequenceNumber = b.SequenceNumber.Value;
                    botInstance.LogOnDetails.Username = b.SteamUserName;
                    botInstance.LogOnDetails.Password = b.SteamPassword;
                    return botInstance;
                }).ToList();
                SteamBot.BotInstances.AddRange(newBots);
                foreach (var botInstance in newBots)
                {
                    botInstance.Start();
                }
                return null;
            }

            if (SteamBot.BotInstances.Count > count)
            {
                var deallocCount = SteamBot.BotInstances.Count - count;
                var botToDealloc = SteamBot.BotInstances.Take(deallocCount).ToList();
                SteamBot.BotInstances.RemoveRange(0, deallocCount);
                foreach (var botInstance in botToDealloc)
                {
                    botInstance.Dispose();
                }
                return botToDealloc.Select(b => b.Id).ToArray();
            }

            return null;
        }

        public void StopBot(string botId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null) return;
            SteamBot.BotInstances.Remove(botInstance);
            botInstance.Dispose();
        }

        public void AddFriend(string botId, string steamId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return;
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            botInstance.SteamFriends.AddFriend(id);
        }

        public void RemoveFriend(string botId, string steamId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return;
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            botInstance.SteamFriends.RemoveFriend(id);
        }

        public void SendChatMessage(string botId, string steamId, string message, bool logMessage)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null || string.IsNullOrEmpty(message))
                return;
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            botInstance.SteamFriends.SendChatMessage(id, EChatEntryType.ChatMsg, message);
            if (logMessage)
            {
                var friendName = botInstance.SteamFriends.GetFriendPersonaName(id);
                _logger.Info($"#{botInstance.SequenceNumber} [Chat TX] To {friendName} ({steamId}): {message}");
            }
        }

        public void BroadcastMessage(string message)
        {
            if (SteamBot.BotInstances == null || string.IsNullOrEmpty(message))
                return;
            _logger.Info($"Broadcasting chat message: {message}");
            foreach (var botInstance in SteamBot.BotInstances)
            {
                var count = botInstance.SteamFriends.GetFriendCount();
                for (var i = 0; i < count; i++)
                {
                    botInstance.SteamFriends.SendChatMessage(botInstance.SteamFriends.GetFriendByIndex(i),
                        EChatEntryType.ChatMsg, message);
                }
            }
        }

        public string GetUserAvatarHash(string botId, string steamId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return null;
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            return BitConverter.ToString(botInstance.SteamFriends.GetFriendAvatar(id))
                .Replace("-", string.Empty)
                .ToLower();
        }

        public string GetUserProfileName(string botId, string steamId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return null;
            var id = new SteamID();
            id.SetFromSteam3String(steamId);
            return botInstance.SteamFriends.GetFriendPersonaName(id);
        }

        public string[] GetFriendList(string botId)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return null;
            var count = botInstance.SteamFriends.GetFriendCount();
            var result = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                result.Add(botInstance.SteamFriends.GetFriendByIndex(i).Render(true));
            }
            return result.ToArray();
        }

        public void SetPlayingGame(string botId, int appId)
        {
            var playGameMessage = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            playGameMessage.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(appId)
            });
            if (botId == null)
            {
                if (SteamBot.BotInstances == null)
                    return;
                foreach (var bot in SteamBot.BotInstances)
                {
                    bot.SteamClient.Send(playGameMessage);
                }
                return;
            }
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            botInstance?.SteamClient.Send(playGameMessage);
        }

        public string Curl(string botId, string url)
        {
            var botInstance = SteamBot.BotInstances?.SingleOrDefault(b => b.Id == botId);
            if (botInstance == null)
                return null;
            try
            {
                var request = botInstance.CookieManager.CreateWebRequest(url);
                using (var response = request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                        return null;
                    var reader = new StreamReader(responseStream);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException)
            {
                return null;
            }
        }
    }
}