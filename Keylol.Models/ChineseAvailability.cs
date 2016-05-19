using System.Collections.Generic;

namespace Keylol.Models
{
    public class ChineseAvailability
    {
        public Language English { get; set; }

        public Language Japanese { get; set; }

        public Language SimplifiedChinese { get; set; }

        public Language TraditionalChinese { get; set; }

        public List<ThirdPartyLink> ThirdPartyLinks { get; set; }

        public class Language
        {
            public bool Interface { get; set; }

            public bool Subtitles { get; set; }

            public bool FullAudio { get; set; }
        }

        public class ThirdPartyLink
        {
            public string Title { get; set; }

            public string Link { get; set; }
        }
    }
}