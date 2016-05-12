using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// 是日优惠据点列表
    /// </summary>
    public class OnSalePointList : List<OnSalePoint>
    {
        private const int RecordsPerPage = 10;

        private OnSalePointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取指定页码的是日优惠据点列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>是日优惠据点列表</returns>
        public static async Task<OnSalePointList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.CurrentUser().Identity.GetUserId(), page, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="OnSalePointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="OnSalePointList"/></returns>
        public static async Task<OnSalePointList> CreateAsync(string currentUserId, int page, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == OnSalePointStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                orderby feed.Id descending
                select new
                {
                    point.Id,
                    point.IdCode,
                    point.CapsuleImage,
                    point.ChineseName,
                    point.EnglishName,
                    point.SteamPrice,
                    point.SteamDiscountedPrice,
                    point.SteamAppId
                }).TakePage(page, RecordsPerPage).ToListAsync();
            var result = new OnSalePointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new OnSalePoint
                {
                    IdCode = p.IdCode,
                    CapsuleImage = p.CapsuleImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    SteamPrice = p.SteamPrice,
                    SteamDiscountedPrice = p.SteamDiscountedPrice,
                    InLibrary = p.SteamAppId != null &&
                                await cachedData.Users.IsSteamAppInLibrary(currentUserId, p.SteamAppId.Value)
                });
            }
            return result;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        /// <returns>总页数</returns>
        public static async Task<int> PageCountAsync(KeylolDbContext dbContext)
        {
            return (int) Math.Ceiling(await (from feed in dbContext.Feeds
                where feed.StreamName == OnSalePointStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                select feed)
                .CountAsync()/(double) RecordsPerPage);
        }
    }

    /// <summary>
    /// 是日优惠据点
    /// </summary>
    public class OnSalePoint
    {
        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 胶囊图
        /// </summary>
        public string CapsuleImage { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

        /// <summary>
        /// Steam 价格
        /// </summary>
        public double? SteamPrice { get; set; }

        /// <summary>
        /// Steam 折后价格
        /// </summary>
        public double? SteamDiscountedPrice { get; set; }

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool InLibrary { get; set; }
    }
}