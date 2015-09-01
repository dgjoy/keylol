using System;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models
{
    public abstract class Log
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class LoginLog : Log
    {
        [Required]
        [MaxLength(64)]
        public string Ip { get; set; }

        [Required]
        public virtual KeylolUser User { get; set; }
    }

    public class EditLog : Log
    {
        [Required]
        public virtual Article Article { get; set; }

        [Required]
        public virtual KeylolUser Editor { get; set; }

        [Required]
        [MaxLength(120)]
        public string OldTitle { get; set; }

        [Required]
        [MaxLength(300000)]
        public string OldContent { get; set; }
    }
}