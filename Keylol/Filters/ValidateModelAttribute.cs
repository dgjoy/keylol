using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JetBrains.Annotations;
using Keylol.Utilities;

namespace Keylol.Filters
{
    /// <summary>
    ///     验证 Model 正确性（ModelState.IsValid 和 [NotNull]）
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     Action 执行前触发
        /// </summary>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            foreach (var parameter in actionContext.ActionDescriptor.GetParameters().Where(p =>
                p.GetCustomAttributes<NotNullAttribute>().Any() &&
                actionContext.ActionArguments[p.ParameterName] == null))
            {
                actionContext.ModelState.AddModelError(parameter.ParameterName, Errors.Required);
            }
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}