using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.UserGameRecord
{
    public partial class UserGameRecordController
    {
        /// <summary>
        ///     获取指定用户指定游戏的在档时间
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="steamAppId">游戏 Steam App ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}/{steamAppId}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (double))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户没有该游戏的在档记录")]
        public async Task<IHttpActionResult> GetOnePlayTimeByUser(string id, int steamAppId,
            UserIdentityType idType = UserIdentityType.Id)
        {
            string userId;
            switch (idType)
            {
                case UserIdentityType.UserName:
                {
                    var user = await _userManager.FindByNameAsync(id);
                    userId = user.Id;
                    break;
                }

                case UserIdentityType.IdCode:
                {
                    var user = await _userManager.FindByIdCodeAsync(id);
                    if (user == null)
                        return NotFound();
                    userId = user.Id;
                    break;
                }

                case UserIdentityType.Id:
                    userId = id;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            var gameRecord = await _dbContext.UserGameRecords
                .Where(r => r.UserId == userId && r.SteamAppId == steamAppId)
                .SingleOrDefaultAsync();
            if (gameRecord == null)
                return NotFound();

            return Ok(gameRecord.TotalPlayedTime);
        }
    }
}