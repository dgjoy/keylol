using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace Keylol.Utilities
{
    /// <summary>
    /// SignalR User ID Provider
    /// </summary>
    public class KeylolUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// 获取用户 ID
        /// </summary>
        /// <param name="request">SignarR Request</param>
        /// <returns>用户 ID</returns>
        public string GetUserId(IRequest request)
        {
            return request.User?.Identity?.GetUserId();
        }
    }
}