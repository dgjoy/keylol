using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace Keylol.Utilities
{
    /// <summary>
    ///     要求被绑定的参数在实际的 <see cref="HttpParameterBinding" /> 执行之后不能为 <c>null</c>
    /// </summary>
    public class NotNullHttpParameterBinding : HttpParameterBinding
    {
        private readonly HttpParameterBinding _actualBinding;

        /// <summary>
        ///     创建 <see cref="NotNullHttpParameterBinding" />
        /// </summary>
        /// <param name="descriptor">
        ///     <see cref="HttpParameterDescriptor" />
        /// </param>
        /// <param name="actualBinding">实际要执行的 <see cref="HttpParameterBinding" /></param>
        public NotNullHttpParameterBinding(HttpParameterDescriptor descriptor, HttpParameterBinding actualBinding)
            : base(descriptor)
        {
            _actualBinding = actualBinding;
        }

        /// <summary>
        ///     Returns a value indicating whether this <see cref="T:System.Web.Http.Controllers.HttpParameterBinding" /> instance
        ///     will read the entity body of the HTTP message.
        /// </summary>
        /// <returns>
        ///     true if this <see cref="T:System.Web.Http.Controllers.HttpParameterBinding" /> will read the entity body;
        ///     otherwise, false.
        /// </returns>
        public override bool WillReadBody => _actualBinding.WillReadBody;

        /// <summary>
        ///     If the binding is invalid, gets an error message that describes the binding error.
        /// </summary>
        /// <returns>
        ///     An error message. If the binding was successful, the value is null.
        /// </returns>
        public override string ErrorMessage => _actualBinding.ErrorMessage;

        /// <summary>
        ///     Asynchronously executes the binding for the given request.
        /// </summary>
        /// <returns>
        ///     A task object representing the asynchronous operation.
        /// </returns>
        /// <param name="metadataProvider">Metadata provider to use for validation.</param>
        /// <param name="actionContext">
        ///     The action context for the binding. The action context contains the parameter dictionary
        ///     that will get populated with the parameter.
        /// </param>
        /// <param name="cancellationToken">Cancellation token for cancelling the binding operation.</param>
        public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
            HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            await _actualBinding.ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
            if (actionContext.ActionArguments[Descriptor.ParameterName] == null)
            {
                actionContext.ModelState.AddModelError(Descriptor.ParameterName, Errors.Required);
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    actionContext.ModelState);
            }
        }
    }
}