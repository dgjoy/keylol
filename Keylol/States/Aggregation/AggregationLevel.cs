using Keylol.States.Aggregation.Point;
using Keylol.States.Aggregation.User;

namespace Keylol.States.Aggregation
{
    /// <summary>
    /// 聚合层级
    /// </summary>
    public class AggregationLevel
    {
        /// <summary>
        /// 据点层级
        /// </summary>
        public PointLevel Point { get; set; }

        /// <summary>
        /// 用户层级
        /// </summary>
        public UserLevel User { get; set; }
    }
}