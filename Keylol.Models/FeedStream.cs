using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public static class SlideshowStream
    {
        public static string Name => "slideshow";
        public static int Capacity => 100;

        public class FeedProperties
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Summary { get; set; }
            public string MinorTitle { get; set; }
            public string MinorSubtitle { get; set; }
        }
    }

    public class UserTimelineStream
    {
        public string Name(string userId) => $"u-{userId}";
        public static int Capacity => 500;
    }

    public class PointTimelineStream
    {
        public string Name(string userId) => $"p-{userId}";
        public static int Capacity => 500;
    }
}