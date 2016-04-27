using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        private KeylolUserManager(IUserStore<KeylolUser> store) : base(store)
        {
            ClaimsIdentityFactory = new ClaimsIdentityFactory<KeylolUser>
            {
                UserIdClaimType = KeylolClaimTypes.UserId,
                UserNameClaimType = KeylolClaimTypes.UserName,
                RoleClaimType = KeylolClaimTypes.Role,
                SecurityStampClaimType = KeylolClaimTypes.SecurityStamp
            };

            UserValidator = new KeylolUserValidator(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            PasswordValidator = new PasswordValidator
            {
                RequireDigit = false,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false,
                RequiredLength = 6
            };

            UserLockoutEnabledByDefault = false;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            MaxFailedAccessAttemptsBeforeLockout = 10;

            SteamChatMessageService = new KeylolSteamChatMessageService();
            EmailService = new KeylolEmailService();
            SmsService = new KeylolSmsService();
        }

        /// <summary>
        ///     创建 <see cref="KeylolUserManager" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        public KeylolUserManager(KeylolDbContext dbContext) : this(new UserStore<KeylolUser>(dbContext))
        {
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
        public async Task<KeylolUser> FindBySteamIdAsync(string steamId)
        {
            if (!SupportsQueryableUsers)
                throw new NotSupportedException();
            if (steamId == null)
                throw new ArgumentNullException(nameof(steamId));
            return await Users.Where(u => u.SteamId == steamId).FirstOrDefaultAsync();
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
            if (SteamChatMessageService != null)
            {
                var msg = new IdentityMessage
                {
                    Destination = user.SteamId,
                    Subject = $"{user.SteamBotId},{tempSilence}",
                    Body = message
                };
                await SteamChatMessageService.SendAsync(msg);
            }
        }
    }
}