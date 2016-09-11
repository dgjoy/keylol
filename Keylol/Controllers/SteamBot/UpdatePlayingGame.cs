using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Services;
using Keylol.Utilities;

namespace Keylol.Controllers.SteamBot
{
    public partial class SteamBotController
    {
        /// <summary>
        ///     设置指定机器人正在玩的游戏
        /// </summary>
        /// <param name="botSid">机器人序号列表，用逗号分隔，例如 "1,22,33"，"*" 表示所有机器人</param>
        /// <param name="appIds">App ID 列表，用逗号分隔</param>
        /// <param name="gameName">游戏名称，null 表示不指定名称</param>
        [Route("playing-game")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdatePlayingGame(string botSid, string appIds = null, string gameName = null)
        {
            var appIdList = appIds?.Split(',').Select(uint.Parse).ToList() ?? new List<uint>();
            if (botSid == "*")
            {
                foreach (var client in SteamBotCoordinator.Sessions.Values.Select(c => c.Client))
                {
                    await client.SetPlayingGame(null, appIdList, gameName);
                }
            }
            else
            {
                var sns = botSid.Split(',').Select(int.Parse);
                var bots = await _dbContext.SteamBots.Where(b => sns.Contains(b.Sid))
                    .ToListAsync();
                foreach (var bot in bots.Where(b => b.IsOnline()))
                {
                    await SteamBotCoordinator.Sessions[bot.SessionId].Client.SetPlayingGame(bot.Id, appIdList, gameName);
                }
            }
            return Ok();
        }
    }
}