using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Subscription
{
    public partial class SubscriptionController
    {
        /// <summary>
        /// 创建一个订阅
        /// </summary>
        /// <param name="targetId">目标 ID</param>
        /// <param name="targetType">目标类型</param>
        [Route]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOne(string targetId, SubscriptionTargetType targetType)
        {
            var subscriberId = User.Identity.GetUserId();
            bool existed;
            KeylolUser targetUser = null;
            switch (targetType)
            {
                case SubscriptionTargetType.Point:
                    existed = await _dbContext.Points.AnyAsync(p => p.Id == targetId);
                    break;

                case SubscriptionTargetType.User:
                    if (subscriberId.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                        return this.BadRequest(nameof(targetId), Errors.Invalid);
                    targetUser = await _userManager.FindByIdAsync(targetId);
                    existed = targetUser != null;
                    break;

                case SubscriptionTargetType.Conference:
                    existed = await _dbContext.Conferences.AnyAsync(c => c.Id == targetId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
            if (!existed)
                return this.BadRequest(nameof(targetId), Errors.NonExistent);
            var succeed = await _cachedData.Subscriptions.AddAsync(subscriberId, targetId, targetType);
            if (targetType == SubscriptionTargetType.User && succeed)
            {
                Debug.Assert(targetUser != null, "targetUser != null");
                var subscriberCount =
                    (int) await _cachedData.Subscriptions.GetSubscriberCountAsync(targetId, targetType);
                if (targetUser.NotifyOnSubscribed)
                {
                    _dbContext.Messages.Add(new Message
                    {
                        Type = MessageType.NewSubscriber,
                        OperatorId = subscriberId,
                        ReceiverId = targetId,
                        Count = subscriberCount
                    });
                    await _dbContext.SaveChangesAsync();
                }
                if (targetUser.SteamNotifyOnSubscribed)
                {
                    var subscriber = await _userManager.FindByIdAsync(subscriberId);
                    await _userManager.SendSteamChatMessageAsync(targetUser,
                        await _cachedData.Users.IsFriendAsync(subscriberId, targetId)
                            ? $"用户 {subscriber.UserName} 成为了你的第 {subscriberCount} 位听众，并开始与你互相关注。换句话说，你们已经成为好友啦！"
                            : $"用户 {subscriber.UserName} 关注了你并成为你的第 {subscriberCount} 位听众，你们之间互相关注后会成为好友。");
                }
            }
            return Ok();
        }
    }
}