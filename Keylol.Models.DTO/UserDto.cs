using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     KeylolUser DTO
    /// </summary>
    [DataContract]
    public class UserDto
    {
        /// <summary>
        ///     创建空 DTO，需要手动填充
        /// </summary>
        public UserDto()
        {
        }

        /// <summary>
        ///     创建 DTO 并自动填充部分数据
        /// </summary>
        public UserDto(KeylolUser user)
        {
            Id = user.Id;
            IdCode = user.IdCode;
            UserName = user.UserName;
            GamerTag = user.GamerTag;
            AvatarImage = user.AvatarImage;
        }

        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     识别码
        /// </summary>
        [DataMember]
        public string IdCode { get; set; }

        /// <summary>
        ///     用户名
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        ///     玩家标签
        /// </summary>
        [DataMember]
        public string GamerTag { get; set; }

        /// <summary>
        ///     邮箱
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        ///     头像
        /// </summary>
        [DataMember]
        public string AvatarImage { get; set; }

        /// <summary>
        ///     个人据点背景横幅
        /// </summary>
        [DataMember]
        public string ProfilePointBackgroundImage { get; set; }

        /// <summary>
        ///     登录保护开关
        /// </summary>
        [DataMember]
        public bool? LockoutEnabled { get; set; }

        /// <summary>
        ///     Steam ID 3
        /// </summary>
        [DataMember]
        public string SteamId { get; set; }

        /// <summary>
        ///     Steam 玩家昵称
        /// </summary>
        [DataMember]
        public string SteamProfileName { get; set; }

        /// <summary>
        ///     身份
        /// </summary>
        [DataMember]
        public List<string> Roles { get; set; }

        /// <summary>
        ///     相关 Steam 机器人
        /// </summary>
        [DataMember]
        public SteamBotDto SteamBot { get; set; }

        /// <summary>
        ///     订阅的据点数量
        /// </summary>
        [DataMember]
        public int? SubscribedPointCount { get; set; }

        /// <summary>
        ///     读者数量
        /// </summary>
        [DataMember]
        public int? SubscriberCount { get; set; }

        /// <summary>
        ///     文章数量
        /// </summary>
        [DataMember]
        public int? ArticleCount { get; set; }

        /// <summary>
        ///     是否已被当前用户订阅
        /// </summary>
        [DataMember]
        public bool? Subscribed { get; set; }

        /// <summary>
        ///     未读消息数量，逗号分隔，从左至又分别表示认可、评论、公函
        /// </summary>
        [DataMember]
        public string MessageCount { get; set; }

        /// <summary>
        ///     评测数量（不包含简评）
        /// </summary>
        [DataMember]
        public int? ReviewCount { get; set; }

        /// <summary>
        ///     简评数量
        /// </summary>
        [DataMember]
        public int? ShortReviewCount { get; set; }

        /// <summary>
        ///     文券数量
        /// </summary>
        [DataMember]
        public int? Coupon { get; set; }

        /// <summary>
        ///     收到的总认可数量
        /// </summary>
        [DataMember]
        public int? LikeCount { get; set; }

        /// <summary>
        ///     Steam 通知开关：文章收到评论
        /// </summary>
        [DataMember]
        public bool? SteamNotifyOnArticleReplied { get; set; }

        /// <summary>
        ///     Steam 通知开关：评论收到回复
        /// </summary>
        [DataMember]
        public bool? SteamNotifyOnCommentReplied { get; set; }

        /// <summary>
        ///     Steam 通知开关：文章被认可
        /// </summary>
        [DataMember]
        public bool? SteamNotifyOnArticleLiked { get; set; }

        /// <summary>
        ///     Steam 通知开关：评论被认可
        /// </summary>
        [DataMember]
        public bool? SteamNotifyOnCommentLiked { get; set; }

        /// <summary>
        ///     同步订阅开关
        /// </summary>
        [DataMember]
        public bool? AutoSubscribeEnabled { get; set; }

        /// <summary>
        ///     同步订阅周期
        /// </summary>
        [DataMember]
        public double? AutoSubscribeDaySpan { get; set; }
    }

    /// <summary>
    ///     用户身份类型
    /// </summary>
    [DataContract]
    public enum UserIdentityType
    {
        /// <summary>
        ///     Id
        /// </summary>
        [EnumMember] Id,

        /// <summary>
        ///     识别码
        /// </summary>
        [EnumMember] IdCode,

        /// <summary>
        ///     用户名
        /// </summary>
        [EnumMember] UserName,

        /// <summary>
        ///     Steam ID
        /// </summary>
        [EnumMember] SteamId
    }
}