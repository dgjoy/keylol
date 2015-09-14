using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Keylol.Controllers.API
{
    [Authorize]
    public class ArticleController : KeylolApiController
    {
        [AllowAnonymous]
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

        [AllowAnonymous]
        public async Task<IHttpActionResult> Get(string idCode, int sequenceNumber)
        {
            var article =
                await
                    DbContext.Articles.SingleOrDefaultAsync(
                        a => a.Principal.User.IdCode == idCode && a.SequenceNumberForAuthor == sequenceNumber);
            if (article == null)
                return NotFound();
            var articleDTO = new ArticleDTO(article)
            {
                AuthorIdCode = idCode
            };
            return Ok(articleDTO);
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
                article.VoteForPoint = voteForPoint;
                article.Vote = vm.Vote;
            }

            article.Type = type;
            article.Title = vm.Title;
            article.Content = vm.Content;
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(point => vm.AttachedPointsId.Contains(point.Id)).ToListAsync();
            article.RecommendedArticle = await DbContext.Articles.FindAsync(vm.RecommendedArticleId);
            article.Principal = (await UserManager.FindByIdAsync(User.Identity.GetUserId())).ProfilePoint;
            article.SequenceNumberForAuthor =
                (await
                    DbContext.Articles.Where(a => a.Principal == article.Principal)
                        .Select(a => a.SequenceNumberForAuthor)
                        .DefaultIfEmpty(0)
                        .MaxAsync()) + 1;
            await DbContext.SaveChangesAsync();
            return Created($"api/article/{article.Id}", new ArticleDTO(article, false));
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
            if (article.Principal.User.Id != User.Identity.GetUserId())
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
                article.VoteForPoint = voteForPoint;
                article.Vote = vm.Vote;
            }

            article.Type = type;
            article.Title = vm.Title;
            article.Content = vm.Content;
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(point => vm.AttachedPointsId.Contains(point.Id)).ToListAsync();
            article.RecommendedArticle = await DbContext.Articles.FindAsync(vm.RecommendedArticleId);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}