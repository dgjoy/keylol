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
        ///     为指定机器人添加免费 License
        /// </summary>
        /// <param name="botSid">机器人序号列表，用逗号分隔，例如 "1,22,33"，"*" 表示所有机器人</param>
        /// <param name="appIds">App ID 列表，用逗号分隔</param>
        [Route("add-license")]
        [HttpPost]
        public async Task<IHttpActionResult> AddLicense(string botSid, string appIds)
        {
            var appIdList = appIds.Split(',').Select(uint.Parse).ToList();
            if (botSid == "*")
            {
                foreach (var client in SteamBotCoordinator.Sessions.Values.Select(c => c.Client))
                {
                    await client.AddLicense(null, appIdList);
                }
            }
            else
            {
                var sns = botSid.Split(',').Select(int.Parse);
                var bots = await _dbContext.SteamBots.Where(b => sns.Contains(b.Sid))
                    .ToListAsync();
                foreach (var bot in bots.Where(b => b.IsOnline()))
                {
                    await SteamBotCoordinator.Sessions[bot.SessionId].Client.AddLicense(bot.Id, appIdList);
                }
            }
            return Ok();
        }
    }
}