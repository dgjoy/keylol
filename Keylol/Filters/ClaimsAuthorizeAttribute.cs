using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JetBrains.Annotations;

namespace Keylol.Filters
{
    /// <summary>
    /// 验证请求 <see cref="IPrincipal"/> 是否具备指定 Claim
    /// </summary>
    public class ClaimsAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private readonly string _claimType;
        private readonly string _claimValue;

        /// <summary>
        /// 验证请求 <see cref="IPrincipal"/> 是否具备指定 Claim
        /// </summary>
        /// <param name="claimType">Claim 类型</param>
        /// <param name="claimValue">Claim 值</param>
        public ClaimsAuthorizeAttribute([NotNull] string claimType, [NotNull] string claimValue)
        {
            _claimType = claimType;
            _claimValue = claimValue;
        }

        /// <summary>
        /// 认证时触发
        /// </summary>
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (SkipAuthorization(actionContext))
                return Task.FromResult(0);

            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            if (principal?.Identity == null ||
                !principal.Identity.IsAuthenticated ||
                !principal.HasClaim(_claimType, _claimValue))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            return Task.FromResult(0);
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
                   actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>()
                       .Any();
        }
    }
}