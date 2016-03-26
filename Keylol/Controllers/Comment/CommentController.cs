using System.Web.Http;

namespace Keylol.Controllers.Comment
{
    /// <summary>
    ///     评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("comment")]
    public partial class CommentController : KeylolApiController
    {
    }
}