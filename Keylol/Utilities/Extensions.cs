using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Services;
using Microsoft.AspNet.Identity;

namespace Keylol.Utilities
{
    /// <summary>
    ///     一些常用对象扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     转换为 Unix 时间戳（秒）形式
        /// </summary>
        /// <param name="dateTime">要转换的 DateTime 对象</param>
        /// <returns>Unix 时间戳（秒）</returns>
        public static long ToTimestamp(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000)/10000000;
        }

        /// <summary>
        ///     从一个集合中取出指定数量元素的所以组合情况
        /// </summary>
        /// <param name="items">集合</param>
        /// <param name="count">取出元素数量</param>
        /// <typeparam name="T">集合类型</typeparam>
        /// <returns>全部组合</returns>
        public static IEnumerable<IEnumerable<T>> AllCombinations<T>(this IEnumerable<T> items, int count)
        {
            var i = 0;
            var list = items as IList<T> ?? items.ToList();
            foreach (var item in list)
            {
                if (count == 1)
                    yield return new[] {item};
                else
                {
                    foreach (var result in list.Skip(i + 1).AllCombinations(count - 1))
                        yield return new[] {item}.Concat(result);
                }
                i++;
            }
        }

        /// <summary>
        ///     将字符串转换为指定的 Enum 类型
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <typeparam name="TEnum">转换目标 Enum 类型</typeparam>
        /// <returns>如果转换成功，返回对应 Enum 值，如果失败，返回该 Enum 类型默认值</returns>
        public static TEnum ToEnum<TEnum>(this string text) where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse(text, out result) && Enum.IsDefined(typeof (TEnum), result))
                return result;
            return default(TEnum);
        }

        /// <summary>
        ///     设置 X-Total-Record-Count Header
        /// </summary>
        /// <param name="headers">HttpResponseHeaders 对象</param>
        /// <param name="totalCount">要设置的数值</param>
        public static void SetTotalCount(this HttpResponseHeaders headers, int totalCount)
        {
            if (headers.Contains("X-Total-Record-Count"))
                headers.Remove("X-Total-Record-Count");
            headers.Add("X-Total-Record-Count", totalCount.ToString());
        }

        /// <summary>
        ///     判断 Steam 机器人是否属于“在线”状态
        /// </summary>
        /// <param name="bot">Steam 机器人实体</param>
        /// <returns>是否在线</returns>
        public static bool IsOnline(this SteamBot bot)
        {
            return bot.Online && bot.SessionId != null &&
                   SteamBotCoordinator.Sessions.ContainsKey(bot.SessionId);
        }

        /// <summary>
        /// 为 ModelState 增加指定错误并返回 BadRequest
        /// </summary>
        /// <param name="controller"><see cref="ApiController"/> 对象</param>
        /// <param name="modelError">错误描述，最后一个出现的字符串将作为 errorMessage，其他字符串用 "." 拼接后作为 key</param>
        /// <returns><see cref="IHttpActionResult"/> 对象</returns>
        public static IHttpActionResult BadRequest(this ApiController controller, params string[] modelError)
        {
            controller.ModelState.AddModelError(string.Join(".", modelError.Take(modelError.Length - 1)),
                modelError.Last());
            return new InvalidModelStateResult(controller.ModelState, controller);
        }
    }

    public static class StatusClaim
    {
        public const string ClaimType = "status";
        public const string Probationer = "probationer";
        public const string Normal = null;

        /// null represents "normal"
        public static async Task<string> GetStatusClaimAsync(this KeylolUserManager manager, string userId)
        {
            return (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType)?.Value;
        }

        public static async Task<IdentityResult> RemoveStatusClaimAsync(this KeylolUserManager manager, string userId)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                return await manager.RemoveClaimAsync(userId, claim);
            }
            return new IdentityResult("User doesn't have any status claims.");
        }

        public static async Task<IdentityResult> SetStatusClaimAsync(this KeylolUserManager manager, string userId,
            string status)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                await manager.RemoveClaimAsync(userId, claim);
            }
            return await manager.AddClaimAsync(userId, new Claim(ClaimType, status));
        }
    }

    public static class StaffClaim
    {
        public const string ClaimType = "staff";
        public const string Manager = "manager";
        public const string Moderator = "moderator";
        public const string Operator = "operator";
        public const string User = null;

        /// null represents "user"
        public static async Task<string> GetStaffClaimAsync(this KeylolUserManager manager, string userId)
        {
            return (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType)?.Value;
        }

        public static async Task<IdentityResult> RemoveStaffClaimAsync(this KeylolUserManager manager, string userId)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                return await manager.RemoveClaimAsync(userId, claim);
            }
            return new IdentityResult("User doesn't have any staff claims.");
        }

        public static async Task<IdentityResult> SetStaffClaimAsync(this KeylolUserManager manager, string userId,
            string staff)
        {
            var claim = (await manager.GetClaimsAsync(userId)).SingleOrDefault(c => c.Type == ClaimType);
            if (claim != null)
            {
                await manager.RemoveClaimAsync(userId, claim);
            }
            return await manager.AddClaimAsync(userId, new Claim(ClaimType, staff));
        }
    }
}