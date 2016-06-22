using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.States.PostOffice.SocialActivity;
using Keylol.Utilities;

namespace Keylol.States.PostOffice
{
    /// <summary>
    /// 邮政中心消息列表
    /// </summary>
    public class PostOfficeMessageList : List<PostOfficeMessage>
    {
        private const int RecordsPerPage = 10;

        private PostOfficeMessageList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="PostOfficeMessageList"/>
        /// </summary>
        /// <param name="pageType">邮政页面类型</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="returnPageCount">是否返回总页数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns>Item1 表示 <see cref="PostOfficeMessageList"/>，Item2 表示总页数</returns>
        public static async Task<Tuple<PostOfficeMessageList, int>> CreateAsync(Type pageType, string currentUserId,
            int page, bool returnPageCount, KeylolDbContext dbContext)
        {
            Expression<Func<Message, bool>> condition;
            if (pageType == typeof(UnreadPage))
                condition = m => m.ReceiverId == currentUserId;
            else if (pageType == typeof(CommentPage))
                condition = m => m.ReceiverId == currentUserId && (int) m.Type >= 100 && (int) m.Type <= 199;
            else if (pageType == typeof(LikePage))
                condition = m => m.ReceiverId == currentUserId && m.Type >= 0 && (int) m.Type <= 99;
            else if (pageType == typeof(SubscriberPage))
                condition = m => m.ReceiverId == currentUserId && (int) m.Type >= 300 && (int) m.Type <= 399;
            else if (pageType == typeof(MissivePage))
                condition = m => m.ReceiverId == currentUserId && (int) m.Type >= 200 && (int) m.Type <= 299;
            else throw new ArgumentOutOfRangeException(nameof(pageType));

            var messages = await dbContext.Messages.IncludeRelated()
                .Where(condition)
                .OrderByDescending(m => m.Unread)
                .ThenByDescending(m => m.Sid)
                .TakePage(page, RecordsPerPage)
                .ToListAsync();
            var result = new PostOfficeMessageList(messages.Count);
            foreach (var m in messages)
            {
                var item = new PostOfficeMessage
                {
                    Type = m.Type,
                    CreateTime = m.CreateTime,
                    Unread = m.Unread
                };

                if (m.Type.IsMissiveMessage())
                {
                    item.Id = m.Id;
                }
                else
                {
                    item.OperatorIdCode = m.Operator.IdCode;
                    item.OperatorAvatarImage = m.Operator.AvatarImage;
                    item.OperatorUserName = m.Operator.UserName;
                }

                if (!string.IsNullOrWhiteSpace(m.Reasons))
                    item.Reasons =
                        m.Reasons.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                if (m.ArticleId != null)
                {
                    item.ArticleAuthorIdCode = m.Article.Author.IdCode;
                    item.ArticleSidForAuthor = m.Article.SidForAuthor;
                    item.ArticleTitle = CollapleArticleTitle(m.Article.Title);
                }
                else if (m.ActivityId != null)
                {
                    item.ActivityContent = CollapseActivityContent(m.Activity);
                }
                else if (m.ArticleCommentId != null)
                {
                    item.CommentContent = CollapseCommentContent(m.ArticleComment.UnstyledContent);
                    item.CommentSidForParent = m.ArticleComment.SidForArticle;
                    item.ArticleAuthorIdCode = m.ArticleComment.Article.Author.IdCode;
                    item.ArticleSidForAuthor = m.ArticleComment.Article.SidForAuthor;
                    item.ArticleTitle = CollapleArticleTitle(m.ArticleComment.Article.Title);
                }
                else if (m.ActivityCommentId != null)
                {
                    item.CommentContent = CollapseCommentContent(m.ActivityComment.Content);
                    item.CommentSidForParent = m.ActivityComment.SidForActivity;
                    item.ArticleAuthorIdCode = m.ActivityComment.Activity.Author.IdCode;
                    item.ArticleSidForAuthor = m.ActivityComment.Activity.SidForAuthor;
                    item.ActivityContent = CollapseActivityContent(m.ActivityComment.Activity);
                }

                if (m.Count > 0) item.Count = m.Count;
                if (m.SecondCount > 0) item.SecondCount = m.SecondCount;

                result.Add(item);
                m.Unread = false;
            }
            await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.ClientWin);
            var pageCount = 1;
            if (returnPageCount)
            {
                var totalCount = await dbContext.Messages.CountAsync(condition);
                pageCount = totalCount > 0 ? (int) Math.Ceiling(totalCount/(double) RecordsPerPage) : 1;
            }
            return new Tuple<PostOfficeMessageList, int>(result, pageCount);
        }

        /// <summary>
        /// 折叠动态内容
        /// </summary>
        /// <param name="activity">动态对象</param>
        /// <returns>折叠后的动态内容</returns>
        public static string CollapseActivityContent(Activity activity)
        {
            var content = string.IsNullOrWhiteSpace(activity.CoverImage) ? activity.Content : $"{activity.Content}〔附图〕";
            return activity.Content.Length > 50 ? $"{activity.Content.Substring(0, 50)} …" : content;
        }

        private static string CollapseCommentContent(string content)
        {
            return content.Length > 200 ? $"{content.Substring(0, 200)} …" : content;
        }

        private static string CollapleArticleTitle(string title)
        {
            return title.Length > 50 ? $"{title.Substring(0, 50)} …" : title;
        }
    }

    /// <summary>
    /// 邮政中心消息
    /// </summary>
    public class PostOfficeMessage
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 操作人识别码
        /// </summary>
        public string OperatorIdCode { get; set; }

        /// <summary>
        /// 操作人头像
        /// </summary>
        public string OperatorAvatarImage { get; set; }

        /// <summary>
        /// 操作人用户名
        /// </summary>
        public string OperatorUserName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 是否未读
        /// </summary>
        public bool? Unread { get; set; }

        /// <summary>
        /// 相关文章作者识别码
        /// </summary>
        public string ArticleAuthorIdCode { get; set; }

        /// <summary>
        /// 相关文章在作者名下序号
        /// </summary>
        public int? ArticleSidForAuthor { get; set; }

        /// <summary>
        /// 相关文章标题
        /// </summary>
        public string ArticleTitle { get; set; }

        /// <summary>
        /// 相关动态内容
        /// </summary>
        public string ActivityContent { get; set; }

        /// <summary>
        /// 相关评论内容
        /// </summary>
        public string CommentContent { get; set; }

        /// <summary>
        /// 相关评论楼层号
        /// </summary>
        public int? CommentSidForParent { get; set; }

        /// <summary>
        /// 相关计数
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// 另一个相关技术
        /// </summary>
        public int? SecondCount { get; set; }

        /// <summary>
        /// 公函理由
        /// </summary>
        public List<int> Reasons { get; set; }
    }
}