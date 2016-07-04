using Keylol.States.Search.Article;
using Keylol.States.Search.Point;
using Keylol.States.Search.User;

namespace Keylol.States.Search
{
    /// <summary>
    /// 搜索层级
    /// </summary>
    public class SearchLevel
    {
        /// <summary>
        /// 据点
        /// </summary>
        public PointPage Point { get; set; }
        
        /// <summary>
        /// 文章
        /// </summary>
        public ArticlePage Article { get; set; }
        
        /// <summary>
        /// 用户
        /// </summary>
        public UserPage User { get; set; }
    }
}