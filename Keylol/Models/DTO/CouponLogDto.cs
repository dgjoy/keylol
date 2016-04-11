using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Keylol.Models.DTO
{
    /// <summary>
    ///     CouponLog DTO
    /// </summary>
    public class CouponLogDto
    {
        /// <summary>
        ///     Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     变动事件
        /// </summary>
        public CouponEvent? Event { get; set; }

        /// <summary>
        ///     变动前余额
        /// </summary>
        public int? Before { get; set; }

        /// <summary>
        ///     变动数值
        /// </summary>
        public int? Change { get; set; }

        /// <summary>
        ///     变动后余额
        /// </summary>
        public int? Balance { get; set; }

        /// <summary>
        ///     发生时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        ///     详细描述
        /// </summary>
        public dynamic Description { get; set; }
    }
}