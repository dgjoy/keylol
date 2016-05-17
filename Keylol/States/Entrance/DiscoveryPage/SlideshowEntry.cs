using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Newtonsoft.Json;

namespace Keylol.States.Entrance.DiscoveryPage
{
    /// <summary>
    /// 滑动展柜内容列表
    /// </summary>
    public class SlideshowEntryList : List<SlideshowEntry>
    {
        private SlideshowEntryList([NotNull] IEnumerable<SlideshowEntry> collection) : base(collection)
        {
        }

        /// <summary>
        /// 创建 <see cref="SlideshowEntryList"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SlideshowEntryList"/></returns>
        public static async Task<SlideshowEntryList> CreateAsync(KeylolDbContext dbContext)
        {
            return new SlideshowEntryList((await (from feed in dbContext.Feeds
                where feed.StreamName == SlideshowStream.Name
                orderby feed.Id descending
                select feed.Properties)
                .Take(4)
                .ToListAsync())
                .Select(JsonConvert.DeserializeObject<SlideshowStream.FeedProperties>)
                .Select(e => new SlideshowEntry
                {
                    Title = e.Title,
                    Subtitle = e.Subtitle,
                    Summary = e.Summary,
                    MinorTitle = e.MinorTitle,
                    MinorSubtitle = e.MinorSubtitle,
                    BackgroundImage = e.BackgroundImage,
                    Link = e.Link
                }));
        }
    }

    /// <summary>
    /// 滑动展柜内容
    /// </summary>
    public class SlideshowEntry
    {
        /// <summary>
        /// 主标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 概要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 次要主标题
        /// </summary>
        public string MinorTitle { get; set; }

        /// <summary>
        /// 次要副标题
        /// </summary>
        public string MinorSubtitle { get; set; }

        /// <summary>
        /// 背景图片
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 目标链接
        /// </summary>
        public string Link { get; set; }
    }
}