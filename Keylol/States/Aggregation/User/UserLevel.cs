using System;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Aggregation.User.Dossier;
using Keylol.States.Aggregation.User.Dossier.Default;
using Keylol.States.Aggregation.User.People;
using Keylol.States.Shared;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User
{
    /// <summary>
    /// 聚合 - 个人层级
    /// </summary>
    public class UserLevel
    {
        /// <summary>
        /// 获取用户个人层级状态树
        /// </summary>
        /// <param name="entrance">要获取的页面</param>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="UserLevel"/></returns>
        public static async Task<UserLevel> Get(string entrance, string userIdCode,
            [Injected] KeylolDbContext dbContext, [Injected] CachedDataProvider cachedData,
            [Injected] KeylolUserManager userManager)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), userIdCode,
                entrance.ToEnum<EntrancePage>(), dbContext, cachedData, userManager);
        }

        /// <summary>
        /// 创建 <see cref="UserLevel"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="targetPage">要获取的页面</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="UserLevel"/></returns>
        public static async Task<UserLevel> CreateAsync(string currentUserId, string userIdCode, EntrancePage targetPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData, KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if (user == null)
                return new UserLevel();
            var result = new UserLevel
            {
                BasicInfo = await UserBasicInfo.CreateAsync(currentUserId, user, dbContext, cachedData, userManager)
            };
            switch (targetPage)
            {
                case EntrancePage.Auto:
//                    if (await cachedData.Subscriptions
//                        .IsSubscribedAsync(currentUserId, user.Id, SubscriptionTargetType.User))
//                    {
//                        result.Current = EntrancePage.Timeline;
//                    }
//                    else
//                    {
//                        result.Dossier =
//                            await DossierPage.CreateAsync(user, currentUserId, dbContext, cachedData, userManager);
//                        result.Current = EntrancePage.Dossier;
//                    }
                    result.Dossier = new DossierLevel
                    {
                        Default = await DefaultPage.CreateAsync(user, currentUserId, dbContext, cachedData, userManager)
                    };
                    result.Current = EntrancePage.Dossier;
                    break;

                case EntrancePage.Dossier:
                    result.Dossier = new DossierLevel
                    {
                        Default = await DefaultPage.CreateAsync(user, currentUserId, dbContext, cachedData,userManager)
                    };
                    break;

                case EntrancePage.People:
                    result.People = await PeoplePage.CreateAsync(user.Id, currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Timeline:
                    result.Timeline = await TimelinePage.CreateAsync(user.Id, currentUserId, dbContext, cachedData);
                    break;

                case EntrancePage.Edit:
                    if (await StateTreeHelper.CanAccessAsync<UserLevel>(nameof(Edit)))
                        result.Edit = await EditPage.CreateAsync(user, currentUserId, dbContext, userManager);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetPage), targetPage, null);
            }
            return result;
        }

        /// <summary>
        /// 用户基础信息
        /// </summary>
        public UserBasicInfo BasicInfo { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public EntrancePage? Current { get; set; }

        /// <summary>
        /// 档案
        /// </summary>
        public DossierLevel Dossier { get; set; }

        /// <summary>
        /// 人脉
        /// </summary>
        public PeoplePage People { get; set; }

        /// <summary>
        /// 轨道
        /// </summary>
        public TimelinePage Timeline { get; set; }

        /// <summary>
        /// 编辑
        /// </summary>
        [Authorize]
        public EditPage Edit { get; set; }
    }

    /// <summary>
    /// 目标入口页
    /// </summary>
    public enum EntrancePage
    {
        /// <summary>
        /// 自动（根据订阅状态）
        /// </summary>
        Auto,

        /// <summary>
        /// 档案
        /// </summary>
        Dossier,

        /// <summary>
        /// 人脉
        /// </summary>
        People,

        /// <summary>
        /// 轨道
        /// </summary>
        Timeline,

        /// <summary>
        /// 编辑
        /// </summary>
        Edit
    }
}