using System.Web.Http;

namespace Keylol.Controllers.Comment
{
    [Authorize]
    [RoutePrefix("comment")]
    public partial class CommentController : KeylolApiController
    {
    }
}