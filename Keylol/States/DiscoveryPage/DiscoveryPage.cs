using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// 发现页
    /// </summary>
    public class DiscoveryPage
    {
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
            return new DiscoveryPage
            {
                SlideshowEntries = await SlideshowEntryList.CreateAsync(dbContext),
                SpotlightPoints = await SpotlightPointList.CreateAsync(currentUserId, dbContext, cachedData),
                SpotlightReviews = await SpotlightArticleList.CreateAsync(currentUserId,
                    SpotlightArticleStream.ArticleCategory.Review, dbContext),
                SpotlightConferences = await SpotlightConferenceList.CreateAsync(dbContext),
                SpotlightStudies = await SpotlightArticleList.CreateAsync(currentUserId,
                    SpotlightArticleStream.ArticleCategory.Study, dbContext),
                SpotlightStories = await SpotlightArticleList.CreateAsync(currentUserId,
                    SpotlightArticleStream.ArticleCategory.Story, dbContext)
            };
        }

        /// <summary>
        /// Slideshow Entries
        /// </summary>
        public SlideshowEntryList SlideshowEntries { get; set; }

        /// <summary>
        /// Spotlight Points
        /// </summary>
        public SpotlightPointList SpotlightPoints { get; set; }

        /// <summary>
        /// Spotlight Reviews
        /// </summary>
        public SpotlightArticleList SpotlightReviews { get; set; }

        /// <summary>
        /// Spotlight Conferences
        /// </summary>
        public SpotlightConferenceList SpotlightConferences { get; set; }

        /// <summary>
        /// Spotlight Studies
        /// </summary>
        public SpotlightArticleList SpotlightStudies { get; set; }

        /// <summary>
        /// Spotlight Stories
        /// </summary>
        public SpotlightArticleList SpotlightStories { get; set; }
    }
}