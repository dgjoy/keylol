using Keylol.States.PostOffice.SocialActivity;

namespace Keylol.States.PostOffice
{
    /// <summary>
    /// 邮政中心层级
    /// </summary>
    public class PostOfficeLevel
    {
        /// <summary>
        /// 未读
        /// </summary>
        public UnreadPage Unread { get; set; }

        /// <summary>
        /// 社交
        /// </summary>
        public SocialActivityLevel SocialActivity { get; set; }

        /// <summary>
        /// 公函
        /// </summary>
        public MissivePage Missive { get; set; }
    }
}