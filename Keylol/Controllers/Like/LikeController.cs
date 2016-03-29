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
        /// <summary>
        /// 创建 LikeController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public LikeController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}