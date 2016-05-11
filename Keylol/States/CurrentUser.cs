using Keylol.Models;

namespace Keylol.States
{
    /// <summary>
    /// 当前登录的用户
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// 用户名（昵称）
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 未读邮政消息数
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// 文券
        /// </summary>
        public int Coupon { get; set; }

        /// <summary>
        /// 草稿数
        /// </summary>
        public int DraftCount { get; set; }

        /// <summary>
        /// 据点主显名称偏好
        /// </summary>
        public PreferredPointName PreferredPointName { get; set; }
    }
}