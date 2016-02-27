using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        /// <summary>
        ///     注册一个新用户
        /// </summary>
        /// <param name="vm">用户相关属性</param>
        [AllowAnonymous]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (LoginLogDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "邀请码无效")]
        public async Task<IHttpActionResult> CreateOne(UserPostVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var geetest = new Geetest();
            if (!await geetest.ValidateAsync(vm.GeetestChallenge, vm.GeetestSeccode, vm.GeetestValidate))
            {
                ModelState.AddModelError("authCode", "true");
                return BadRequest(ModelState);
            }
            var invitationCode = await DbContext.InvitationCodes.FindAsync(vm.InvitationCode);
            if (invitationCode == null || invitationCode.UsedByUser != null)
            {
                return Unauthorized();
            }
            var steamBindingToken = await DbContext.SteamBindingTokens.FindAsync(vm.SteamBindingTokenId);
            if (steamBindingToken == null)
            {
                ModelState.AddModelError("vm.SteamBindingTokenId", "Invalid steam binding token.");
                return BadRequest(ModelState);
            }
            if (await DbContext.Users.SingleOrDefaultAsync(u => u.SteamId == steamBindingToken.SteamId) != null)
            {
                ModelState.AddModelError("vm.SteamBindingTokenId",
                    "Steam account has been binded to another Keylol account.");
                return BadRequest(ModelState);
            }
            if (!Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "Only 5 uppercase letters and digits are allowed in IdCode.");
                return BadRequest(ModelState);
            }

            if (await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == vm.IdCode) != null ||
                !IsIdCodeLegit(vm.IdCode))
            {
                ModelState.AddModelError("vm.IdCode", "IdCode is already used by others.");
                return BadRequest(ModelState);
            }
            if (await UserManager.FindByNameAsync(vm.UserName) != null)
            {
                ModelState.AddModelError("vm.UserName", "UserName is already used by others.");
                return BadRequest(ModelState);
            }
            var user = new KeylolUser
            {
                IdCode = vm.IdCode,
                UserName = vm.UserName,
                RegisterIp = OwinContext.Request.RemoteIpAddress,
                AvatarImage = vm.AvatarImage,
                SteamBindingTime = DateTime.Now,
                SteamId = steamBindingToken.SteamId,
                SteamProfileName = vm.SteamProfileName,
                SteamBotId = steamBindingToken.BotId,
                InvitationCode = invitationCode
            };

            var result = await UserManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Contains("UserName"))
                        ModelState.AddModelError("vm.UserName", error);
                    else if (error.Contains("Password"))
                        ModelState.AddModelError("vm.Password", error);
                }
                return BadRequest(ModelState);
            }

            DbContext.SteamBindingTokens.Remove(steamBindingToken);
            user.SequenceNumber =
                await DbContext.Database.SqlQuery<int>("SELECT NEXT VALUE FOR [dbo].[UserSequence]").SingleAsync();
            await DbContext.SaveChangesAsync();

            // Auto login
            await SignInManager.SignInAsync(user, true, true);
            var loginLog = new LoginLog
            {
                Ip = OwinContext.Request.RemoteIpAddress,
                UserId = user.Id
            };
            DbContext.LoginLogs.Add(loginLog);
            await DbContext.SaveChangesAsync();

            return Created($"user/{user.Id}", new LoginLogDTO(loginLog));
        }

        private static bool IsIdCodeLegit(string idCode)
        {
            if (new[]
            {
                @"^([A-Z0-9])\1{4}$",
                @"^0000\d$",
                @"^\d0000$",
                @"^TEST.$",
                @"^.TEST$"
            }.Any(pattern => Regex.IsMatch(idCode, pattern)))
                return false;

            if (new[]
            {
                "12345",
                "54321",
                "ADMIN",
                "STAFF",
                "KEYLO",
                "KYLOL",
                "KEYLL",
                "VALVE",
                "STEAM",
                "CHINA",
                "JAPAN"
            }.Contains(idCode))
                return false;

            return true;
        }
    }
}