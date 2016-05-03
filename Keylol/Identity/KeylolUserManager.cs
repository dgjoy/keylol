using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Identity.MessageServices;
using Keylol.Models;
using Keylol.Models.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Identity
{
    /// <summary>
    ///     ASP.NET Identity UserManager Keylol implementation
    /// </summary>
    public class KeylolUserManager : UserManager<KeylolUser>
    {
        /// <summary>
        ///     创建 <see cref="KeylolUserManager" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        public KeylolUserManager(KeylolDbContext dbContext) : base(new UserStore<KeylolUser>(dbContext))
        {
            ClaimsIdentityFactory = new ClaimsIdentityFactory<KeylolUser>
            {
                UserIdClaimType = KeylolClaimTypes.UserId,
                UserNameClaimType = KeylolClaimTypes.UserName,
                RoleClaimType = KeylolClaimTypes.Role,
                SecurityStampClaimType = KeylolClaimTypes.SecurityStamp
            };

            UserValidator = new KeylolUserValidator(this);
            PasswordValidator = new KeylolPasswordValidator();

            UserLockoutEnabledByDefault = false;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            MaxFailedAccessAttemptsBeforeLockout = 10;

            SteamChatMessageService = new KeylolSteamChatMessageService();
            EmailService = new KeylolEmailService();
            SmsService = new KeylolSmsService();
        }

        /// <summary>
        ///     Used to send Steam chat message
        /// </summary>
        public IIdentityMessageService SteamChatMessageService { get; set; }

        /// <summary>
        ///     Create a user with the given password
        /// </summary>
        /// <param name="user" />
        /// <param name="password" />
        /// <returns />
        public override Task<IdentityResult> CreateAsync(KeylolUser user, string password)
        {
            user.ProfilePoint = new ProfilePoint();
            return base.CreateAsync(user, password);
        }

        /// <summary>
        ///     根据识别码查询用户
        /// </summary>
        /// <param name="idCode">识别码</param>
        /// <returns>查询到的用户对象，或者 null 表示没有查到</returns>
        [ItemCanBeNull]
        public async Task<KeylolUser> FindByIdCodeAsync(string idCode)
        {
            if (!SupportsQueryableUsers)
                throw new NotSupportedException();
            if (idCode == null)
                throw new ArgumentNullException(nameof(idCode));
            return await Users.Where(u => u.IdCode == idCode).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     根据识别码查询用户
        /// </summary>
        /// <param name="steamId">Steam ID 3</param>
        /// <returns>查询到的用户对象，或者 null 表示没有查到</returns>
        /// <exception cref="ArgumentNullException">steamId 为 null</exception>
        [ItemCanBeNull]
        public async Task<KeylolUser> FindBySteamIdAsync(string steamId)
        {
            if (steamId == null)
                throw new ArgumentNullException(nameof(steamId));
            return await FindAsync(new UserLoginInfo(KeylolLoginProviders.Steam, steamId));
        }

        /// <summary>
        ///     获取指定用户的 Steam ID
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>用户的 Steam ID 3</returns>
        public async Task<string> GetSteamIdAsync(string userId)
        {
            return (await GetLoginsAsync(userId))
                .FirstOrDefault(l => l.LoginProvider == KeylolLoginProviders.Steam)?
                .ProviderKey;
        }

        /// <summary>
        ///     命令机器人向用户发送一条 Steam 聊天消息
        /// </summary>
        /// <param name="user"><see cref="KeylolUser" /> 用户对象</param>
        /// <param name="message">聊天消息内容</param>
        /// <param name="tempSilence">是否在两分钟内关闭机器人的自动回复（图灵机器人）</param>
        /// <returns></returns>
        public async Task SendSteamChatMessageAsync(KeylolUser user, string message, bool tempSilence = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (user.SteamBotId != null && SteamChatMessageService != null)
            {
                var msg = new IdentityMessage
                {
                    Destination = await GetSteamIdAsync(user.Id),
                    Subject = $"{user.SteamBotId},{tempSilence}",
                    Body = message
                };
                await SteamChatMessageService.SendAsync(msg);
            }
        }
    }
}