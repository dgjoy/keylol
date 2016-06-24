using Keylol.States.Content.Activity;
using Keylol.States.Content.Article;

namespace Keylol.States.Content
{
    /// <summary>
    /// 内容层级
    /// </summary>
    public class ContentLevel
    {
        /// <summary>
        /// 文章页
        /// </summary>
        public ArticlePage Article { get; set; }

        /// <summary>
        /// 动态页
        /// </summary>
        public ActivityPage Activity { get; set; }
    }
}