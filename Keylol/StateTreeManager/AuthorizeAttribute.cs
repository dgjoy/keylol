using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Keylol.StateTreeManager
{
    /// <summary>
    /// 要求认证才可访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AuthorizeAttribute : Attribute
    {
        private static readonly string[] EmptyArray = new string[0];
        private string[] _rolesSplit = EmptyArray;
        private string[] _usersSplit = EmptyArray;
        private string _roles;
        private string _users;

        /// <summary>
        /// 允许的角色列表，逗号分隔
        /// </summary>
        public string Roles
        {
            get { return _roles ?? string.Empty; }
            set
            {
                _roles = value;
                _rolesSplit = SplitString(value);
            }
        }

        /// <summary>
        /// 允许的用户列表，逗号分隔
        /// </summary>
        public string Users
        {
            get { return _users ?? string.Empty; }
            set
            {
                _users = value;
                _usersSplit = SplitString(value);
            }
        }

        /// <summary>
        /// 验证当前用户是否满足授权要求
        /// </summary>
        /// <param name="context">当前 <see cref="IOwinContext"/></param>
        /// <returns>如果满足授权要求，返回 <c>true</c></returns>
        /// <exception cref="ArgumentNullException">参数 context 为 null</exception>
        public Task<bool> AuthorizeAsync(IOwinContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var principal = context.Request.User;
            var result = principal?.Identity != null && principal.Identity.IsAuthenticated &&
                         (_usersSplit.Length <= 0 ||
                          _usersSplit.Contains(principal.Identity.Name, StringComparer.OrdinalIgnoreCase)) &&
                         (_rolesSplit.Length <= 0 || _rolesSplit.Any(principal.IsInRole));
            return Task.FromResult(result);
        }

        private static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
                return EmptyArray;
            return original.Split(',').Select(piece => new
            {
                piece,
                trimmed = piece.Trim()
            }).Where(p => !string.IsNullOrEmpty(p.trimmed)).Select(p => p.trimmed).ToArray();
        }
    }

    /// <summary>
    /// 允许匿名访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowAnonymousAttribute : Attribute
    {
    }

    /// <summary>
    /// 表示该参数由服务容器注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InjectedAttribute : Attribute
    {
    }
}