using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        /// 针对 SteamCN 多格，获取最新的九篇文章
        /// </summary>
        [AllowAnonymous]
        [Route("steamcn")]
        [HttpGet]
        public async Task<IHttpActionResult> GetListForSteamCn()
        {
            var articles = await _dbContext.Articles
                .Where(a => a.Archived == ArchivedState.None && a.Rejected == false)
                .OrderByDescending(a => a.Sid)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Subtitle,
                    a.PublishTime,
                    a.SidForAuthor,
                    PointChineseName = a.TargetPoint.ChineseName,
                    PointEnglishName = a.TargetPoint.EnglishName,
                    AuthorUserName = a.Author.UserName,
                    AuthorIdCode = a.Author.IdCode
                })
                .Take(9)
                .ToListAsync();
            var result = new List<object>(articles.Count);
            foreach (var a in articles)
            {
                result.Add(new
                {
                    a.Id,
                    a.Title,
                    a.Subtitle,
                    PublishTime = a.PublishTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    a.PointChineseName,
                    a.PointEnglishName,
                    a.SidForAuthor,
                    a.AuthorUserName,
                    a.AuthorIdCode,
                    LikeCount = await _cachedData.Likes.GetTargetLikeCountAsync(a.Id, LikeTargetType.Article),
                    CommentCount = await _cachedData.ArticleComments.GetArticleCommentCountAsync(a.Id)
                });
            }
            return Ok(result);
        }
    }
}