using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers
{
    [Authorize]
    public class ArticleController : KeylolApiController
    {
        public async Task<IHttpActionResult> Get(string id)
        {
            var article = await DbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();
            var articleDTO = new ArticleDTO(article)
            {
                AuthorIdCode = article.Principal.User.IdCode
            };
            return Ok(articleDTO);
        }

        public async Task<IHttpActionResult> Get(string authorIdCode, int sequenceNumberForAuthor)
        {
            var article =
                await
                    DbContext.Articles.SingleOrDefaultAsync(
                        a =>
                            a.Principal.User.IdCode == authorIdCode &&
                            a.SequenceNumberForAuthor == sequenceNumberForAuthor);
            if (article == null)
                return NotFound();
            var articleDTO = new ArticleDTO(article)
            {
                AuthorIdCode = authorIdCode
            };
            return Ok(articleDTO);
        }

        public async Task<IHttpActionResult> Get(string keyword, int skip = 0, int take = 5)
        {
            return Ok((await DbContext.Articles.SqlQuery(@"SELECT * FROM [dbo].[Entries] AS [t1] INNER JOIN (
	                SELECT * FROM CONTAINSTABLE([dbo].[Entries], ([Title], [Content]), {0})
	            ) AS [t2] ON [t1].[Id] = [t2].[KEY]
	            ORDER BY [t2].[RANK] DESC
	            OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync()).Select(
                    article => new ArticleDTO(article) {AuthorIdCode = article.Principal.User.IdCode}));
        }

        [ClaimsAuthorize(StatusClaim.ClaimType, StatusClaim.Normal)]
        public async Task<IHttpActionResult> Post(ArticleVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var type = await DbContext.ArticleTypes.FindAsync(vm.TypeId);
            if (type == null)
            {
                ModelState.AddModelError("vm.TypeId", "Invalid article type.");
                return BadRequest(ModelState);
            }

            var article = DbContext.Articles.Create();

            if (type.AllowVote && vm.Vote != null)
            {
                var voteForPoint = await DbContext.NormalPoints.FindAsync(vm.VoteForPointId);
                if (voteForPoint == null)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Invalid point for vote.");
                    return BadRequest(ModelState);
                }
                if (voteForPoint.Type != NormalPointType.Game)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Point for vote is not a game point.");
                    return BadRequest(ModelState);
                }
                article.VoteForPointId = voteForPoint.Id;
                article.Vote = vm.Vote;
            }

            article.Type = type;
            article.Title = vm.Title;
            article.Content = vm.Content;
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(point => vm.AttachedPointsId.Contains(point.Id)).ToListAsync();
            article.Principal = (await UserManager.FindByIdAsync(User.Identity.GetUserId())).ProfilePoint;
            article.SequenceNumberForAuthor =
                (await
                    DbContext.Articles.Where(a => a.PrincipalId == article.PrincipalId)
                        .Select(a => a.SequenceNumberForAuthor)
                        .DefaultIfEmpty(0)
                        .MaxAsync()) + 1;
            DbContext.Articles.Add(article);
            await DbContext.SaveChangesAsync();
            return Created($"article/{article.Id}", new ArticleDTO(article, false));
        }

        public async Task<IHttpActionResult> Put(string id, ArticleVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await DbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var editorId = User.Identity.GetUserId();
            if (article.PrincipalId != editorId)
                return Unauthorized();

            var type = await DbContext.ArticleTypes.FindAsync(vm.TypeId);
            if (type == null)
            {
                ModelState.AddModelError("vm.TypeId", "Invalid article type.");
                return BadRequest(ModelState);
            }

            if (type.AllowVote && vm.Vote != null)
            {
                var voteForPoint = await DbContext.NormalPoints.FindAsync(vm.VoteForPointId);
                if (voteForPoint == null)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Invalid point for vote.");
                    return BadRequest(ModelState);
                }
                if (voteForPoint.Type != NormalPointType.Game)
                {
                    ModelState.AddModelError("vm.VoteForPointId", "Point for vote is not a game point.");
                    return BadRequest(ModelState);
                }
                article.VoteForPointId = voteForPoint.Id;
                article.Vote = vm.Vote;
            }

            DbContext.EditLogs.Add(new EditLog
            {
                ArticleId = article.Id,
                EditorId = editorId,
                OldContent = article.Content,
                OldTitle = article.Title
            });
            article.Type = type;
            article.Title = vm.Title;
            article.Content = vm.Content;
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(point => vm.AttachedPointsId.Contains(point.Id)).ToListAsync();
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}