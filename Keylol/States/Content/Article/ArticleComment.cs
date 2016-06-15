using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Content.Article
{
    /// <summary>
    /// 文章评论列表
    /// </summary>
    public class ArticleCommentList : List<ArticleComment>
    {
        public static async Task<Tuple<ArticleCommentList, int, int>> CreateAsync(string articleId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var conditionQuery = from comment in dbContext.ArticleComments
                                 where comment.ArticleId == articleId
                                 orderby comment.Sid
                                 select comment;
//            var queryResult = await conditionQuery.Select(a => new
//            {
//
//            });
            return null;
        }
    }

    /// <summary>
    /// 文章评论
    /// </summary>
    public class ArticleComment
    {
        /// <summary>
        /// 作者识别码
        /// </summary>
        public string AuthorIdCode { get; set; }

        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorAvatarImage { get; set; }

        /// <summary>
        /// 作者用户名
        /// </summary>
        public string AuthorUserName { get; set; }

        /// <summary>
        /// 作者在档时间
        /// </summary>
        public double? AuthorPlayedTime { get; set; }

        /// <summary>
        /// 认可数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 楼层号
        /// </summary>
        public int SidForArticle { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否被封存
        /// </summary>
        public bool? Archived { get; set; }

        /// <summary>
        /// 是否被警告
        /// </summary>
        public bool? Warned { get; set; }
    }
}