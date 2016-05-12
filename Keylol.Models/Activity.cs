using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models
{
    public class Activity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Index(IsUnique = true, IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Sid { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Index]
        public DateTime PublishTime { get; set; } = DateTime.Now;

        [Required]
        public string AuthorId { get; set; }

        public virtual KeylolUser Author { get; set; }

        [Index]
        public int SidForAuthor { get; set; }

        [Required]
        [MaxLength(30000)]
        public string Content { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(256)]
        public string CoverImage { get; set; } = string.Empty;

        public bool DismissLikeMessage { get; set; } = false;

        public bool DismissCommentMessage { get; set; } = false;

        public string TargetPointId { get; set; }

        public virtual Point TargetPoint { get; set; }

        /// <summary>
        /// 只有在收稿据点为游戏、硬件类型时填写
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 如果填写收稿据点，则必填此项（没有额外投稿时使用 "[]"），否则使用空值
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string AttachedPoints { get; set; } = string.Empty;

        public DeletedState Deleted { get; set; } = DeletedState.None;

        public ArchivedState Archived { get; set; } = ArchivedState.None;

        public bool Rejected { get; set; } = false;

        public bool Warned { get; set; } = false;
    }
}
