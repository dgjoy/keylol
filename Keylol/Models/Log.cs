using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public abstract class Log
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index]
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class LoginLog : Log
    {
        [Required]
        [Index]
        [MaxLength(64)]
        public string Ip { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual KeylolUser User { get; set; }
    }

    public class EditLog : Log
    {
        [Required]
        public string ArticleId { get; set; }

        public virtual Article Article { get; set; }

        [Required]
        public string EditorId { get; set; }

        public virtual KeylolUser Editor { get; set; }

        [Required]
        [MaxLength(120)]
        public string OldTitle { get; set; }

        [Required]
        [MaxLength(300000)]
        public string OldContent { get; set; }
    }
}