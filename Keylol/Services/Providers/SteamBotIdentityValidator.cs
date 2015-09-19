using System.Data.Entity;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using Keylol.DAL;

namespace Keylol.Services.Providers
{
    public class SteamBotIdentityValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            using (var dbContext = new KeylolDbContext())
            {
                if (userName == "keylol-bot" && password == "neLFDyJB8Vj2Xtsn2KMTUEFw")
                    return;
                throw new MessageSecurityException("Authentication failed.",
                    new FaultException("ClientId or ClientSecret is not correct."));
            }
        }
    }
}