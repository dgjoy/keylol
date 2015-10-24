using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("article-type")]
    public class ArticleTypeController : KeylolApiController
    {
        [Route]
        [ResponseType(typeof(List<ArticleTypeDTO>))]
        public async Task<IHttpActionResult> Get()
        {
            return
                Ok((await DbContext.ArticleTypes.ToListAsync()).Select(articleType => new ArticleTypeDTO(articleType)));
        }
    }
}