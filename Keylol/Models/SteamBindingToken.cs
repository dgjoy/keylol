using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.DAL;

namespace Keylol.Models
{
    public class SteamBindingToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(8)]
        [Index(IsUnique = true)]
        public string Code { get; set; }

        [Required]
        [Index]
        [MaxLength(128)]
        public string BrowserConnectionId { get; set; }

        public string SteamId { get; set; }

        [Required]
        public string BotId { get; set; }

        public virtual SteamBot Bot { get; set; }

        public static async Task<string> GenerateCodeAsync(KeylolDbContext dbContext)
        {
            string code;
            var random = new Random();
            do
            {
                var sb = new StringBuilder();
                for (var i = 0; i < 4; i++)
                {
                    sb.Append((char)random.Next('A', 'Z'));
                }
                for (var i = 0; i < 4; i++)
                {
                    sb.Append(random.Next(0, 10));
                }
                code = sb.ToString();
            } while ((await dbContext.SteamBindingTokens.SingleOrDefaultAsync(token => token.Code == code)) != null);
            return code;
        }
    }
}
