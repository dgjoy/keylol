namespace Keylol.Models
{
    public class PointMedia
    {
        public MediaType Type { get; set; }

        public string Link { get; set; }

        public enum MediaType
        {
            Video,
            Screenshot,
            Artwork
        }
    }
}