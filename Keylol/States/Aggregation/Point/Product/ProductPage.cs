using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point.Product
{
    /// <summary>
    /// 聚合 - 据点 - 作品
    /// </summary>
    public class ProductPage
    {
        /// <summary>
        /// 获取据点作品页
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ProductPage"/></returns>
        public static async Task<ProductPage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new ProductPage();
            return await CreateAsync(point, StateTreeHelper.GetCurrentUserId(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="ProductPage"/>
        /// </summary>
        /// <param name="point">已经查询好的据点对象</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="ProductPage"/></returns>
        public static async Task<ProductPage> CreateAsync(Models.Point point, string currentUserId,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            return new ProductPage
            {
                Products = await ProductPointList.CreateAsync(currentUserId, point.Id, dbContext, cachedData)
            };
        }

        /// <summary>
        /// 作品列表
        /// </summary>
        public ProductPointList Products { get; set; }
    }
}