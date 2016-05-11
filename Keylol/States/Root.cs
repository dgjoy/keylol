using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Keylol.States
{
    /// <summary>
    /// State Tree Root
    /// </summary>
    public class Root
    {
        /// <summary>
        /// 获取新的完整状态树
        /// </summary>
        /// <param name="page">访问的页面</param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>完整状态树</returns>
        public static async Task<Root> Get(Page page, [Injected] KeylolUserManager userManager,
            [Injected] KeylolDbContext dbContext, [Injected] CouponProvider coupon,
            [Injected] CachedDataProvider cachedData)
        {
            var root = new Root();
            var currentUserId = StateTreeHelper.CurrentUser().Identity.GetUserId();
            if (await StateTreeHelper.CanAccessAsync<Root>(nameof(CurrentUser)))
            {
                var user = await userManager.FindByIdAsync(currentUserId);

                // 每日访问奖励
                if (DateTime.Now.Date > user.LastDailyRewardTime.Date)
                {
                    user.LastDailyRewardTime = DateTime.Now;
                    user.FreeLike = 5; // 免费认可重置
                    try
                    {
                        await dbContext.SaveChangesAsync();
                        await coupon.UpdateAsync(user, CouponEvent.每日访问);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                    }
                }

                root.CurrentUser = new CurrentUser
                {
                    UserName = user.UserName,
                    IdCode = user.IdCode,
                    AvatarImage = user.AvatarImage,
                    MessageCount = await dbContext.Messages.CountAsync(m => m.ReceiverId == user.Id && m.Unread),
                    Coupon = user.Coupon,
                    DraftCount = 0, // TODO
                    PreferredPointName = user.PreferredPointName
                };
            }

            switch (page)
            {
                case Page.Discovery:
                {
                    root.DiscoveryPage = new DiscoveryPage.DiscoveryPage
                    {
                        SlideshowEntries = (await dbContext.Feeds.Where(f => f.StreamName == SlideshowStream.Name)
                            .OrderByDescending(f => f.Id)
                            .Take(4)
                            .Select(f => f.Properties)
                            .ToListAsync())
                            .Select(JsonConvert.DeserializeObject<SlideshowStream.FeedProperties>)
                            .Select(e => new DiscoveryPage.SlideshowEntry
                            {
                                Title = e.Title,
                                Subtitle = e.Subtitle,
                                Summary = e.Summary,
                                MinorTitle = e.MinorTitle,
                                MinorSubtitle = e.MinorSubtitle,
                                BackgroundImage = e.BackgroundImage,
                                Link = e.Link
                            })
                            .ToList(),
                        SpotlightPoints = (await Task.WhenAll((await (from feed in dbContext.Feeds
                            where feed.StreamName == SpotlightPointStream.Name
                            orderby feed.Id descending
                            join point in dbContext.Points on feed.Entry equals point.Id
                            select new
                            {
                                point.Id,
                                point.IdCode,
                                point.AvatarImage,
                                point.EnglishName,
                                point.ChineseName,
                                point.SteamAppId,
                                point.SteamPrice,
                                point.SteamDiscount,
                                point.SonkwoProductId,
                                point.SonkwoPrice,
                                point.SonkwoDiscount,
                                point.UplayLink,
                                point.UplayPrice,
                                point.XboxLink,
                                point.XboxPrice,
                                point.PlayStationPrice,
                                point.PlayStationLink
                            }).Take(30).ToListAsync())
                            .Select(async p => new DiscoveryPage.SpotlightPoint
                            {
                                IdCode = p.IdCode,
                                AvatarImage = p.AvatarImage,
                                EnglishName = p.EnglishName,
                                ChineseName = p.ChineseName,
                                AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                                SteamAppId = p.SteamAppId,
                                SteamPrice = p.SteamPrice,
                                SteamDiscount = p.SteamDiscount,
                                SonkwoProductId = p.SonkwoProductId,
                                SonkwoPrice = p.SonkwoPrice,
                                SonkwoDiscount = p.SonkwoDiscount,
                                UplayLink = p.UplayLink,
                                UplayPrice = p.UplayPrice,
                                XboxLink = p.XboxLink,
                                XboxPrice = p.XboxPrice,
                                PlayStationLink = p.PlayStationLink,
                                PlayStationPrice = p.PlayStationPrice,
                                Subscribed = await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                                    SubscriptionTargetType.Point)
                            }))).ToList()
                    };
                    break;
                }

                case Page.Points:
                    break;

                case Page.Timeline:
                    break;
            }

            return root;
        }

        /// <summary>
        /// 需要访问的页面
        /// </summary>
        public enum Page
        {
            /// <summary>
            /// 发现
            /// </summary>
            Discovery,

            /// <summary>
            /// 据点
            /// </summary>
            Points,

            /// <summary>
            /// 轨道
            /// </summary>
            Timeline
        }

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        [Authorize]
        public CurrentUser CurrentUser { get; set; }

        /// <summary>
        /// 发现页
        /// </summary>
        public DiscoveryPage.DiscoveryPage DiscoveryPage { get; set; }
    }
}