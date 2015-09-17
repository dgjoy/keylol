using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.DAL;
using Keylol.Models;
using Keylol.Models.DTO;

namespace Keylol.Controllers
{
    [Route("steam-binding-token")]
    public class SteamBindingTokenController : KeylolApiController
    {
        public async Task<IHttpActionResult> Post()
        {
            var token = new SteamBindingToken {Code = await SteamBindingToken.GenerateCodeAsync(DbContext)};
            DbContext.SteamBindingTokens.Add(token);
            await DbContext.SaveChangesAsync();
            DelayDeleteToken(token);
            return Created($"steam-binding-token/{token.Id}", new SteamBindingTokenDTO(token));
        }

        private static void DelayDeleteToken(SteamBindingToken token)
        {
            Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(async task =>
            {
                if (token.SteamId == null)
                {
                    using (var dbContext = new KeylolDbContext())
                    {
                        dbContext.SteamBindingTokens.Attach(token);
                        dbContext.SteamBindingTokens.Remove(token);
                        await dbContext.SaveChangesAsync();
                    }
                }
            });
        }
    }
}