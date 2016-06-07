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
            var productPage = new ProductPage();
            if (point.Type == PointType.Vendor)
            {
                var developerProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Developer, null, dbContext, cachedData);
                productPage.DeveloperProducts = developerProducts.Item1;
                productPage.DeveloperProductCount = developerProducts.Item2;

                var publisherProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Publisher, null, dbContext, cachedData);
                productPage.PublisherProducts = publisherProducts.Item1;
                productPage.PublisherProductCount = publisherProducts.Item2;

                var resellerProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Reseller, null, dbContext, cachedData);
                productPage.ResellerProducts = resellerProducts.Item1;
                productPage.ResellerProductCount = resellerProducts.Item2;

                var manufacturerProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Manufacturer, null, dbContext, cachedData);
                productPage.ManufacturerProducts = manufacturerProducts.Item1;
                productPage.ManufacturerProductCount = manufacturerProducts.Item2;
            }
            else if (point.Type == PointType.Category)
            {
                var tagProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Tag, null, dbContext, cachedData);
                productPage.TagProducts = tagProducts.Item1;
                productPage.TagProductCount = tagProducts.Item2;

                var seriesProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Series, null, dbContext, cachedData);
                productPage.SeriesProducts = seriesProducts.Item1;
                productPage.SeriesProductCount = seriesProducts.Item2;

                var genreProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Genre, null, dbContext, cachedData);
                productPage.GenreProducts = genreProducts.Item1;
                productPage.GenreProductCount = genreProducts.Item2;
            }
            else if (point.Type == PointType.Platform)
            {
                var platformProducts = await ProductPointList.CreateAsync(currentUserId, point.Id,
                    PointRelationshipType.Platform, null, dbContext, cachedData);
                productPage.PlatformProducts = platformProducts.Item1;
            }
            return productPage;
        }

        /// <summary>
        /// 开发的作品
        /// </summary>
        public ProductPointList DeveloperProducts { get; set; }

        /// <summary>
        /// 开发的作品数
        /// </summary>
        public int? DeveloperProductCount { get; set; }

        /// <summary>
        /// 发行的作品
        /// </summary>
        public ProductPointList PublisherProducts { get; set; }

        /// <summary>
        /// 发行的作品数
        /// </summary>
        public int? PublisherProductCount { get; set; }

        /// <summary>
        /// 代理的作品
        /// </summary>
        public ProductPointList ResellerProducts { get; set; }

        /// <summary>
        /// 代理的作品数
        /// </summary>
        public int? ResellerProductCount { get; set; }

        /// <summary>
        /// 制造的作品
        /// </summary>
        public ProductPointList ManufacturerProducts { get; set; }

        /// <summary>
        /// 制造的作品数
        /// </summary>
        public int? ManufacturerProductCount { get; set; }

        /// <summary>
        /// 该特性的作品
        /// </summary>
        public ProductPointList TagProducts { get; set; }

        /// <summary>
        /// 该特性的作品数
        /// </summary>
        public int? TagProductCount { get; set; }

        /// <summary>
        /// 该系列的作品
        /// </summary>
        public ProductPointList SeriesProducts { get; set; }

        /// <summary>
        /// 该系列的作品数
        /// </summary>
        public int? SeriesProductCount { get; set; }

        /// <summary>
        /// 该流派的作品
        /// </summary>
        public ProductPointList GenreProducts { get; set; }

        /// <summary>
        /// 该流派的作品数
        /// </summary>
        public int? GenreProductCount { get; set; }

        /// <summary>
        /// 该平台的作品
        /// </summary>
        public ProductPointList PlatformProducts { get; set; }
    }
}