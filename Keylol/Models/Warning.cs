using System;

namespace Keylol.Models
{
    public class Warning
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public virtual KeylolUser Target { get; set; }
        public virtual KeylolUser Operator { get; set; }
    }
}