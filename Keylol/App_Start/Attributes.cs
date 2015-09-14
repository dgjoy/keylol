using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
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
}