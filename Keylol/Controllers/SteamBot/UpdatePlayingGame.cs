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
        /// 设置指定机器人正在玩的游戏
        /// </summary>
        /// <param name="botSequenceNumbers">机器人序号列表，用逗号分隔，例如 "1,22,33"，"*" 表示所有机器人</param>
        /// <param name="appId">App ID，0 表示停止游戏</param>
        [Route("playing-game")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdatePlayingGame(string botSequenceNumbers, int appId)
        {
            if (botSequenceNumbers == "*")
            {
                foreach (var client in SteamBotCoordinator.Sessions.Values.Select(c => c.Client))
                {
                    await client.SetPlayingGame(null, appId);
                }
            }
            else
            {
                var sns = botSequenceNumbers.Split(',').Select(int.Parse);
                var bots = await _dbContext.SteamBots.Where(b => sns.Contains(b.SequenceNumber))
                    .ToListAsync();
                foreach (var bot in bots.Where(b => b.IsOnline()))
                {
                    await SteamBotCoordinator.Sessions[bot.SessionId].Client.SetPlayingGame(bot.Id, appId);
                }
            }
            return Ok();
        }
    }
}