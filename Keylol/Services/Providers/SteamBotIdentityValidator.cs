using System.Data.Entity;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using Keylol.DAL;

namespace Keylol.Services.Providers
{
    public class SteamBotIdentityValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            using (var dbContext = new KeylolDbContext())
            {
                if (
                    dbContext.SteamBotManagers.SingleOrDefaultAsync(
                        manager => manager.ClientId == userName && manager.ClientSecret == password) != null)
                    return;
                throw new FaultException("Authentication failed.");
            }
        }
    }
}