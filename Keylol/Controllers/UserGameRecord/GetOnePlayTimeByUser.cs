using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using Keylol.Controllers.NormalPoint;
using Keylol.Controllers.User;
using Keylol.Models;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.UserGameRecord
{
    public partial class UserGameRecordController
    {
        /// <summary>
        /// 获取指定用户指定游戏的在档时间
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="steamAppId">游戏 Steam App ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}/{steamAppId}")]
        [HttpGet]
        [ResponseType(typeof (double))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户没有该游戏的在档记录")]
        public async Task<IHttpActionResult> GetOnePlayTimeByUser(string id, int steamAppId,
            UserController.IdType idType = UserController.IdType.Id)
        {
            string userId;
            switch (idType)
            {
                case UserController.IdType.UserName:
                {
                    var user = await DbContext.Users.SingleAsync(u => u.UserName == id);
                    userId = user.Id;
                    break;
                }

                case UserController.IdType.IdCode:
                {
                    var user = await DbContext.Users.SingleAsync(u => u.IdCode == id);
                    userId = user.Id;
                    break;
                }

                case UserController.IdType.Id:
                    userId = id;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            var gameRecord = await DbContext.UserGameRecords
                .Where(r => r.UserId == userId && r.SteamAppId == steamAppId)
                .SingleOrDefaultAsync();
            if (gameRecord == null)
                return NotFound();

            return Ok(gameRecord.TotalPlayedTime);
        }
    }
}