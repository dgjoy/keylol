using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Services;
using Keylol.Utilities;

namespace Keylol.Controllers.SteamBot
{
    public partial class SteamBotController
    {
        /// <summary>
        /// 检查机器人好友列表和数据库暂准状态是否同步，返回不同步的用户列表
        /// </summary>
        /// <param name="tryFix">是否尝试修复，默认 false</param>
        /// <returns></returns>
        [Route("sync")]
        [HttpGet]
        public async Task<IHttpActionResult> CheckSync(bool tryFix = false)
        {
            var markedUsers = new HashSet<string>();
            var botFriendsToRemove = new List<string>();
            var usersToRemoveStatusClaim = new List<string>();
            var bots = await DbContext.SteamBots.ToListAsync();
            foreach (var bot in bots)
            {
                if (!bot.IsOnline())
                    continue;
                var client = SteamBotCoordinator.Sessions[bot.SessionId].Client;
                foreach (var steamId in await client.GetFriendList(bot.Id))
                {
                    var user = await DbContext.Users.Where(u => u.SteamId == steamId && u.SteamBotId == bot.Id)
                        .Select(u => new
                        {
                            u.Id,
                            u.IdCode
                        })
                        .SingleOrDefaultAsync();
                    if (user == null)
                    {
                        botFriendsToRemove.Add($"{bot.SequenceNumber} {steamId}");
                        if (tryFix)
                            await client.RemoveFriend(bot.Id, steamId);
                    }
                    else if (tryFix)
                    {
                        await UserManager.RemoveStatusClaimAsync(user.Id);
                    }
                    markedUsers.Add(steamId);
                }
            }
            var usersWithoutBotFriend = await DbContext.Users.Where(u => !markedUsers.Contains(u.SteamId))
                .Select(u => new
                {
                    u.Id,
                    u.IdCode
                })
                .ToListAsync();
            var usersToAddStatusClaim = new List<string>();
            if (tryFix)
            {
                foreach (var user in usersWithoutBotFriend)
                {
                    await UserManager.SetStatusClaimAsync(user.Id, StatusClaim.Probationer);
                }
            }
            await DbContext.SaveChangesAsync();
            return Ok(new
            {
                botFriendsToRemove,
                usersToRemoveStatusClaim,
                usersWithoutBotFriend = usersWithoutBotFriend.Select(u => u.IdCode),
                usersToAddStatusClaim
            });
        }
    }
}