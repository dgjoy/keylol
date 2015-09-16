using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Filters;

namespace Keylol
{
    public class ClaimsAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private readonly string _claimType;
        private readonly string _claimValue;

        public ClaimsAuthorizeAttribute(string claimType, string claimValue)
        {
            _claimType = claimType;
            _claimValue = claimValue;
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(0);
            }

            if ((_claimValue == null && principal.HasClaim(x => x.Type == _claimType)) ||
                (_claimValue != null && !principal.HasClaim(x => x.Type == _claimType && x.Value == _claimValue)))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(0);
            }

            //User is Authorized, complete execution
            return Task.FromResult(0);
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class EnableCorsRegexAttribute : Attribute, ICorsPolicyProvider
    {
        private readonly string _originPattern;

        public bool SupportsCredentials { get; set; } = false;

        public EnableCorsRegexAttribute(string originPattern)
        {
            _originPattern = originPattern;
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corsRequestContext = request.GetCorsRequestContext();
            var policy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                SupportsCredentials = SupportsCredentials,
                PreflightMaxAge = 365 * 24 * 3600
            };
            if (Regex.IsMatch(corsRequestContext.Origin, _originPattern, RegexOptions.IgnoreCase))
            {
                policy.Origins.Add(corsRequestContext.Origin);
            }
            return Task.FromResult(policy);
        }
    }
}