using System;

namespace Keylol.Models
{
    public class Warning
    {
        public Warning()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public virtual KeylolUser Target { get; set; }
        public virtual KeylolUser Operator { get; set; }
    }
}