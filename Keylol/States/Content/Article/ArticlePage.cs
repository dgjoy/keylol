using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Keylol.States.Aggregation.Point.BasicInfo;

namespace Keylol.States.Content.Article
{
    /// <summary>
    /// 内容 - 文章
    /// </summary>
    public class ArticlePage
    {
        public BasicInfo PointBasicInfo { get; set; }

        public string AuthorIdCode { get; set; }

        public string AuthorAvatarImage { get; set; }

        public string AuthorUserName { get; set; }

        public long AuthorFriendCount { get; set; }

        public long AuthorSubscriptionCount { get; set; }

        public long AuthorSubscriberCount { get; set; }

        public string AuthorSteamProfileName { get; set; }
    }
}