using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Keylol.Controllers.Message
{
    /// <summary>
    /// 消息 Controller（邮政中心）
    /// </summary>
    [Authorize]
    [RoutePrefix("message")]
    public partial class MessageController : KeylolApiController
    {
    }
}