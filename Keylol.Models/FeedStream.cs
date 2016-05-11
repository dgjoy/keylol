namespace Keylol.Models
{
    public static class SlideshowStream
    {
        public static string Name => "slideshow";
        public static int Capacity => 30;

        public class FeedProperties
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Summary { get; set; }
            public string MinorTitle { get; set; }
            public string MinorSubtitle { get; set; }
            public string BackgroundImage { get; set; }
            public string Link { get; set; }
        }
    }

    public static class SpotlightPointStream
    {
        public static string Name => "spotlight-point";
        public static int Capacity => 150;
    }

    public static class UserTimelineStream
    {
        public static string Name(string userId) => $"u-{userId}";
        public static int Capacity => 500;
    }

    public static class PointTimelineStream
    {
        public static string Name(string userId) => $"p-{userId}";
        public static int Capacity => 500;
    }
}