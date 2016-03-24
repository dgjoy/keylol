using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Message
{
    public partial class MessageController
    {
        /// <summary>
        /// 获取当前登录用户的邮政消息
        /// </summary>
        [Route]
        [HttpGet]
        public async Task<IHttpActionResult> GetListByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            return Ok(await DbContext.Messages.ToListAsync());
        }
    }
}