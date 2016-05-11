using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keylol.Models
{
    public class Article
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
        [MaxLength(120)]
        public string Title { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(120)]
        public string Subtitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(300000)]
        public string Content { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1024)]
        public string ThumbnailImage { get; set; } = string.Empty;

        public bool DismissLikeMessage { get; set; } = false;

        public bool DismissCommentMessage { get; set; } = false;

        public string TargetPointId { get; set; }

        public virtual Point TargetPoint { get; set; }

        /// <summary>
        /// 只有在收稿据点为游戏、硬件类型时填写
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 额外投稿据点，如果填写收稿据点，则必填此项（不应该包含收稿据点，没有额外投稿时使用 "[]"），否则使用空值
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string AttachedPoints { get; set; } = string.Empty;

        /// <summary>
        /// 收稿据点为游戏、硬件类型时，必填此项（如无则使用 "[]"），否则使用空值
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        [MaxLength(5000)]
        public string Pros { get; set; } = string.Empty;

        /// <summary>
        /// 收稿据点为游戏、硬件类型时，必填此项（如无则使用 "[]"），否则使用空值
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        [MaxLength(5000)]
        public string Cons { get; set; } = string.Empty;
        
        public DeletedState Deleted { get; set; } = DeletedState.None;

        public ArchivedState Archived { get; set; } = ArchivedState.None;
        
        public bool Rejected { get; set; } = false;

        public bool Spotlighted { get; set; } = false;

        public bool Warned { get; set; } = false;
    }

    public enum DeletedState
    {
        /// <summary>
        ///     没有删除
        /// </summary>
        None,

        /// <summary>
        ///     用户自行删除
        /// </summary>
        User,

        /// <summary>
        ///     运维职员删除
        /// </summary>
        Operator
    }

    public enum ArchivedState
    {
        /// <summary>
        ///     没有封存
        /// </summary>
        None,

        /// <summary>
        ///     用户自行封存
        /// </summary>
        User,

        /// <summary>
        ///     运维职员封存
        /// </summary>
        Operator
    }
}