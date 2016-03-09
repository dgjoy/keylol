using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public enum AutoSubscriptionType
    {
        MostPlayed,
        RecentPlayed,
        Genre,
        Manufacture
    }

    public class AutoSubscription
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        public KeylolUser User { get; set; }

        [Required]
        public string NormalPointId { get; set; }

        public virtual NormalPoint NormalPoint { get; set; }

        public AutoSubscriptionType Type { get; set; }

        [Index]
        public int DisplayOrder { get; set; }
    }
}