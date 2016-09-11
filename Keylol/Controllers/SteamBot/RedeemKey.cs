using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Services;

namespace Keylol.Controllers.SteamBot
{
    public partial class SteamBotController
    {
        /// <summary>
        ///     为指定机器人兑换 CD Key
        /// </summary>
        /// <param name="botSid">机器人序号</param>
        /// <param name="cdKey">CD Key</param>
        [Route("redeem-key")]
        [HttpPost]
        public async Task<IHttpActionResult> RedeemKey(int botSid, string cdKey)
        {
            var bot = await _dbContext.SteamBots.Where(b => b.Sid == botSid).SingleAsync();
            await SteamBotCoordinator.Sessions[bot.SessionId].Client.RedeemKey(bot.Id, cdKey);
            return Ok();
        }
    }
}