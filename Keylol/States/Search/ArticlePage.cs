
namespace Keylol.States.Search
{
    /// <summary>
    /// 文章搜索
    /// </summary>
    public class ArticlePage
    {
    }

    /// <summary>
    /// 文章搜索结果
    /// </summary>
    public class ArticleResult
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Titile { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 投稿据点ID
        /// </summary>
        public string TargetPointId { get; set; }

        /// <summary>
        /// 据点头像
        /// </summary>
        public string TargetPointAvater { get; set; }

        /// <summary>
        /// 获赞数
        /// </summary>
        public long LikeCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public long CommentCount { get; set; }
    }
}