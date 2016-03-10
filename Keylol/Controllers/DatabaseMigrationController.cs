using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;

namespace Keylol.Controllers
{
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        // 迁移方法需要保证幂等性

        /// <summary>
        ///     v1.1.8: 新增一批机器人
        /// </summary>
        [Route("v1-1-8-add-new-bots")]
        [HttpPost]
        public async Task<IHttpActionResult> V118AddNewBots()
        {
            var credentials =
                @"keylol_bot_6 RsHkuSXVF5wdNtVPh3kjBqR5|keylol_bot_7 h8VRuC8SygpX4Vk7FQKpW84y|keylol_bot_8 u7jgf4GWbzBGtUukp9f4fvSu|keylol_bot_9 mgNwtCSu7rVuynushQYpBGwe|keylol_bot_10 YMcKXQ5Ex8EXRSj68qVay85h|keylol_bot_11 cZKW7u6ePzqsqtG6ezkNPXbD|keylol_bot_12 4QUJ5RfxGj7ENu7PVRfyTazG|keylol_bot_13 BEmF5dHFw3dRTUuvDH9aeDg5|keylol_bot_14 NraAHQGa8Zr533hVVWLzFs6S|keylol_bot_15 NVnbSP4H5MbYpX9pLscUr2Nb|keylol_bot_16 AYvcr2awpftbscTQP3q6rDwP|keylol_bot_17 h724fKzxc3rpDRZChTY7SyTm|keylol_bot_18 rVS7aNEhmMGZrdezwTd6ALUP|keylol_bot_19 pHWcwx4HfPXY5EzKHrHMkYtc|keylol_bot_20 qHbdqPY9ZNXCRNsuXkQaHGdC|keylol_bot_21 7Gv6sFdcXjbjeyJMTSbdwyDs|keylol_bot_22 pRDVBDtAnyQywgJk8M22Sazd|keylol_bot_23 Z9WwZTaGavLdpKtaXvnjRAQF|keylol_bot_24 XafHwYEcvvvFR8jWDE9L73QP|keylol_bot_25 a49m9THX2f4352tmAS7ywvtm|keylol_bot_26 cdpnLWtvCgZcgNdmk4p8ppfB|keylol_bot_27 WyUPgadctuDSg4yVgJnEFxFW|keylol_bot_28 aN4MCsfBn5PEBVnnvcpMhnbM|keylol_bot_29 3Eb9zgG87bE2MZwEKKHQwDAu|keylol_bot_30 PFHmBWnXbMZLPpZMdPPnG6hX|keylol_bot_31 duGxff6bxnVk76C29pR5KhdV|keylol_bot_32 tq4g7sbzETHjXunWJ6fWe2yK|keylol_bot_33 2sKRE6mYQtKNkGZFhdetNhAz|keylol_bot_34 ePxUAJ29fuYZvvg3mAhBCD5b|keylol_bot_35 eMNGAAuGfGyFryeQ4UVj9sm9|keylol_bot_36 WmtKkA7Wu6VJNKkXUEGuUBg9|keylol_bot_37 BdULsUBJKNVAeFGSwNWdAqCg|keylol_bot_38 sLCjWZafmWzfCu5acw5e5mhA|keylol_bot_39 3x7PaaSRVhhzUf3Pc7WHJvMg|keylol_bot_40 AEz4WyDVhkN2aGEPK478fqmp|keylol_bot_41 LwcskHtpEGZ9n5xtHNv2D6yS|keylol_bot_42 JwK3KnkH5mnZ5LJFAFFtj54h|keylol_bot_43 qA9phBKkVP6SfeLKhyK7jxJk|keylol_bot_44 zG43rzqe5aU6GyMzRuTaxj3Y|keylol_bot_45 GAZ4HtuP36uVHCmvjXmb6Yd8"
                    .Split('|').Select(s =>
                    {
                        var parts = s.Split(' ');
                        return new
                        {
                            UserName = parts[0],
                            Password = parts[1]
                        };
                    });

            foreach (var credential in credentials)
            {
                DbContext.SteamBots.AddOrUpdate(bot => bot.SteamUserName, new Models.SteamBot
                {
                    SteamUserName = credential.UserName,
                    SteamPassword = credential.Password,
                    FriendUpperLimit = 200
                });
            }
            await DbContext.SaveChangesAsync();
            return Ok("迁移成功");
        }
    }
}