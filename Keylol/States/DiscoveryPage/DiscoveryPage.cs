using System.Collections.Generic;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// 发现页
    /// </summary>
    public class DiscoveryPage
    {
        /// <summary>
        /// Slideshow Entries
        /// </summary>
        public List<SlideshowEntry> SlideshowEntries { get; set; }

        /// <summary>
        /// Spotlight Points
        /// </summary>
        public List<SpotlightPoint> SpotlightPoints { get; set; }
    }
}