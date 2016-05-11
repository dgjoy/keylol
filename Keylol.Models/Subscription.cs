using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Subscription
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [Required]
        public string SubscriberId { get; set; }

        public KeylolUser Subscriber { get; set; }

        public SubscriptionTargetType TargetType { get; set; }

        [Index]
        [Required]
        [MaxLength(128)]
        public string TargetId { get; set; }
    }

    public enum SubscriptionTargetType
    {
        Point,
        User,
        Conference
    }
}