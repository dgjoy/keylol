using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Keylol.Utilities;

namespace Keylol.Controllers.InvitationCode
{
    public partial class InvitationCodeController
    {
        /// <summary>
        ///     获取未使用的邀请码列表
        /// </summary>
        /// <param name="source">邀请码来源，不填表示获取所有来源的邀请码，默认 null</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 50，最大 2000</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpGet]
        [ResponseType(typeof (List<InvitationCodeDto>))]
        public async Task<IHttpActionResult> GetListOfUnused(string source = null, int skip = 0, int take = 50)
        {
            if (take > 2000) take = 2000;
            var query = DbContext.InvitationCodes.Where(c => c.UsedByUser == null);
            if (source != null)
                query = query.Where(c => c.Source == source);
            return
                Ok((await query.OrderBy(c => c.GenerateTime).Skip(() => skip).Take(() => take).ToListAsync())
                    .Select(c => new InvitationCodeDto
                    {
                        Id = c.Id,
                        GenerateTime = c.GenerateTime,
                        Source = c.Source
                    }));
        }
    }
}