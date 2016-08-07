using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Keylol.States.Aggregation.User.Dossier.Article;
using Keylol.States.Aggregation.User.Dossier.Default;
using Keylol.States.Aggregation.User.Dossier.Point;

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 档案
    /// </summary>
    public class DossierLevel
    {
        /// <summary>
        /// 文章页
        /// </summary>
        public ArticlePage Article { get; set; }

        /// <summary>
        /// 默认页
        /// </summary>
        public DefaultPage Default { get; set; }

        /// <summary>
        /// 据点页
        /// </summary>
        public PointPage Point { get; set; }
    }
}