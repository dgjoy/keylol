using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;
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
        /// 获取滑动展柜内容列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SlideshowEntryList"/></returns>
        public static async Task<SlideshowEntryList> Get(int page, [Injected] KeylolDbContext dbContext)
        {
            return await CreateAsync(page, 10, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="SlideshowEntryList"/>
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页个数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SlideshowEntryList"/></returns>
        public static async Task<SlideshowEntryList> CreateAsync(int page, int recordsPerPage, KeylolDbContext dbContext)
        {
            return new SlideshowEntryList((await (from feed in dbContext.Feeds
                where feed.StreamName == SlideshowStream.Name
                orderby feed.Id descending
                select new
                {
                    feed.Id,
                    feed.Properties
                })
                .TakePage(page, recordsPerPage)
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

        /// <summary>
        /// 获取推送参考
        /// </summary>
        /// <param name="link">文章链接</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SlideshowEntry"/></returns>
        public static async Task<SlideshowEntry> GetReference(string link, [Injected] KeylolDbContext dbContext)
        {
            var result = new SlideshowEntry();
            var match = Regex.Match(link, @"^https?:\/\/.+\.keylol\.com\/article\/(.+)\/(\d+)$");
            if (!match.Success)
                return result;
            var idCode = match.Groups[1].Value;
            var sidForAuthor = int.Parse(match.Groups[2].Value);
            var article = await dbContext.Articles.Include(a => a.Author)
                .Where(a => a.Author.IdCode == idCode && a.SidForAuthor == sidForAuthor)
                .SingleOrDefaultAsync();
            if (article == null)
                return result;
            result.Title = article.Title;
            result.Subtitle = article.Subtitle;
            result.Author = article.Author.UserName;
            result.Date = article.PublishTime.Date.ToString("M月d日");
            result.BackgroundImage = article.CoverImage;
            return result;
        }

        /// <summary>
        /// 推送参考
        /// </summary>
        [Authorize(Roles = KeylolRoles.Operator)]
        public SlideshowEntry Reference { get; set; }
    }

    /// <summary>
    /// 滑动展柜内容
    /// </summary>
    public class SlideshowEntry
    {
        /// <summary>
        /// Feed ID
        /// </summary>
        public int? FeedId { get; set; }

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