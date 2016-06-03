using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.Point.Edit
{
    /// <summary>
    /// 聚合 - 据点 - 编辑 - 样式
    /// </summary>
    public class StylePage
    {
        /// <summary>
        /// 获取样式页
        /// </summary>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="StylePage"/></returns>
        public static async Task<StylePage> Get(string pointIdCode, [Injected] KeylolDbContext dbContext)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return new StylePage();
            return Create(point);
        }

        /// <summary>
        /// 创建 <see cref="StylePage"/>
        /// </summary>
        /// <param name="point">据点对象</param>
        /// <returns><see cref="StylePage"/></returns>
        public static StylePage Create(Models.Point point)
        {
            if (point.Type != PointType.Game && point.Type != PointType.Hardware)
                return new StylePage();
            return new StylePage
            {
                MediaHeaderImage = point.MediaHeaderImage,
                ThumbnailImage = point.ThumbnailImage
            };
        }

        /// <summary>
        /// 媒体中心头部图
        /// </summary>
        public string MediaHeaderImage { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }
    }
}