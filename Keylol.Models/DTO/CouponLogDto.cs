using System;
using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     CouponLog DTO
    /// </summary>
    [DataContract]
    public class CouponLogDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     变动事件
        /// </summary>
        [DataMember]
        public CouponEvent? Event { get; set; }

        /// <summary>
        ///     变动前余额
        /// </summary>
        [DataMember]
        public int? Before { get; set; }

        /// <summary>
        ///     变动数值
        /// </summary>
        [DataMember]
        public int? Change { get; set; }

        /// <summary>
        ///     变动后余额
        /// </summary>
        [DataMember]
        public int? Balance { get; set; }

        /// <summary>
        ///     发生时间
        /// </summary>
        [DataMember]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        ///     详细描述
        /// </summary>
        [DataMember]
        public dynamic Description { get; set; }
    }
}