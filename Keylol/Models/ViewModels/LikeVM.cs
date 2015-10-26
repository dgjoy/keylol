using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
