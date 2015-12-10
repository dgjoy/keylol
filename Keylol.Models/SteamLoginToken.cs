using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models.DAL;

namespace Keylol.Models
{
    public class SteamLoginToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(4)]
        [Index(IsUnique = true)]
        public string Code { get; set; }

        [Required]
        [Index]
        [MaxLength(128)]
        public string BrowserConnectionId { get; set; }
        
        [MaxLength(64)]
        [Index]
        public string SteamId { get; set; }

        public static async Task<string> GenerateCodeAsync(KeylolDbContext dbContext)
        {
            string code;
            var random = new Random();
            do
            {
                code = random.Next(0, 10000).ToString("D4");
            } while ((await dbContext.SteamLoginTokens.SingleOrDefaultAsync(token => token.Code == code)) != null);
            return code;
        }
    }
}
