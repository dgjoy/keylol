using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

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
        public static async Task<CouponGiftList> Get([Injected] KeylolDbContext dbContext,CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="CouponGiftList"/>
        /// </summary>
        /// <param name="currentUserId">当前用户Id</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<CouponGiftList> CreateAsync(string currentUserId, [Injected] KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult =
                await
                    dbContext.CouponGifts.Where(g => DateTime.Now < g.EndTime)
                        .OrderByDescending(g => g.CreateTime)
                        .Select(u => new
                        {
                            u.Id,
                            u.Name,
                            u.Descriptions,
                            u.PreviewImage,
                            u.Price,
                            u.ThumbnailImage,
                            u.GiftType
                        }).ToListAsync();

            var result = new CouponGiftList(queryResult.Count);
            foreach (var g in queryResult)
            {
                result.Add(new CouponGift
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Descriptions,
                    ThumbnailImage = g.ThumbnailImage,
                    Price = g.Price,
                    UserSeasonLiked = 9,
                    GiftType = g.GiftType
                });
            }
            return result;
        }

    }

    /// <summary>
    /// 商店上架商品
    /// </summary>
    public class CouponGift
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 商品名
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        [Obsolete]
        public string PreviewImage { get; set; }

        /// <summary>
        /// 文券价格
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 用户当季获得认可数
        /// </summary>
        public int UserSeasonLiked { get; set; }

        /// <summary>
        /// 商品类型，0 为未定义商品
        /// </summary>
        public CouponGiftType GiftType { get; set; }
    }
}