using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Like
{
    /// <summary>
    /// 认可 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("like")]
    public partial class LikeController : KeylolApiController
    {
    }
}