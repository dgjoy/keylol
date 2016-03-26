using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Message
{
    public partial class MessageController
    {
        /// <summary>
        ///     邮政消息过滤类型
        /// </summary>
        public enum MessageFilter
        {
            /// <summary>
            ///     认可
            /// </summary>
            Like,

            /// <summary>
            ///     评论
            /// </summary>
            Comment,

            /// <summary>
            ///     公函
            /// </summary>
            Missive
        }

        /// <summary>
        ///     获取当前登录用户的邮政消息
        /// </summary>
        /// <param name="filter">消息类型过滤条件</param>
        /// <param name="beforeSn">获取编号小于这个数字的消息，用于分块加载，默认 2147483647</param>
        /// <param name="take">获取数量，最大 50，默认 30</param>
        [Route]
        [HttpGet]
        [ResponseType(typeof (MessageGetListByCurrentUserResponseDto))]
        public async Task<IHttpActionResult> GetListByCurrentUser(MessageFilter? filter = null,
            int beforeSn = int.MaxValue, int take = 30)
        {
            if (take > 50) take = 50;
            var userId = User.Identity.GetUserId();
            IQueryable<Models.Message> query;
            switch (filter)
            {
                case MessageFilter.Like:
                    query = DbContext.Messages.Where(m => m.ReceiverId == userId &&
                                                          m.Type >= 0 && (int) m.Type <= 99 &&
                                                          m.SequenceNumber < beforeSn);
                    break;

                case MessageFilter.Comment:
                    query = DbContext.Messages.Where(m => m.ReceiverId == userId &&
                                                          (int) m.Type >= 100 && (int) m.Type <= 199 &&
                                                          m.SequenceNumber < beforeSn);
                    break;

                case MessageFilter.Missive:
                    query = DbContext.Messages.Where(m => m.ReceiverId == userId &&
                                                          (int) m.Type >= 100 && (int) m.Type <= 199 &&
                                                          m.SequenceNumber < beforeSn);
                    break;

                case null:
                    query = DbContext.Messages.Where(m => m.ReceiverId == userId && m.SequenceNumber < beforeSn);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
            var result = await query.Include(m => m.Article)
                .Include(m => m.Operator)
                .Include(m => m.Comment)
                .Include(m => m.Comment.Article)
                .OrderByDescending(m => m.Unread)
                .ThenByDescending(m => m.SequenceNumber)
                .Take(() => take)
                .ToListAsync();
            var response = result.Select(m =>
            {
                var dto = new MessageGetListByCurrentUserResponseDto
                {
                    Id = m.Id,
                    Type = m.Type,
                    SequenceNumber = m.SequenceNumber,
                    CreateTime = m.CreateTime,
                    Unread = m.Unread
                };
                if (!m.Type.IsMissiveMessage())
                    dto.Operator = new UserDTO(m.Operator);

                if (m.Type.HasReasonProperty())
                    dto.Reasons =
                        m.Reasons.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                if (m.Type.HasArticleProperty())
                    dto.Article = new ArticleDTO(m.Article, true, 128);

                if (m.Type.HasCommentProperty())
                {
                    dto.Comment = new CommentDTO(m.Comment, true, 128);
                    dto.Article = new ArticleDTO(m.Comment.Article);
                }
                return dto;
            });
            foreach (var message in result)
            {
                message.Unread = false;
            }
            await DbContext.SaveChangesAsync();
            return Ok(response);
        }

        /// <summary>
        ///     响应 DTO
        /// </summary>
        public class MessageGetListByCurrentUserResponseDto
        {
            /// <summary>
            ///     Id
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     类型
            /// </summary>
            public MessageType Type { get; set; }

            /// <summary>
            ///     序号
            /// </summary>
            public int SequenceNumber { get; set; }

            /// <summary>
            ///     发送时间
            /// </summary>
            public DateTime CreateTime { get; set; }

            /// <summary>
            ///     发送人
            /// </summary>
            public UserDTO Operator { get; set; }

            /// <summary>
            ///     相关文章
            /// </summary>
            public ArticleDTO Article { get; set; }

            /// <summary>
            ///     相关评论
            /// </summary>
            public CommentDTO Comment { get; set; }

            /// <summary>
            ///     原因
            /// </summary>
            public List<int> Reasons { get; set; }

            /// <summary>
            ///     是否未读
            /// </summary>
            public bool Unread { get; set; }
        }
    }
}