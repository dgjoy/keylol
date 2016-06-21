using System;
using System.Collections.Generic;
using System.Linq;
using Keylol.Models;

namespace Keylol.States.PostOffice
{
    /// <summary>
    /// 邮政中心消息列表
    /// </summary>
    public class PostOfficeMessageList : List<PostOfficeMessage>
    {
        private PostOfficeMessageList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="PostOfficeMessageList"/>
        /// </summary>
        /// <param name="messages">邮政消息对象列表</param>
        /// <returns><see cref="PostOfficeMessageList"/></returns>
        public static PostOfficeMessageList Create(List<Message> messages)
        {
            var result = new PostOfficeMessageList(messages.Count);
            foreach (var m in messages)
            {
                var item = new PostOfficeMessage
                {
                    Type = m.Type,
                    CreateTime = m.CreateTime,
                    Unread = m.Unread
                };

                if (!m.Type.IsMissiveMessage())
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
                    item.CommentContent = CollapseCommentContent(m.ArticleComment.Content, true);
                    item.CommentSidForParent = m.ArticleComment.SidForArticle;
                    item.ArticleAuthorIdCode = m.ArticleComment.Article.Author.IdCode;
                    item.ArticleSidForAuthor = m.ArticleComment.Article.SidForAuthor;
                    item.ArticleTitle = CollapleArticleTitle(m.Article.Title);
                }
                else if (m.ActivityCommentId != null)
                {
                    item.CommentContent = CollapseCommentContent(m.ActivityComment.Content, false);
                    item.CommentSidForParent = m.ActivityComment.SidForActivity;
                    item.ArticleAuthorIdCode = m.ActivityComment.Activity.Author.IdCode;
                    item.ArticleSidForAuthor = m.ActivityComment.Activity.SidForAuthor;
                    item.ActivityContent = CollapseActivityContent(m.ActivityComment.Activity);
                }

                if (m.Count > 0) item.Count = m.Count;
                if (m.SecondCount > 0) item.SecondCount = m.Count;

                result.Add(item);
            }
            return result;
        }

        private static string CollapseActivityContent(Activity activity)
        {
            var content = string.IsNullOrWhiteSpace(activity.CoverImage) ? activity.Content : $"{activity.Content}〔附图〕";
            return activity.Content.Length > 60 ? activity.Content.Substring(0, 60) : content;
        }

        private static string CollapseCommentContent(string content, bool isHtml)
        {
            if (isHtml)
            {
                // TODO: HTML 摘要提取
            }
            return content.Length > 60 ? content.Substring(0, 200) : content;
        }

        private static string CollapleArticleTitle(string title)
        {
            return title.Length > 60 ? title.Substring(0, 60) : title;
        }
    }

    /// <summary>
    /// 邮政中心消息
    /// </summary>
    public class PostOfficeMessage
    {
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