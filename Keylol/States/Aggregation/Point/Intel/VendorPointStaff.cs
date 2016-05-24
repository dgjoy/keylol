namespace Keylol.States.Aggregation.Point.Intel
{
    /// <summary>
    /// 据点职员
    /// </summary>
    public class VendorPointStaff
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 头部图
        /// </summary>
        public string HeaderImage { get; set; }

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
        /// 管理据点中文名
        /// </summary>
        public string PointChineseName { get; set; }

        /// <summary>
        /// 管理据点英文名
        /// </summary>
        public string PointEnglishName { get; set; }

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