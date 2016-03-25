using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取 5 篇全站最新文章
        /// </summary>
        [Route("latest")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetListOfLatest()
        {
            var articleEntries =
                await DbContext.Articles.AsNoTracking()
                    .Where(a => a.Archived == ArchivedState.None && a.Rejected == false)
                    .OrderByDescending(a => a.SequenceNumber).Take(() => 5)
                    .Select(a => new
                    {
                        article = a,
                        authorIdCode = a.Principal.User.IdCode
                    }).ToListAsync();
            return Ok(articleEntries.Select(e => new ArticleDTO(e.article, true, 100) {AuthorIdCode = e.authorIdCode}));
        }
    }
}