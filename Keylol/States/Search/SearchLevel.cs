
namespace Keylol.States.Search
{
    /// <summary>
    /// 搜索层级
    /// </summary>
    public class SearchLevel
    {
        /// <summary>
        /// 据点查询结果
        /// </summary>
        public PointPage Point { get; set; }
        
        /// <summary>
        /// 文章查询结果
        /// </summary>
        public ArticlePage Article { get; set; }
        
        /// <summary>
        /// 用户查询结果
        /// </summary>
        public UserPage User { get; set; }


    }
}