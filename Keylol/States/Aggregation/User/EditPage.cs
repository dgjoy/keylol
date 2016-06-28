using System.Threading.Tasks;
using Keylol.Hubs;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.Aggregation.User
{
    /// <summary>
    /// 聚合 - 个人 - 编辑
    /// </summary>
    public class EditPage
    {
        /// <summary>
        /// 获取指定用户的编辑页
        /// </summary>
        /// <param name="userIdCode">用户识别码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="EditPage"/></returns>
        public static async Task<EditPage> Get(string userIdCode, [Injected] KeylolDbContext dbContext,
            [Injected] KeylolUserManager userManager)
        {
            var user = await userManager.FindByIdCodeAsync(userIdCode);
            if (user == null)
                return new EditPage();
            return await CreateAsync(user, StateTreeHelper.GetCurrentUserId(), dbContext, userManager);
        }

        /// <summary>
        /// 创建 <see cref="EditPage"/>
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="EditPage"/></returns>
        public static async Task<EditPage> CreateAsync(KeylolUser user, string currentUserId, KeylolDbContext dbContext,
            KeylolUserManager userManager)
        {
            if (currentUserId != user.Id && !await userManager.IsInRoleAsync(currentUserId, KeylolRoles.Operator))
                return new EditPage();

            var editPage = new EditPage
            {
                Email = user.Email,
                LockoutEnabled = user.LockoutEnabled,
                OpenInNewWindow = user.OpenInNewWindow,
                UseEnglishPointName = user.PreferredPointName == PreferredPointName.English,
                NotifyOnArticleReplied = user.NotifyOnArticleReplied,
                NotifyOnCommentReplied = user.NotifyOnCommentReplied,
                NotifyOnActivityReplied = user.NotifyOnActivityReplied,
                NotifyOnArticleLiked = user.NotifyOnArticleLiked,
                NotifyOnCommentLiked = user.NotifyOnCommentLiked,
                NotifyOnActivityLiked = user.NotifyOnActivityLiked,
                NotifyOnSubscribed = user.NotifyOnSubscribed,
                SteamNotifyOnArticleReplied = user.SteamNotifyOnArticleReplied,
                SteamNotifyOnCommentReplied = user.SteamNotifyOnCommentReplied,
                SteamNotifyOnActivityReplied = user.SteamNotifyOnActivityReplied,
                SteamNotifyOnArticleLiked = user.SteamNotifyOnArticleLiked,
                SteamNotifyOnCommentLiked = user.SteamNotifyOnCommentLiked,
                SteamNotifyOnActivityLiked = user.SteamNotifyOnActivityLiked,
                SteamNotifyOnSubscribed = user.SteamNotifyOnSubscribed,
                SteamNotifyOnSpotlighted = user.SteamNotifyOnSpotlighted,
                SteamNotifyOnMissive = user.SteamNotifyOnMissive
            };
            if (user.SteamBotId == null)
            {
                editPage.SteamBotLost = true;
                editPage.SteamBotSteamId = (await SteamBindingHub.GetNextBotAsync(dbContext)).SteamId;
            }
            else
            {
                editPage.SteamBotName = $"其乐机器人 Keylol.com #{user.SteamBot.Sid}";
                editPage.SteamBotSteamId = user.SteamBot.SteamId;
            }
            int steamCnUid;
            if (int.TryParse(await userManager.GetSteamCnUidAsync(user.Id), out steamCnUid))
            {
                editPage.SteamCnUid = steamCnUid;
                editPage.SteamCnUserName = user.SteamCnUserName;
            }
            return editPage;
        }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Steam 机器人名称
        /// </summary>
        public string SteamBotName { get; set; }

        /// <summary>
        /// Steam 机器人 Steam ID 3
        /// </summary>
        public string SteamBotSteamId { get; set; }

        /// <summary>
        /// 是否与机器人失联
        /// </summary>
        public bool? SteamBotLost { get; set; }

        /// <summary>
        /// SteamCN 用户名
        /// </summary>
        public string SteamCnUserName { get; set; }

        /// <summary>
        /// SteamCN UID
        /// </summary>
        public int? SteamCnUid { get; set; }

        /// <summary>
        /// 登录保护
        /// </summary>
        public bool? LockoutEnabled { get; set; }

        /// <summary>
        /// 新窗口打开
        /// </summary>
        public bool? OpenInNewWindow { get; set; }

        /// <summary>
        /// 主选外语
        /// </summary>
        public bool? UseEnglishPointName { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 文章收到评论
        /// </summary>
        public bool? NotifyOnArticleReplied { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 评论被回复
        /// </summary>
        public bool? NotifyOnCommentReplied { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 动态收到评论
        /// </summary>
        public bool? NotifyOnActivityReplied { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 文章获得认可
        /// </summary>
        public bool? NotifyOnArticleLiked { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 评论获得认可
        /// </summary>
        public bool? NotifyOnCommentLiked { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 动态获得认可
        /// </summary>
        public bool? NotifyOnActivityLiked { get; set; }

        /// <summary>
        /// 邮政中心提醒 - 新听众
        /// </summary>
        public bool? NotifyOnSubscribed { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 文章收到评论
        /// </summary>
        public bool? SteamNotifyOnArticleReplied { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 评论被回复
        /// </summary>
        public bool? SteamNotifyOnCommentReplied { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 动态收到评论
        /// </summary>
        public bool? SteamNotifyOnActivityReplied { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 文章获得认可
        /// </summary>
        public bool? SteamNotifyOnArticleLiked { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 评论获得认可
        /// </summary>
        public bool? SteamNotifyOnCommentLiked { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 动态获得认可
        /// </summary>
        public bool? SteamNotifyOnActivityLiked { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 新听众
        /// </summary>
        public bool? SteamNotifyOnSubscribed { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 萃选推送
        /// </summary>
        public bool? SteamNotifyOnSpotlighted { get; set; }

        /// <summary>
        /// Steam 机器人提醒 - 系统公函
        /// </summary>
        public bool? SteamNotifyOnMissive { get; set; }
    }
}