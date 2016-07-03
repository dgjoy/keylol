
using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Search
{
    /// <summary>
    /// 文章搜索
    /// </summary>
    public class ArticlePage
    {
    }

    //public class ArticleResultList : List<ArticleResult>
    //{
    //    private ArticleResultList()
    //    {
    //    }

    //    private ArticleResultList(int capacity) : base(capacity)
    //    {
    //    }

    //    public static async Task<ArticleResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
    //        [Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
    //    {
    //        var 
    //    }

    //    public static async Task<ArticleResultList> CreateAsync(string userId, string keyword,
    //        [Injected] KeylolDbContext dbContext,[Injected] CachedDataProvider cachedData, int page, bool searchAll = true)
    //    {
    //        var searchResult = await dbContext.Database.SqlQuery<ArticleResult>(
    //            @"").ToListAsync();
    //    }
    //}

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
        /// 作者用户识别码
        /// </summary>
        public string AutherrUserIdCode { get; set; }

        /// <summary>
        /// 作者的文章定位
        /// </summary>
        public int SidForAuther { get; set; }

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