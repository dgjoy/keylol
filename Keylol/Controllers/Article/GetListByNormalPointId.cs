using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Controllers.NormalPoint;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     获取指定据点时间轴的文章
        /// </summary>
        /// <param name="normalPointId">据点 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        /// <param name="articleTypeFilter">文章类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="beforeSN">获取编号小于这个数字的文章，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route("point/{normalPointId}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDTO>))]
        public async Task<IHttpActionResult> GetListByNormalPointId(string normalPointId,
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
                var types = articleTypeFilter.Split(',').Select(s => s.Trim().ToEnum<ArticleType>()).ToList();
                articleQuery =
                    articleQuery.Where(PredicateBuilder.Contains<Models.Article, ArticleType>(types, a => a.Type));
            }
            var articleEntries = await articleQuery.OrderByDescending(a => a.SequenceNumber).Take(() => take).Select(
                a => new
                {
                    article = a,
                    likeCount = a.Likes.Count(l => l.Backout == false),
                    commentCount = a.Comments.Count,
                    type = a.Type,
                    author = a.Principal.User,
                    voteForPoint = a.VoteForPoint
                }).ToListAsync();
            return Ok(articleEntries.Select(entry =>
            {
                var articleDTO = new ArticleDTO(entry.article, true, 256, true)
                {
                    LikeCount = entry.likeCount,
                    CommentCount = entry.commentCount,
                    TypeName = entry.type.ToString(),
                    Author = new UserDTO(entry.author),
                    VoteForPoint = entry.voteForPoint == null ? null : new NormalPointDTO(entry.voteForPoint, true)
                };
                if (string.IsNullOrEmpty(entry.article.ThumbnailImage))
                {
                    articleDTO.ThumbnailImage = entry.voteForPoint?.BackgroundImage;
                }
                if (articleDTO.TypeName != "简评")
                    articleDTO.TruncateContent(128);
                return articleDTO;
            }));
        }
    }
}