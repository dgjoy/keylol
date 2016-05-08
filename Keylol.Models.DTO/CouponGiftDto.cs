using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     CouponGift DTO
    /// </summary>
    [DataContract]
    public class CouponGiftDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        [DataMember]
        public List<string> Descriptions { get; set; }

        /// <summary>
        ///     缩略图
        /// </summary>
        [DataMember]
        public string ThumbnailImage { get; set; }

        /// <summary>
        ///     预览图片
        /// </summary>
        [DataMember]
        public string PreviewImage { get; set; }

        /// <summary>
        ///     接受的用户输入字段
        /// </summary>
        [DataMember]
        public List<CouponGiftAcceptedFieldDto> AcceptedFields { get; set; }

        /// <summary>
        ///     价格
        /// </summary>
        [DataMember]
        public int? Price { get; set; }

        /// <summary>
        ///     上架日期
        /// </summary>
        [DataMember]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        ///     下架日期
        /// </summary>
        [DataMember]
        public DateTime? EndTime { get; set; }

        /// <summary>
        ///     被兑换的总次数
        /// </summary>
        [DataMember]
        public int? RedeemCount { get; set; }

        /// <summary>
        ///     是否已被当前用户兑换过
        /// </summary>
        [DataMember]
        public bool? Redeemed { get; set; }

        /// <summary>
        ///     用户此前兑换输入过的字段
        /// </summary>
        [DataMember]
        public dynamic Extra { get; set; }
    }

    /// <summary>
    ///     用于 <see cref="CouponGiftDto" /> AcceptedFields
    /// </summary>
    [DataContract]
    public class CouponGiftAcceptedFieldDto
    {
        /// <summary>
        ///     字段 ID
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     字段名称
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        ///     字段描述
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        ///     用户输入类型，用于 input 标签 type 属性
        /// </summary>
        [DataMember]
        public string InputType { get; set; }
    }
}