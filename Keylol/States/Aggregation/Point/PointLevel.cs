using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Aggregation.Point.Edit;
using Keylol.States.Aggregation.Point.Frontpage;
using Keylol.States.Aggregation.Point.Intel;
using Keylol.States.Aggregation.Point.Timeline;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.Point
{
    /// <summary>
    /// 聚合 - 据点层级
    /// </summary>
    public class PointLevel
    {
        /// <summary>
        /// 获取据点层级状态树
        /// </summary>
        /// <param name="entrance">要获取的页面</param>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointLevel"/></returns>
        public static async Task<PointLevel> Get(string entrance, string pointIdCode,
            [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(),
                pointIdCode, entrance.ToEnum<EntrancePage>(), dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="PointLevel"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="pointIdCode">据点识别码</param>
        /// <param name="targetPage">要获取的页面</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetPage"/> 超出范围</exception>
        public static async Task<PointLevel> CreateAsync(string currentUserId, string pointIdCode,
            EntrancePage targetPage, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var point = await dbContext.Points.Where(p => p.IdCode == pointIdCode).SingleOrDefaultAsync();
            if (point == null)
                return null;
            var result = new PointLevel
            {
                BasicInfo = await Point.BasicInfo.BasicInfo.CreateAsync(currentUserId, point, dbContext, cachedData)
            };
            switch (targetPage)
            {
                case EntrancePage.Auto:
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        result.Frontpage = await FrontpagePage.CreateAsync(point, currentUserId, dbContext, cachedData);
                        result.Current = EntrancePage.Frontpage;
                    }
                    else
                    {
                        result.Current = EntrancePage.Timeline;
                    }
                    break;

                case EntrancePage.Frontpage:
                    result.Frontpage = await FrontpagePage.CreateAsync(point, currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Intel:
                    result.Intel = await IntelPage.CreateAsync(point, currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Timeline:
                    break;

                case EntrancePage.EditInfo:
                    result.Edit = new EditLevel
                    {
                        Info = await InfoPage.CreateAsync(point, dbContext)
                    };
                    break;

                case EntrancePage.EditStyle:
                    result.Edit = new EditLevel
                    {
                        Style = StylePage.Create(point)
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetPage), targetPage, null);
            }
            return result;
        }

        /// <summary>
        /// 据点基础信息
        /// </summary>
        public BasicInfo.BasicInfo BasicInfo { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public EntrancePage? Current { get; set; }

        /// <summary>
        /// 扉页
        /// </summary>
        public FrontpagePage Frontpage { get; set; }

        /// <summary>
        /// 情报
        /// </summary>
        public IntelPage Intel { get; set; }

        /// <summary>
        /// 轨道
        /// </summary>
        public TimelinePage Timeline { get; set; }

        /// <summary>
        /// 编辑层级
        /// </summary>
        [Authorize]
        public EditLevel Edit { get; set; }
    }

    /// <summary>
    /// 目标入口页
    /// </summary>
    public enum EntrancePage
    {
        /// <summary>
        /// 自动（根据登录状态）
        /// </summary>
        Auto,

        /// <summary>
        /// 扉页
        /// </summary>
        Frontpage,

        /// <summary>
        /// 情报
        /// </summary>
        Intel,

        /// <summary>
        /// 轨道
        /// </summary>
        Timeline,

        /// <summary>
        /// 编辑 - 信息
        /// </summary>
        EditInfo,

        /// <summary>
        /// 编辑 - 样式
        /// </summary>
        EditStyle
    }
}