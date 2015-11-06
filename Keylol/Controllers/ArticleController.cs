using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CsQuery;
using CsQuery.Output;
using Ganss.XSS;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers
{
    [Authorize]
    [RoutePrefix("article")]
    public class ArticleController : KeylolApiController
    {
        private static readonly object ArticleSaveLock = new object();

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
                        authorIdCode = a.Principal.User.IdCode,
                        voteForPoint = a.VoteForPoint
                    })
                .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article, true)
            {
                AuthorIdCode = articleEntry.authorIdCode,
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.typeName,
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            if (articleEntry.voteForPoint != null)
                switch (articleEntry.voteForPoint.PreferedName)
                {
                    case PreferedNameType.Chinese:
                        articleDTO.VoteForPointName = articleEntry.voteForPoint.ChineseName;
                        break;

                    case PreferedNameType.English:
                        articleDTO.VoteForPointName = articleEntry.voteForPoint.EnglishName;
                        break;
                }
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
                            attachedPoints = a.AttachedPoints,
                            voteForPoint = a.VoteForPoint
                        })
                        .SingleOrDefaultAsync();
            if (articleEntry == null)
                return NotFound();
            var articleDTO = new ArticleDTO(articleEntry.article, true)
            {
                AttachedPoints = articleEntry.attachedPoints.Select(point => new NormalPointDTO(point, true)).ToList(),
                TypeName = articleEntry.typeName,
                LikeCount = articleEntry.likeCount,
                Liked = articleEntry.liked
            };
            if (articleEntry.voteForPoint != null)
                switch (articleEntry.voteForPoint.PreferedName)
                {
                    case PreferedNameType.Chinese:
                        articleDTO.VoteForPointName = articleEntry.voteForPoint.ChineseName;
                        break;

                    case PreferedNameType.English:
                        articleDTO.VoteForPointName = articleEntry.voteForPoint.EnglishName;
                        break;
                }
            return Ok(articleDTO);
        }

        /// <summary>
        /// 获取 5 篇全站热门文章
        /// </summary>
        [Route("hot")]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetHot()
        {
            var articleEntries =
                await DbContext.Articles.AsNoTracking()
                    .Where(a => a.PublishTime >= DbFunctions.AddDays(DateTime.Now, -14))
                    .OrderByDescending(a => a.Likes.Count(l => l.Backout == false)).Take(() => 5)
                    .Select(a => new
                    {
                        article = a,
                        authorIdCode = a.Principal.User.IdCode
                    }).ToListAsync();
            return Ok(articleEntries.Select(e => new ArticleDTO(e.article, true, 256) {AuthorIdCode = e.authorIdCode}));
        }

        /// <summary>
        /// 获取指定据点时间轴的文章
        /// </summary>
        /// <param name="normalPointId">据点 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("point/{normalPointId}")]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetByNormalPointId(string normalPointId,
            NormalPointController.IdType idType, string articleTypeFilter = null, int beforeSN = int.MaxValue,
            int take = 30)
        {
            if (take > 50) take = 50;
            var articleQuery = DbContext.Articles.AsNoTracking().Where(a => a.SequenceNumber < beforeSN);
            switch (idType)
            {
                case NormalPointController.IdType.Id:
                    articleQuery = articleQuery.Where(a => a.AttachedPoints.Select(p => p.Id).Contains(normalPointId));
                    break;

                case NormalPointController.IdType.IdCode:
                    articleQuery =
                        articleQuery.Where(a => a.AttachedPoints.Select(p => p.IdCode).Contains(normalPointId));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }
            if (articleTypeFilter != null)
            {
                var typesName = articleTypeFilter.Split(',').Select(s => s.Trim()).ToList();
                articleQuery =
                    articleQuery.Where(PredicateBuilder.Contains<Article, string>(typesName, a => a.Type.Name));
            }
            var articleEntries = await articleQuery.OrderByDescending(a => a.SequenceNumber).Take(() => take).Select(
                a => new
                {
                    article = a,
                    likeCount = a.Likes.Count(l => l.Backout == false),
                    commentCount = a.Comments.Count,
                    typeName = a.Type.Name,
                    author = a.Principal.User
                }).ToListAsync();
            return Ok(articleEntries.Select(entry =>
                new ArticleDTO(entry.article, true, 256, true)
                {
                    LikeCount = entry.likeCount,
                    CommentCount = entry.commentCount,
                    TypeName = entry.typeName,
                    Author = new UserDTO(entry.author)
                }));
        }

        /// <summary>
        /// 获取指定用户时间轴的文章
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("user/{userId}")]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetByUserId(string userId, UserController.IdType idType,
            string articleTypeFilter = null, int beforeSN = int.MaxValue, int take = 30)
        {
            if (take > 50) take = 50;
            IQueryable<KeylolUser> userQuery;
            switch (idType)
            {
                case UserController.IdType.Id:
                    userQuery = DbContext.Users.AsNoTracking().Where(u => u.Id == userId);
                    break;

                case UserController.IdType.IdCode:
                    userQuery = DbContext.Users.AsNoTracking().Where(u => u.IdCode == userId);
                    break;

                case UserController.IdType.UserName:
                    userQuery = DbContext.Users.AsNoTracking().Where(u => u.UserName == userId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }
            var articleQuery = userQuery.SelectMany(u => u.ProfilePoint.Entries.OfType<Article>())
                .Where(a => a.SequenceNumber < beforeSN)
                .Select(a => new
                {
                    article = a,
                    reason = ArticleDTO.TimelineReasonType.Publish,
                    author = (KeylolUser) null
                })
                .Concat(userQuery.SelectMany(u => u.Likes.OfType<ArticleLike>())
                    .Where(l => l.Backout == false && l.Article.SequenceNumber < beforeSN)
                    .Select(l => new
                    {
                        article = l.Article,
                        reason = ArticleDTO.TimelineReasonType.Like,
                        author = l.Article.Principal.User
                    }));
            if (articleTypeFilter != null)
            {
                var typesName = articleTypeFilter.Split(',').Select(s => s.Trim()).ToList();
                articleQuery = articleQuery.Where(PredicateBuilder.Contains(typesName, a => a.article.Type.Name, new
                {
                    article = (Article) null,
                    reason = ArticleDTO.TimelineReasonType.Like,
                    author = (KeylolUser) null
                }));
            }
            var articleEntries = await articleQuery.GroupBy(e => e.article)
                .OrderByDescending(g => g.Key.SequenceNumber).Take(() => take)
                .Select(g => new
                {
                    article = g.Key,
                    candicates = g,
                    reason = g.Max(ee => ee.reason)
                })
                .Select(g => new
                {
                    g.article,
                    g.reason,
                    g.candicates.FirstOrDefault(e => e.reason == g.reason).author,
                    likeCount = g.article.Likes.Count(l => l.Backout == false),
                    commentCount = g.article.Comments.Count,
                    typeName = g.article.Type.Name
                })
                .ToListAsync();
            return Ok(articleEntries.Select(entry =>
            {
                var articleDTO = new ArticleDTO(entry.article, true, 256, true)
                {
                    TimelineReason = entry.reason,
                    LikeCount = entry.likeCount,
                    CommentCount = entry.commentCount,
                    TypeName = entry.typeName
                };
                if (entry.reason != ArticleDTO.TimelineReasonType.Publish)
                {
                    articleDTO.Author = new UserDTO(entry.author);
                }
                return articleDTO;
            }));
        }

        /// <summary>
        /// 获取当前用户主订阅时间轴的文章
        /// </summary>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("subscription")]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetBySubscription(string articleTypeFilter = null,
            int beforeSN = int.MaxValue, int take = 30)
        {
            if (take > 50) take = 50;

            var userId = User.Identity.GetUserId();
            var userQuery = DbContext.Users.AsNoTracking().Where(u => u.Id == userId);
            var profilePointsQuery = userQuery.SelectMany(u => u.SubscribedPoints.OfType<ProfilePoint>());

            var articleQuery =
                userQuery.SelectMany(u => u.SubscribedPoints.OfType<NormalPoint>())
                    .SelectMany(p => p.Articles)
                    .Where(a => a.SequenceNumber < beforeSN)
                    .Select(a => new
                    {
                        article = a,
                        reason = ArticleDTO.TimelineReasonType.Point
                    })
                    .Concat(profilePointsQuery.SelectMany(p => p.Entries.OfType<Article>())
                        .Where(a => a.SequenceNumber < beforeSN)
                        .Select(a => new
                        {
                            article = a,
                            reason = ArticleDTO.TimelineReasonType.Publish
                        }))
                    .Concat(profilePointsQuery.Select(p => p.User)
                        .SelectMany(u => u.Likes.OfType<ArticleLike>())
                        .Where(l => l.Backout == false && l.Article.SequenceNumber < beforeSN)
                        .Select(l => new
                        {
                            article = l.Article,
                            reason = ArticleDTO.TimelineReasonType.Like
                        }));

            if (articleTypeFilter != null)
            {
                var typesName = articleTypeFilter.Split(',').Select(s => s.Trim()).ToList();
                articleQuery = articleQuery.Where(PredicateBuilder.Contains(typesName, a => a.article.Type.Name, new
                {
                    article = (Article) null,
                    reason = ArticleDTO.TimelineReasonType.Like
                }));
            }

            var articleEntries = await articleQuery.GroupBy(e => e.article)
                .OrderByDescending(g => g.Key.SequenceNumber).Take(() => take)
                .Select(g => new
                {
                    article = g.Key,
                    reason = g.Max(ee => ee.reason)
                })
                .Select(g => new
                {
                    g.article,
                    g.reason,
                    likedByUsers = g.article.Likes.Select(l => l.Operator),
                    attachedPoints = g.article.AttachedPoints,
                    author = g.article.Principal.User,
                    likeCount = g.article.Likes.Count(l => l.Backout == false),
                    commentCount = g.article.Comments.Count,
                    typeName = g.article.Type.Name
                })
                .ToListAsync();

            return Ok(articleEntries.Select(entry =>
            {
                var articleDTO = new ArticleDTO(entry.article, true, 256, true)
                {
                    TimelineReason = entry.reason,
                    LikeCount = entry.likeCount,
                    CommentCount = entry.commentCount,
                    TypeName = entry.typeName,
                    Author = new UserDTO(entry.author)
                };
                switch (entry.reason)
                {
                    case ArticleDTO.TimelineReasonType.Point:
                        articleDTO.AttachedPoints =
                            entry.attachedPoints.Select(p => new NormalPointDTO(p, true)).ToList();
                        break;

                    case ArticleDTO.TimelineReasonType.Like:
                        articleDTO.LikeByUsers = entry.likedByUsers.Select(u => new UserDTO(u)).ToList();
                        break;
                }
                return articleDTO;
            }));
        }

        /// <summary>
        /// 根据关键字搜索对应文章
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="full">是否获取完整的文章信息，包括文章概要、作者等，默认 false</param>
        /// <param name="skip">起始位置</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route("keyword/{keyword}")]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetByKeyword(string keyword, bool full = false, int skip = 0, int take = 5)
        {
            if (take > 50) take = 50;

            if (!full)
                return Ok(await DbContext.Database.SqlQuery<ArticleDTO>(@"SELECT
	                [t3].[Id],
	                [t3].[PublishTime],
	                [t3].[Title],
	                [t3].[SequenceNumberForAuthor],
	                [t3].[SequenceNumber],
	                [t3].[AuthorIdCode],
	                (SELECT
                        COUNT(1)
                        FROM [dbo].[Comments]
                        WHERE [t3].[Id] = [dbo].[Comments].[ArticleId]) AS [CommentCount],
	                (SELECT
                        COUNT(1)
                        FROM [dbo].[Likes]
                        WHERE ([dbo].[Likes].[Discriminator] = N'ArticleLike') AND ([t3].[Id] = [dbo].[Likes].[ArticleId]) AND (0 = [dbo].[Likes].[Backout])) AS [LikeCount]
	                FROM (SELECT
		                [t1].*,
		                [t4].[IdCode] AS [AuthorIdCode]
		                FROM [dbo].[Entries] AS [t1]
		                INNER JOIN (SELECT * FROM CONTAINSTABLE([dbo].[Entries], ([Title], [Content]), {0})) AS [t2] ON [t1].[Id] = [t2].[KEY]
                        LEFT OUTER JOIN [dbo].[KeylolUsers] AS [t4] ON [t4].[Id] = [t1].[PrincipalId]
                        ORDER BY [t2].[RANK] DESC, [t1].[SequenceNumber] DESC
                        OFFSET({1}) ROWS FETCH NEXT({2}) ROWS ONLY) AS [t3]",
                    $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync());

            var articles = (await DbContext.Database.SqlQuery<ArticleDTO>(@"SELECT
	            [t3].[Id],
	            [t3].[PublishTime],
	            [t3].[Title],
	            [t3].[UnstyledContent] AS [Content],
	            [t3].[SequenceNumberForAuthor],
	            [t3].[SequenceNumber],
	            [t3].[TypeName],
	            [t3].[AuthorId],
	            [t3].[AuthorIdCode],
	            [t3].[AuthorUserName],
	            [t3].[AuthorAvatarImage],
	            [t3].[AuthorProfilePointBackgroundImage],
	            (SELECT
                    COUNT(1)
                    FROM [dbo].[Comments]
                    WHERE [t3].[Id] = [dbo].[Comments].[ArticleId]) AS [CommentCount],
	            (SELECT
                    COUNT(1)
                    FROM [dbo].[Likes]
                    WHERE ([dbo].[Likes].[Discriminator] = N'ArticleLike') AND ([t3].[Id] = [dbo].[Likes].[ArticleId]) AND (0 = [dbo].[Likes].[Backout])) AS [LikeCount]
	            FROM (SELECT
		            [t1].*,
		            [t4].[Name] AS [TypeName],
		            [t5].[Id] AS [AuthorId],
		            [t5].[IdCode] AS [AuthorIdCode],
		            [t5].[UserName] AS [AuthorUserName],
		            [t5].[AvatarImage] AS [AuthorAvatarImage],
		            [t6].[BackgroundImage] AS [AuthorProfilePointBackgroundImage]
		            FROM [dbo].[Entries] AS [t1]
		            INNER JOIN (SELECT * FROM CONTAINSTABLE([dbo].[Entries], ([Title], [Content]), {0})) AS [t2] ON [t1].[Id] = [t2].[KEY]
                    LEFT OUTER JOIN [dbo].[ArticleTypes] AS [t4] ON [t1].[TypeId] = [t4].[Id]
                    LEFT OUTER JOIN [dbo].[KeylolUsers] AS [t5] ON [t5].[Id] = [t1].[PrincipalId]
                    LEFT OUTER JOIN [dbo].[ProfilePoints] AS [t6] ON [t6].[Id] = [t1].[PrincipalId]
                    ORDER BY [t2].[RANK] DESC, [t1].[SequenceNumber] DESC
                    OFFSET({1}) ROWS FETCH NEXT({2}) ROWS ONLY) AS [t3]",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync())
                .Select(a => a.UnflattenAuthor().TruncateContent(256))
                .ToList();

            for (var i = 1; i < articles.Count(); i++)
                articles[i].AuthorProfilePointBackgroundImage = null;

            return Ok(articles);
        }

        /// <summary>
        /// 创建一篇文章
        /// </summary>
        /// <param name="vm">文章相关属性</param>
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

            var type = await DbContext.ArticleTypes.Where(t => t.Name == vm.TypeName).SingleOrDefaultAsync();
            if (type == null)
            {
                ModelState.AddModelError("vm.TypeName", "Invalid article type.");
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

            article.TypeId = type.Id;
            article.Title = vm.Title;
            article.Content = vm.Content;
            SanitizeArticle(article);
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(PredicateBuilder.Contains<NormalPoint, string>(vm.AttachedPointsId,
                    point => point.Id)).ToListAsync();
            article.PrincipalId = User.Identity.GetUserId();
            DbContext.Articles.Add(article);
            article.SequenceNumber =
                await DbContext.Database.SqlQuery<int>("SELECT NEXT VALUE FOR [dbo].[EntrySequence]").SingleAsync();
            lock (ArticleSaveLock)
            {
                article.SequenceNumberForAuthor =
                    (DbContext.Articles.Where(a => a.PrincipalId == article.PrincipalId)
                        .Select(a => a.SequenceNumberForAuthor)
                        .DefaultIfEmpty(0)
                        .Max()) + 1;
                DbContext.SaveChanges();
            }
            return Created($"article/{article.Id}", new ArticleDTO(article));
        }

        /// <summary>
        /// 编辑指定文章
        /// </summary>
        /// <param name="id">文章 Id</param>
        /// <param name="vm">文章相关属性，其中 Title, Content, TypeName 如果不提交表示不修改</param>
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
            if (vm.TypeName != null)
            {
                type = await DbContext.ArticleTypes.Where(t => t.Name == vm.TypeName).SingleOrDefaultAsync();
                if (type == null)
                {
                    ModelState.AddModelError("vm.TypeId", "Invalid article type.");
                    return BadRequest(ModelState);
                }
                article.TypeId = type.Id;
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
                article.VoteForPointId = null;
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
            {
                article.Content = vm.Content;
                SanitizeArticle(article);
            }
            article.AttachedPoints.Clear();
            await DbContext.SaveChangesAsync();
            article.AttachedPoints =
                await DbContext.NormalPoints.Where(PredicateBuilder.Contains<NormalPoint, string>(vm.AttachedPointsId,
                    point => point.Id)).ToListAsync();
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 净化此前未净化过的文章
        /// </summary>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route("sanitize")]
        public async Task<IHttpActionResult> PutSanitize()
        {
            var articles = await DbContext.Articles.Where(a => a.UnstyledContent == null).ToListAsync();
            foreach (var article in articles)
            {
                SanitizeArticle(article);
                if (article.ThumbnailImage == null)
                    article.ThumbnailImage = string.Empty;
            }
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        private static void SanitizeArticle(Article article)
        {
            Config.HtmlEncoder = new HtmlEncoderMinimum();
            var sanitizer = new HtmlSanitizer(new[] {"div", "br", "h1", "blockquote", "b", "i", "u", "img", "a"},
                null,
                new[] {"href", "src", "webp-src"});
            var dom = CQ.Create(sanitizer.Sanitize(article.Content));
            article.ThumbnailImage = string.Empty;
            foreach (var img in dom["img"])
            {
                if (img.Attributes["src"] == null) continue;
                if (img.Attributes["webp-src"] != null)
                {
                    if (string.IsNullOrEmpty(article.ThumbnailImage))
                        article.ThumbnailImage = img.Attributes["webp-src"];
                    img.RemoveAttribute("src");
                    continue;
                }
                var match = Regex.Match(img.Attributes["src"], @"^(?:http:|https:)?\/\/keylol\.b0\.upaiyun\.com\/(.*)$",
                    RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    if (string.IsNullOrEmpty(article.ThumbnailImage))
                        article.ThumbnailImage = img.Attributes["src"];
                    continue;
                }
                img.RemoveAttribute("src");
                img.Attributes["webp-src"] = "//keylol.b0.upaiyun.com/" + match.Groups[1].Value;
                if (!match.Groups[1].Value.Contains("!"))
                    img.Attributes["webp-src"] += "!article.image";
                if (string.IsNullOrEmpty(article.ThumbnailImage))
                    article.ThumbnailImage = img.Attributes["webp-src"];
            }
            article.Content = dom.Render();
            article.UnstyledContent = dom.Render(OutputFormatters.PlainText);
        }
    }
}