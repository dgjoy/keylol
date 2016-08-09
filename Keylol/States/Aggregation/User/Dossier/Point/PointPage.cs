using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.User.Dossier.Point
{
    /// <summary>
    /// 用户的订阅列表
    /// </summary>
    public class PointPage
    {
        /// <summary>
        /// 用户文章列表页
        /// </summary>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"></param>
        public static async Task<PointPage> Get(string userIdCode, int page, int recordsPerPage,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData,
            [Injected] KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if(user == null)
                return new PointPage();
            return await CreateAsync(user.Id, StateTreeHelper.GetCurrentUserId(), page, recordsPerPage, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="PointPage"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="currentUserId">当前登录用户 Id</param>
        /// <param name="page">搜索页码</param>
        /// <param name="recordsPerPage">每页显示文章数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public static async Task<PointPage> CreateAsync(string userId, string currentUserId, int page, int recordsPerPage, KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var points = await PointList.CreateAsync(userId, StateTreeHelper.GetCurrentUserId(), page, recordsPerPage, true, dbContext, cachedData);
            return new PointPage
            {
                Results = points.Item1,
                PointCount = points.Item2
            };
        }

        /// <summary>
        /// 用户的据点订阅列表
        /// </summary>
        public PointList Results { get; set; }

        /// <summary>
        /// 用户订阅据点总数
        /// </summary>
        public int? PointCount { get; set; }
    }
}