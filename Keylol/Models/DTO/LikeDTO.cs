using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class LikeDTO
    {
        public LikeDTO(Like like)
        {
            Id = like.Id;
            Time = like.Time;
            ReadByTargetUser = like.ReadByTargetUser;
        }

        public string Id { get; set; }
        
        public DateTime? Time { get; set; }

        public bool? ReadByTargetUser { get; set; }

        public UserDTO Operator { get; set; }

        public ArticleDTO Article { get; set; }

        public CommentDTO Comment { get; set; }

        public bool? IgnoreNew { get; set; }
    }
}
