namespace Keylol.Models.DTO
{
    public class SubscribedPointDTO
    {
        public NormalPointDTO NormalPoint { get; set; }
        public UserDTO User { get; set; }
        public int? ArticleCount { get; set; }
        public int? SubscriberCount { get; set; }
    }
}