namespace Keylol.States.PostOffice.SocialActivity
{
    /// <summary>
    /// 邮政中心 - 社交
    /// </summary>
    public class SocialActivityLevel
    {
        /// <summary>
        /// 评论
        /// </summary>
        public CommentPage Comment { get; set; }

        /// <summary>
        /// 认可
        /// </summary>
        public LikePage Like { get; set; }

        /// <summary>
        /// 听众
        /// </summary>
        public SubscriberPage Subscriber { get; set; }
    }
}