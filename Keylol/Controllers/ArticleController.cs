using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("article")]
    public class ArticleController : KeylolApiController
    {
        /// <summary>
        /// 根据 ID 取得一篇文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        [Route("{id}")]
        [ResponseType(typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        public async Task<IHttpActionResult> Get(string id)
        {
            var userId = User.Identity.GetUserId();
            var articleEntry = await DbContext.Articles.Where(a => a.Id == id).Select(
                a =>
                    new
                    {
                        article = a,
                        likeCount = a.Likes.Count(l => l.Backout == false),
                        liked = a.Likes.Any(l => l.OperatorId == userId && l.Backout == false),
                        typeName = a.Type.Name,
                        attachedPoints = a.AttachedPoints,
                        authorIdCode = a.Principal.User.IdCode
                    })
                .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article)
            {
                AuthorIdCode = articleEntry.authorIdCode,
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.typeName,
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            return Ok(articleDTO);
        }

        /// <summary>
        /// 根据作者和文章序号取得一篇文章
        /// </summary>
        /// <param name="authorIdCode">作者 IdCode</param>
        /// <param name="sequenceNumberForAuthor">文章序号</param>
        [Route("{authorIdCode}/{sequenceNumberForAuthor}")]
        [ResponseType(typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        public async Task<IHttpActionResult> Get(string authorIdCode, int sequenceNumberForAuthor)
        {
            var userId = User.Identity.GetUserId();
            var articleEntry =
                await
                    DbContext.Articles.Where(a =>
                        a.Principal.User.IdCode == authorIdCode &&
                        a.SequenceNumberForAuthor == sequenceNumberForAuthor)
                        .Select(a => new
                        {
                            article = a,
                            likeCount = a.Likes.Count(l => l.Backout == false),
                            liked = a.Likes.Any(l => l.OperatorId == userId && l.Backout == false),
                            typeName = a.Type.Name,
                            attachedPoints = a.AttachedPoints
                        })
                        .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article)
            {
                AuthorIdCode = authorIdCode,
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.typeName,
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            return Ok(articleDTO);
        }

        /// <summary>
        /// 根据关键字搜索对应文章
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> Get(string keyword, int skip = 0, int take = 5)
        {
            if (take > 50) take = 50;
            return Ok((await DbContext.Articles.SqlQuery(@"SELECT * FROM [dbo].[Entries] AS [t1] INNER JOIN (
	                SELECT * FROM CONTAINSTABLE([dbo].[Entries], ([Title], [Content]), {0})
	            ) AS [t2] ON [t1].[Id] = [t2].[KEY]
	            ORDER BY [t2].[RANK] DESC
	            OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync()).Select(
                    article => new ArticleDTO(article) {AuthorIdCode = article.Principal.User.IdCode}));
        }

        /// <summary>
        /// 创建一篇文章
        /// </summary>
        /// <param name="vm">文章相关属性</param>
        [ClaimsAuthorize(StatusClaim.ClaimType, StatusClaim.Normal)]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (ArticleDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Post(ArticlePostVM vm)
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

            if (type.AllowVote && vm.VoteForPointId != null)
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
            article.SequenceNumberForAuthor = (await DbContext.Articles.Where(a => a.PrincipalId == article.PrincipalId)
                .Select(a => a.SequenceNumberForAuthor)
                .DefaultIfEmpty(0)
                .MaxAsync()) + 1;
            DbContext.Articles.Add(article);
            await DbContext.SaveChangesAsync();
            return Created($"article/{article.Id}", new ArticleDTO(article, false));
        }

        /// <summary>
        /// 编辑指定文章
        /// </summary>
        /// <param name="id">文章 Id</param>
        /// <param name="vm">文章相关属性，其中 Title, Content, TypeId 如果不提交表示不修改</param>
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "当前用户无权编辑这篇文章")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> Put(string id, ArticlePutVM vm)
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

            ArticleType type;
            if (vm.TypeId != null)
            {
                type = await DbContext.ArticleTypes.FindAsync(vm.TypeId);
                if (type == null)
                {
                    ModelState.AddModelError("vm.TypeId", "Invalid article type.");
                    return BadRequest(ModelState);
                }
                article.TypeId = vm.TypeId;
            }
            else
            {
                type = article.Type;
            }

            if (type.AllowVote && vm.VoteForPointId != null)
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
            else
            {
                article.VoteForPoint = null;
                article.Vote = null;
            }

            DbContext.EditLogs.Add(new EditLog
            {
                ArticleId = article.Id,
                EditorId = editorId,
                OldContent = article.Content,
                OldTitle = article.Title
            });
            if (vm.Title != null)
                article.Title = vm.Title;
            if (vm.Content != null)
                article.Content = vm.Content;
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(point => vm.AttachedPointsId.Contains(point.Id)).ToListAsync();
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}