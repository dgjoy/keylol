using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Conference
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }
    }
}
