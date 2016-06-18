using System;
using System.Data.Entity;
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
            switch (targetType)
            {
                case SubscriptionTargetType.Point:
                    existed = await _dbContext.Points.AnyAsync(p => p.Id == targetId);
                    break;

                case SubscriptionTargetType.User:
                    if (subscriberId.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                        return this.BadRequest(nameof(targetId), Errors.Invalid);
                    existed = await _dbContext.Users.AnyAsync(u => u.Id == targetId);
                    break;

                case SubscriptionTargetType.Conference:
                    existed = await _dbContext.Conferences.AnyAsync(c => c.Id == targetId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
            if (!existed)
                return this.BadRequest(nameof(targetId), Errors.NonExistent);
            await _cachedData.Subscriptions.AddAsync(subscriberId, targetId, targetType);
            return Ok();
        }
    }
}