using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize(Users = "Stackia")]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="DatabaseMigrationController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public DatabaseMigrationController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 添加一个据点职员
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="staffId">职员 ID</param>
        [Route("add-point-staff")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPointStaff(string pointId, string staffId)
        {
            _dbContext.PointStaff.Add(new PointStaff
            {
                PointId = pointId,
                StaffId = staffId
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 重新提取所有文章的 UnstyledContent
        /// </summary>
        [Route("article-unstyled-content")]
        [HttpPost]
        public async Task<IHttpActionResult> ArticleUnstyledContent()
        {
            var articles = await _dbContext.Articles.ToListAsync();
            Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
            foreach (var article in articles)
            {
                article.Content = CQ.Create(article.Content).Render();
                article.UnstyledContent = PlainTextFormatter.FlattenHtml(article.Content, true);
                await _dbContext.SaveChangesAsync();
            }
            return Ok("迁移成功");
        }

        /// <summary>
        /// 重新提取所有文章评论的 UnstyledContent
        /// </summary>
        [Route("article-comment-unstyled-content")]
        [HttpPost]
        public async Task<IHttpActionResult> ArticleCommentUnstyledContent()
        {
            var comments = await _dbContext.ArticleComments.ToListAsync();
            Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
            foreach (var comment in comments)
            {
                comment.UnstyledContent = PlainTextFormatter.FlattenHtml(comment.Content, false);
                await _dbContext.SaveChangesAsync();
            }
            return Ok("迁移成功");
        }
    }
}