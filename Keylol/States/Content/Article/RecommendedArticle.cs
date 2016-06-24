using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Utilities;

namespace Keylol.States.Content.Article
{
    /// <summary>
    /// 推荐文章列表
    /// </summary>
    public class RecommendedArticleList : List<RecommendedArticle>
    {
        private RecommendedArticleList([NotNull] IEnumerable<RecommendedArticle> collection) : base(collection)
        {
        }

        private RecommendedArticleList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="RecommendedArticleList"/>
        /// </summary>
        /// <param name="currentArticleId">当前文章 ID</param>
        /// <param name="authorId">文章作者 ID</param>
        /// <param name="pointId">据点 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="RecommendedArticleList"/></returns>
        public static async Task<RecommendedArticleList> CreateAsync(string currentArticleId, string authorId,
            string pointId, KeylolDbContext dbContext)
        {
            var result = new RecommendedArticleList(3);

            // 作者的其他文章中，认可最多的文章
            var a = await (from article in dbContext.Articles
                where article.AuthorId == authorId && article.Archived == ArchivedState.None &&
                      article.Rejected == false && article.Id != currentArticleId
                orderby dbContext.Likes
                    .Count(l => l.TargetId == article.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    AuthorIdCode = article.Author.IdCode,
                    article.SidForAuthor,
                    article.CoverImage,
                    article.Title,
                    article.Subtitle
                }).FirstOrDefaultAsync();
            if (a != null)
                result.Add(new RecommendedArticle
                {
                    AuthorIdCode = a.AuthorIdCode,
                    SidForAuthor = a.SidForAuthor,
                    CoverImage = a.CoverImage,
                    Title = a.Title,
                    Subtitle = a.Subtitle
                });

            // 据点的其他文章中，认可最多的文章
            a = await (from article in dbContext.Articles
                where article.TargetPointId == pointId && article.Archived == ArchivedState.None &&
                      article.Rejected == false && article.Id != currentArticleId
                orderby dbContext.Likes
                    .Count(l => l.TargetId == article.Id && l.TargetType == LikeTargetType.Article) descending
                select new
                {
                    AuthorIdCode = article.Author.IdCode,
                    article.SidForAuthor,
                    article.CoverImage,
                    article.Title,
                    article.Subtitle
                }).FirstOrDefaultAsync();
            if (a != null)
                result.Add(new RecommendedArticle
                {
                    AuthorIdCode = a.AuthorIdCode,
                    SidForAuthor = a.SidForAuthor,
                    CoverImage = a.CoverImage,
                    Title = a.Title,
                    Subtitle = a.Subtitle
                });

            // 作者最近认可过的文章
            a = await (from like in dbContext.Likes
                where like.OperatorId == authorId && like.TargetType == LikeTargetType.Article
                join article in dbContext.Articles on like.TargetId equals article.Id
                where article.Archived == ArchivedState.None && article.Rejected == false
                orderby like.Sid descending
                select new
                {
                    AuthorIdCode = article.Author.IdCode,
                    article.SidForAuthor,
                    article.CoverImage,
                    article.Title,
                    article.Subtitle
                }).FirstOrDefaultAsync();
            if (a != null)
                result.Add(new RecommendedArticle
                {
                    AuthorIdCode = a.AuthorIdCode,
                    SidForAuthor = a.SidForAuthor,
                    CoverImage = a.CoverImage,
                    Title = a.Title,
                    Subtitle = a.Subtitle
                });

            result = new RecommendedArticleList(result.DistinctBy(aa => new {aa.AuthorIdCode, aa.SidForAuthor}));
            // 广场收稿箱中最近的文章
            if (result.Count < 3)
            {
                var remaining = 3 - result.Count;
                var supplies = await (from article in dbContext.Articles
                    where article.Rejected == false && article.Archived == ArchivedState.None &&
                          article.Id != currentArticleId
                    orderby article.Sid descending
                    select new
                    {
                        AuthorIdCode = article.Author.IdCode,
                        article.SidForAuthor,
                        article.CoverImage,
                        article.Title,
                        article.Subtitle
                    }).Take(() => remaining).ToListAsync();
                result.AddRange(supplies.Select(s => new RecommendedArticle
                {
                    AuthorIdCode = s.AuthorIdCode,
                    SidForAuthor = s.SidForAuthor,
                    CoverImage = s.CoverImage,
                    Title = s.Title,
                    Subtitle = s.Subtitle
                }));
            }

            return result;
        }
    }

    /// <summary>
    /// 推荐文章
    /// </summary>
    public class RecommendedArticle
    {
        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 在作者名下序号
        /// </summary>
        public int? SidForAuthor { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }
    }
}