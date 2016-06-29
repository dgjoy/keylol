
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
        public PointSearch Point { get; set; }
        
        /// <summary>
        /// 文章查询结果
        /// </summary>
        public ArticleSearch Article { get; set; }
        
        /// <summary>
        /// 用户查询结果
        /// </summary>
        public UserSearch User { get; set; }


    }
}