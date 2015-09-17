using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace Keylol.Services.Providers
{
    public class SteamBotIdentityValidator: UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (userName == "keylol" && password == "hahaha")
                return;
            throw new FaultException("haha you are not allowed");
        }
    }
}
