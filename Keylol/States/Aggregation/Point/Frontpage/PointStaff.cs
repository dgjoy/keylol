namespace Keylol.States.Aggregation.Point.Frontpage
{
    /// <summary>
    /// 据点职员
    /// </summary>
    public class PointStaff
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// 是否是好友
        /// </summary>
        public bool? IsFriend { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}