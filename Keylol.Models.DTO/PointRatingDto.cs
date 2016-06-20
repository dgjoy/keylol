using System.Runtime.Serialization;

namespace Keylol.Models.DTO
{
    /// <summary>
    /// Point Rating DTO
    /// </summary>
    [DataContract]
    public class PointRatingsDto
    {
        /// <summary>
        /// 一星评分个数
        /// </summary>
        [DataMember]
        public int OneStarCount { get; set; }

        /// <summary>
        /// 二星评分个数
        /// </summary>
        [DataMember]
        public int TwoStarCount { get; set; }

        /// <summary>
        /// 三星评分个数
        /// </summary>
        [DataMember]
        public int ThreeStarCount { get; set; }

        /// <summary>
        /// 四星评分个数
        /// </summary>
        [DataMember]
        public int FourStarCount { get; set; }

        /// <summary>
        /// 五星评分个数
        /// </summary>
        [DataMember]
        public int FiveStarCount { get; set; }

        /// <summary>
        /// 平均评分，如果评价不足，使用 null
        /// </summary>
        [DataMember]
        public double? AverageRating { get; set; }
    }
}