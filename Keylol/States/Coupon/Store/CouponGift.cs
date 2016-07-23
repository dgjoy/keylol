using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;

namespace Keylol.States.Coupon.Store
{
    /// <summary>
    /// 文券商店商品列表
    /// </summary>
    public class CouponGiftList : List<CouponGift>
    {
        private CouponGiftList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="CouponGiftList"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="CouponGiftList"/></returns>
        public static async Task<CouponGiftList> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, [Injected] KeylolUserManager userManager)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData, userManager);
        }

        /// <summary>
        /// 创建 <see cref="CouponGiftList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户Id</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        public static async Task<CouponGiftList> CreateAsync(string currentUserId, [Injected] KeylolDbContext dbContext,
            [Injected]CachedDataProvider cachedData, [Injected]KeylolUserManager userManager)
        {
            var queryResult = await dbContext.CouponGifts.Where(g => DateTime.Now < g.EndTime)
                .OrderByDescending(g => g.CreateTime)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Descriptions,
                    g.Price,
                    g.ThumbnailImage,
                    g.Type
                }).ToListAsync();

            var steamCnUId = await userManager.GetSteamCnUidAsync(currentUserId);
            var steamCnUserName =
                dbContext.Users.Where(u => u.Id == currentUserId).Select(u => u.SteamCnUserName).First();
            var email = dbContext.Users.Where(u => u.Id == currentUserId).Select(u => u.Email).First();

            var result = new CouponGiftList(queryResult.Count);
            foreach (var g in queryResult)
            {
                result.Add(new CouponGift
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Descriptions,
                    Price = g.Price,
                    ThumbnailImage = g.ThumbnailImage,
                    Type = g.Type,
                    SteamCnUId = steamCnUId,
                    SteamCnUserName = steamCnUserName,
                    Email = email
                });
            }
            return result;
        }

    }

    /// <summary>
    /// 文券商店商品
    /// </summary>
    public class CouponGift
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 商品名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public int? Price { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public CouponGiftType? Type { get; set; }

        /// <summary>
        /// 蒸汽动力账号
        /// </summary>
        public string SteamCnUId { get; set; }

        /// <summary>
        /// SteamCN 用户名
        /// </summary>
        public string SteamCnUserName { get; set; }

        /// <summary>
        /// 电邮地址
        /// </summary>
        public string Email { get; set; }
    }
}