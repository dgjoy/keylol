using System;
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
        /// 总实际评分个数（每个用户只算一个实际评分，取其打出的所有评分的平均值）
        /// </summary>
        [DataMember]
        public int TotalCount { get; set; }

        /// <summary>
        /// 总实际得分
        /// </summary>
        [DataMember]
        public int TotalScore { get; set; }

        /// <summary>
        /// 平均评分，如果评价不足，返回 <c>null</c>
        /// </summary>
        public double? AverageRating => TotalCount < 5 ? (double?) null : Math.Round(TotalScore/(double) TotalCount, 1);
    }
}