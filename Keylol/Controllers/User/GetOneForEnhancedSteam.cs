using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SteamKit2;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        /// 为 Enhanced Steam 提供的 Steam 个人资料页接口
        /// </summary>
        /// <param name="steamId64">用户的 Steam ID 64</param>
        [AllowAnonymous]
        [Route("enhanced-steam")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> GetOneForEnhancedSteam(ulong steamId64)
        {
            var steamId = new SteamID(steamId64);
            var user = await _userManager.FindBySteamIdAsync(steamId.Render(true));
            if (user == null)
                return NotFound();
            return Ok(new
            {
                Link = $"https://www.keylol.com/user/{user.IdCode}"
            });
        }
    }
}