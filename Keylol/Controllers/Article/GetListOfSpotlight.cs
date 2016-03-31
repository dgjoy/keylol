using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取 5 篇最新全站萃选文章
        /// </summary>
        [Route("spotlight")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDto>))]
        public async Task<IHttpActionResult> GetListOfSpotlight()
        {
            var articleEntries =
                await DbContext.Articles.AsNoTracking()
                    .Where(a => a.SpotlightTime >= DbFunctions.AddDays(DateTime.Now, -14))
                    .OrderByDescending(a => a.SpotlightTime).Take(() => 5)
                    .Select(a => new
                    {
                        article = a,
                        authorIdCode = a.Principal.User.IdCode,
                        commentCount = a.Comments.Count,
                        likeCount = a.Likes.Count
                    }).ToListAsync();
            return Ok(articleEntries.Select(e => new ArticleDto(e.article, true, 100)
            {
                AuthorIdCode = e.authorIdCode,
                CommentCount = e.commentCount,
                LikeCount = e.likeCount
            }));
        }
    }
}