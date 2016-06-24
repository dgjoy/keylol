using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Entrance.Discovery
{
    /// <summary>
    /// 入口 - 发现
    /// </summary>
    public class DiscoveryPage
    {
        /// <summary>
        /// 获取“入口 - 发现”页面状态分支
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="DiscoveryPage"/></returns>
        public static async Task<DiscoveryPage> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 获取精选评测
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightArticleList"/></returns>
        public static async Task<SpotlightArticleList> GetSpotlightReviews(int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await SpotlightArticleList.CreateAsync(StateTreeHelper.GetCurrentUserId(), page, 12,
                SpotlightArticleStream.ArticleCategory.Review, dbContext, cachedData);
        }

        /// <summary>
        /// 获取精选研究
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightArticleList"/></returns>
        public static async Task<SpotlightArticleList> GetSpotlightStudies(int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await SpotlightArticleList.CreateAsync(StateTreeHelper.GetCurrentUserId(), page, 12,
                SpotlightArticleStream.ArticleCategory.Study, dbContext, cachedData);
        }

        /// <summary>
        /// 获取精选谈论
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="SpotlightArticleList"/></returns>
        public static async Task<SpotlightArticleList> GetSpotlightStories(int page,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData)
        {
            return await SpotlightArticleList.CreateAsync(StateTreeHelper.GetCurrentUserId(), page, 12,
                SpotlightArticleStream.ArticleCategory.Story, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="DiscoveryPage"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="DiscoveryPage"/></returns>
        public static async Task<DiscoveryPage> CreateAsync(string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var onSalePoints = await OnSalePointList.CreateAsync(currentUserId, 1, true, true, dbContext, cachedData);
            var latestArticles = await LatestArticleList.CreateAsync(1, true, true, dbContext, cachedData);
            return new DiscoveryPage
            {
                SlideshowEntries = await SlideshowEntryList.CreateAsync(1, 4, dbContext),
                SpotlightPoints = await SpotlightPointList.CreateAsync(currentUserId, 1, 30, dbContext, cachedData),
                SpotlightReviews = await SpotlightArticleList.CreateAsync(currentUserId, 1, 4,
                    SpotlightArticleStream.ArticleCategory.Review, dbContext, cachedData),
                SpotlightStudies = await SpotlightArticleList.CreateAsync(currentUserId, 1, 4,
                    SpotlightArticleStream.ArticleCategory.Study, dbContext, cachedData),
                OnSalePointHeaderImage = onSalePoints.Item3,
                OnSalePointPageCount = onSalePoints.Item2,
                OnSalePoints = onSalePoints.Item1,
                SpotlightStories = await SpotlightArticleList.CreateAsync(currentUserId, 1, 4,
                    SpotlightArticleStream.ArticleCategory.Story, dbContext, cachedData),
                LatestArticleHeaderImage = latestArticles.Item3,
                LatestArticlePageCount = latestArticles.Item2,
                LatestArticles = latestArticles.Item1
            };
        }

        /// <summary>
        /// 滑动展柜
        /// </summary>
        public SlideshowEntryList SlideshowEntries { get; set; }

        /// <summary>
        /// 精选据点
        /// </summary>
        public SpotlightPointList SpotlightPoints { get; set; }

        /// <summary>
        /// 精选评测
        /// </summary>
        public SpotlightArticleList SpotlightReviews { get; set; }

        /// <summary>
        /// 精选研究
        /// </summary>
        public SpotlightArticleList SpotlightStudies { get; set; }

        /// <summary>
        /// 是日优惠头部图
        /// </summary>
        public string OnSalePointHeaderImage { get; set; }

        /// <summary>
        /// 是日优惠总页数
        /// </summary>
        public int OnSalePointPageCount { get; set; }

        /// <summary>
        /// 是日优惠
        /// </summary>
        public OnSalePointList OnSalePoints { get; set; }

        /// <summary>
        /// 精选谈论
        /// </summary>
        public SpotlightArticleList SpotlightStories { get; set; }

        /// <summary>
        /// 最新文章头部图
        /// </summary>
        public string LatestArticleHeaderImage { get; set; }

        /// <summary>
        /// 最新文章总页数
        /// </summary>
        public int LatestArticlePageCount { get; set; }

        /// <summary>
        /// 最新文章
        /// </summary>
        public LatestArticleList LatestArticles { get; set; }
    }
}