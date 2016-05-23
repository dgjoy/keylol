using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Newtonsoft.Json;

namespace Keylol.States.Entrance.Discovery
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
                select new
                {
                    feed.Id,
                    feed.Properties
                })
                .Take(4)
                .ToListAsync())
                .Select(f =>
                {
                    var p = JsonConvert.DeserializeObject<SlideshowStream.FeedProperties>(f.Properties);
                    return new SlideshowEntry
                    {
                        FeedId = f.Id,
                        Title = p.Title,
                        Subtitle = p.Subtitle,
                        Author = p.Author,
                        Date = p.Date,
                        MinorTitle = p.MinorTitle,
                        MinorSubtitle = p.MinorSubtitle,
                        BackgroundImage = p.BackgroundImage,
                        Link = p.Link
                    };
                }));
        }
    }

    /// <summary>
    /// 滑动展柜内容
    /// </summary>
    public class SlideshowEntry
    {
        /// <summary>
        /// Feed ID
        /// </summary>
        public int FeedId { get; set; }

        /// <summary>
        /// 主标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

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