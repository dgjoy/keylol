using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Keylol.Services.Validators
{
    /// <summary>
    ///     客户端身份认证
    /// </summary>
    public class ClientValidator : UserNamePasswordValidator
    {
        /// <summary>
        ///     用户名密码认证
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public override void Validate(string userName, string password)
        {
            if (userName == "keylol-service-consumer" && password == "neLFDyJB8Vj2Xtsn2KMTUEFw")
                return;
            throw new MessageSecurityException("Authentication failed.",
                new FaultException("ClientId or ClientSecret is not correct."));
        }
    }
}