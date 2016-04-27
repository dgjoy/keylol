using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using JetBrains.Annotations;

namespace Keylol.Utilities
{
    /// <summary>
    /// 自定义的 <see cref="IActionValueBinder"/>，额外实现了对 <see cref="NotNullAttribute"/> 的支持
    /// </summary>
    public class KeylolActionValueProvider : DefaultActionValueBinder
    {
        /// <summary>
        /// Gets the <see cref="T:System.Web.Http.Controllers.HttpParameterBinding"/> associated with the <see cref="T:System.Web.Http.ModelBinding.DefaultActionValueBinder"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Web.Http.Controllers.HttpParameterBinding"/> associated with the <see cref="T:System.Web.Http.ModelBinding.DefaultActionValueBinder"/>.
        /// </returns>
        /// <param name="parameter">The parameter descriptor.</param>
        protected override HttpParameterBinding GetParameterBinding(HttpParameterDescriptor parameter)
        {
            var binding = base.GetParameterBinding(parameter);
            if (parameter.GetCustomAttributes<NotNullAttribute>().Any())
                binding = new NotNullHttpParameterBinding(parameter, binding);
            return binding;
        }
    }
}