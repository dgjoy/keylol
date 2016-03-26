using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class LikeVM
    {
        public enum LikeType
        {
            ArticleLike,
            CommentLike
        }

        [Required]
        public string TargetId { get; set; }

        public LikeType Type { get; set; }
    }
}