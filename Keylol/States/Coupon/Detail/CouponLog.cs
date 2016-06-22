using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.ServiceBase;
using Keylol.StateTreeManager;
using Keylol.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keylol.States.Coupon.Detail
{
    /// <summary>
    /// 文券记录列表
    /// </summary>
    public class CouponLogList : List<CouponLog>
    {
        private const int RecordsPerPage = 10;

        private CouponLogList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取文券记录列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns><see cref="CouponLogList"/></returns>
        public static async Task<CouponLogList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] KeylolUserManager userManager)
        {
            return (await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, false, dbContext, userManager)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="CouponLogList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="returnPageCount">是否返回总页数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <returns>Item1 表示 <see cref="CouponLogList"/>，Item2 表示总页数</returns>
        public static async Task<Tuple<CouponLogList, int>> CreateAsync(string currentUserId, int page,
            bool returnPageCount, KeylolDbContext dbContext, KeylolUserManager userManager)
        {
            var conditionQuery = from log in dbContext.CouponLogs
                where log.UserId == currentUserId
                orderby log.Sid descending
                select log;
            var queryResult = await conditionQuery.Select(l => new
            {
                Count = returnPageCount ? conditionQuery.Count() : 1,
                l.Event,
                l.Change,
                l.Balance,
                l.CreateTime,
                l.Description
            }).TakePage(page, RecordsPerPage).ToListAsync();

            var result = new CouponLogList(queryResult.Count);
            foreach (var l in queryResult)
            {
                result.Add(new CouponLog
                {
                    Event = l.Event,
                    Change = l.Change,
                    Balance = l.Balance,
                    CreateTime = l.CreateTime,
                    Description = await ParseDescriptionAsync(l.Description, dbContext, userManager)
                });
            }
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<CouponLogList, int>(
                result,
                (int) Math.Ceiling(firstRecord?.Count/(double) RecordsPerPage ?? 1));
        }

        private static async Task<object> ParseDescriptionAsync(string descriptionText, KeylolDbContext dbContext,
            KeylolUserManager userManager)
        {
            var description = Helpers.SafeDeserialize<JObject>(descriptionText);
            if (description == null) return null;
            Func<object, JObject> jObject =
                o => JObject.FromObject(o, new JsonSerializer {NullValueHandling = NullValueHandling.Ignore});
            Func<string, string, Task> fillUser = async (field, newField) =>
            {
                if (description[field] != null)
                {
                    var user = await userManager.FindByIdAsync((string) description[field]);
                    if (user != null)
                    {
                        description.Remove(field);
                        description[newField] = jObject(new
                        {
                            user.UserName,
                            user.IdCode
                        });
                    }
                }
            };
            Func<string, string> truncateContent =
                content => content.Length > 15 ? $"{content.Substring(0, 15)} …" : content;
            if (description["ArticleId"] != null)
            {
                var article = await dbContext.Articles.FindAsync((string) description["ArticleId"]);
                if (article != null)
                {
                    description.Remove("ArticleId");
                    description["Article"] = jObject(new
                    {
                        article.Title,
                        article.SidForAuthor,
                        article.Author.IdCode
                    });
                }
            }
            if (description["CommentId"] != null || description["ArticleCommentId"] != null)
            {
                var commentId = description["ArticleCommentId"] ?? description["CommentId"];
                var comment = await dbContext.ArticleComments.FindAsync((string) commentId);
                if (comment != null)
                {
                    description.Remove("ArticleCommentId");
                    description.Remove("CommentId");
                    description["ArticleComment"] = jObject(new
                    {
                        Content = truncateContent(comment.Content),
                        ArticleAuthorIdCode = comment.Article.Author.IdCode,
                        comment.Article.SidForAuthor,
                        comment.SidForArticle
                    });
                }
            }
            if (description["ActivityId"] != null)
            {
                var activity = await dbContext.Activities.FindAsync((string) description["ActivityId"]);
                if (activity != null)
                {
                    description.Remove("ActivityId");
                    description["Activity"] = jObject(new
                    {
                        Content = truncateContent(activity.Content),
                        activity.SidForAuthor,
                        activity.Author.IdCode
                    });
                }
            }
            if (description["ActivityCommentId"] != null)
            {
                var comment = await dbContext.ActivityComments.FindAsync((string) description["ActivityCommentId"]);
                if (comment != null)
                {
                    description.Remove("ActivityCommentId");
                    description["ActivityComment"] = jObject(new
                    {
                        Content = truncateContent(comment.Content),
                        ArticleAuthorIdCode = comment.Activity.Author.IdCode,
                        comment.Activity.SidForAuthor,
                        comment.SidForActivity
                    });
                }
            }
            if (description["CouponGiftId"] != null)
            {
                var gift = await dbContext.CouponGifts.FindAsync((string) description["CouponGiftId"]);
                if (gift != null)
                {
                    description.Remove("CouponGiftId");
                    description["CouponGift"] = jObject(new
                    {
                        gift.Id,
                        gift.Name
                    });
                }
            }
            await fillUser("OperatorId", "Operator");
            await fillUser("UserId", "User");
            await fillUser("InviterId", "Inviter");
            return description;
        }
    }

    /// <summary>
    /// 文券记录
    /// </summary>
    public class CouponLog
    {
        /// <summary>
        /// 事件
        /// </summary>
        public CouponEvent? Event { get; set; }

        /// <summary>
        /// 变化
        /// </summary>
        public int? Change { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public int? Balance { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public object Description { get; set; }
    }
}