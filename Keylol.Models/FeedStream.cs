using System;
using System.Collections.Generic;

namespace Keylol.Models
{
    public static class SlideshowStream
    {
        public static string Name => "slideshow";

        public class FeedProperties
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Author { get; set; }
            public string Date { get; set; }
            public string MinorTitle { get; set; }
            public string MinorSubtitle { get; set; }
            public string BackgroundImage { get; set; }
            public string Link { get; set; }
        }
    }

    public static class SpotlightPointStream
    {
        public static string Name => "spotlight-point";
    }

    public static class SpotlightArticleStream
    {
        public static string Name(ArticleCategory category)
        {
            switch (category)
            {
                case ArticleCategory.Review:
                    return "spotlight-review";

                case ArticleCategory.Study:
                    return "spotlight-study";

                case ArticleCategory.Story:
                    return "spotlight-story";

                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        /// <summary>
        /// Spotlight Article 可选分类
        /// </summary>
        public enum ArticleCategory
        {
            /// <summary>
            /// 评
            /// </summary>
            Review,

            /// <summary>
            /// 研
            /// </summary>
            Study,

            /// <summary>
            /// 谈
            /// </summary>
            Story
        }

        public class FeedProperties
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
        }
    }

    public static class OnSalePointStream
    {
        public static string Name => "on-sale-point";
    }

    public static class OutpostStream
    {
        public static string Name => "outpost";
    }

    public static class UserStream
    {
        public static string Name(string userId) => $"u:{userId}";
    }

    public static class PointStream
    {
        public static string Name(string pointId) => $"p:{pointId}";
    }

    public static class SubscriptionStream
    {
        public static string Name(string userId) => $"s:{userId}";

        public class FeedProperties
        {
            public List<string> Reasons { get; set; }
        }
    }
}