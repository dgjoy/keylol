using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Controllers.CouponGiftOrder.Processors;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
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
        /// <param name="coupon"></param>
        /// <returns><see cref="CouponGiftList"/></returns>
        public static async Task<CouponGiftList> Get([Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, [Injected] KeylolUserManager userManager,[Injected] CouponProvider coupon)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData, userManager, coupon);
        }

        /// <summary>
        /// 创建 <see cref="CouponGiftList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户Id</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="coupon"></param>
        public static async Task<CouponGiftList> CreateAsync(string currentUserId, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData, [Injected] KeylolUserManager userManager, [Injected] CouponProvider coupon)
        {
            var queryResult = await dbContext.CouponGifts.Where(g => DateTime.Now < g.EndTime)
                .OrderByDescending(g => g.CreateTime)
                .ToListAsync();

            var steamCnUid = await userManager.GetSteamCnUidAsync(currentUserId);
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            var email = dbContext.Users.Where(u => u.Id == currentUserId).Select(u => u.Email).First();

            var result = new CouponGiftList(queryResult.Count);
            foreach (var g in queryResult)
            {
                GiftProcessor processor;
                switch (g.Type)
                {
                    case CouponGiftType.Custom:
                        processor = new CustomProcessor();
                        break;

                    case CouponGiftType.SteamCnCredit:
                        processor = new SteamCnCreditProcessor(dbContext, userManager, coupon);
                        break;

                    case CouponGiftType.SteamGiftCard:
                        processor = new SteamGiftCardProcessor(dbContext, userManager, coupon);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var stateTreeGift = new CouponGift
                {
                    Id = g.Id,
                    Name = g.Name,
                    Descriptions = g.Descriptions,
                    Price = g.Price,
                    ThumbnailImage = g.ThumbnailImage,
                    Type = g.Type
                };
                processor.Initialize(currentUserId, g);
                await processor.FillPropertiesAsync(stateTreeGift);
                result.Add(stateTreeGift);
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
        public string Descriptions { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public int? Price { get; set; }

        /// <summary>
        /// 消费额度
        /// </summary>
        public int? Credit { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public CouponGiftType? Type { get; set; }

        /// <summary>
        /// 蒸汽动力账号
        /// </summary>
        public string SteamCnUid { get; set; }

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