using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("invitation-code")]
    public class InvitationCodeController : KeylolApiController
    {
        /// <summary>
        /// 验证一个邀请码是否正确
        /// </summary>
        /// <param name="code">邀请码</param>
        [AllowAnonymous]
        [Route("{code}")]
        [ResponseType(typeof (InvitationCodeDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "邀请码无效")]
        public async Task<IHttpActionResult> Get(string code)
        {
            var c = await DbContext.InvitationCodes.FindAsync(code);
            if (c == null || c.UserByUserId != null)
                return NotFound();
            return Ok(new InvitationCodeDTO(c));
        }

        /// <summary>
        /// 获取邀请码列表
        /// </summary>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 50，最大 2000</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [ResponseType(typeof (List<InvitationCodeDTO>))]
        public async Task<IHttpActionResult> Get(int skip = 0, int take = 50)
        {
            if (take > 2000) take = 2000;
            return
                Ok((await DbContext.InvitationCodes.Where(c => c.UserByUserId == null)
                    .OrderBy(c => c.GenerateTime)
                    .Skip(() => skip)
                    .Take(() => take)
                    .ToListAsync()).Select(c => new InvitationCodeDTO(c, true)));
        }

        /// <summary>
        /// 生成一个邀请码
        /// </summary>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (InvitationCodeDTO))]
        public async Task<IHttpActionResult> Post()
        {
            var code = DbContext.InvitationCodes.Create();
            DbContext.InvitationCodes.Add(code);
            await DbContext.SaveChangesAsync();
            return Created($"invitation-code/{code.Id}", new InvitationCodeDTO(code, true));
        }
    }
}