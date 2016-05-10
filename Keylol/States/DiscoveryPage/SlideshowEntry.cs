using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// Slideshow Entry
    /// </summary>
    public class SlideshowEntry
    {
        /// <summary>
        /// 主标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 概要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 次要主标题
        /// </summary>
        public string MinorTitle { get; set; }

        /// <summary>
        /// 次要副标题
        /// </summary>
        public string MinorSubtitle { get; set; }

        /// <summary>
        /// 目标链接
        /// </summary>
        public string Link { get; set; }
    }
}