using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.People
{
    /// <summary>
    /// 人脉用户列表
    /// </summary>
    public class UserEntryList : List<UserEntry>
    {
        private const int RecordsPerPage = 10;

        private UserEntryList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="UserEntryList"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="relationship">要获取的关系</param>
        /// <param name="returnCount">是否返回总数和总页数</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>Item1 表示 <see cref="UserEntryList"/>，Item2 表示总页数</returns>
        public static async Task<Tuple<UserEntryList, int>> CreateAsync(string userId, string currentUserId,
            UserRelationship relationship, bool returnCount, int page, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            switch (relationship)
            {
                case UserRelationship.Friend:
                {
                    var friendIds = await cachedData.Subscriptions.GetFriendsAsync(userId);
                    var friendCount = friendIds.Count;
                    friendIds.Reverse();
                    var skip = RecordsPerPage*(page - 1);
                    friendIds = friendIds.Skip(skip).Take(RecordsPerPage).ToList();
                    var queryResult = (from id in friendIds
                        join user in await (from user in dbContext.Users
                            where friendIds.Contains(user.Id)
                            select new
                            {
                                user.Id,
                                user.IdCode,
                                user.AvatarImage,
                                user.UserName,
                                user.GamerTag,
                                ArticleCount = dbContext.Articles.Count(a => a.AuthorId == user.Id),
                                ActivityCount = dbContext.Activities.Count(a => a.AuthorId == user.Id)
                            }).ToListAsync() on id equals user.Id
                        select user).ToList();

                    var result = new UserEntryList(queryResult.Count);
                    foreach (var u in queryResult)
                    {
                        result.Add(new UserEntry
                        {
                            Id = u.Id,
                            IdCode = u.IdCode,
                            AvatarImage = u.AvatarImage,
                            UserName = u.UserName,
                            GamerTag = u.GamerTag,
                            ArticleCount = u.ArticleCount,
                            ActivityCount = u.ActivityCount,
                            LikeCount = await cachedData.Likes.GetUserLikeCountAsync(u.Id)
                        });
                    }
                    return new Tuple<UserEntryList, int>(result,
                        friendCount > 0 ? (int) Math.Ceiling(friendCount/(double) RecordsPerPage) : 1);
                }

                case UserRelationship.SubscribedUser:
                {
                    var queryResult = await (from subscription in dbContext.Subscriptions
                        where subscription.SubscriberId == userId &&
                              subscription.TargetType == SubscriptionTargetType.User
                        join user in dbContext.Users on subscription.TargetId equals user.Id
                        orderby subscription.Sid descending
                        select new
                        {
                            user.Id,
                            user.IdCode,
                            user.AvatarImage,
                            user.UserName,
                            user.GamerTag,
                            ArticleCount = dbContext.Articles.Count(a => a.AuthorId == user.Id),
                            ActivityCount = dbContext.Activities.Count(a => a.AuthorId == user.Id)
                        }).TakePage(page, RecordsPerPage).ToListAsync();

                    var result = new UserEntryList(queryResult.Count);
                    foreach (var u in queryResult)
                    {
                        result.Add(new UserEntry
                        {
                            Id = u.Id,
                            IdCode = u.IdCode,
                            AvatarImage = u.AvatarImage,
                            UserName = u.UserName,
                            GamerTag = u.GamerTag,
                            ArticleCount = u.ArticleCount,
                            ActivityCount = u.ActivityCount,
                            LikeCount = await cachedData.Likes.GetUserLikeCountAsync(u.Id)
                        });
                    }
                    var count = await cachedData.Subscriptions.GetSubscribedUserCountAsync(userId);
                    return new Tuple<UserEntryList, int>(result,
                        count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1);
                }

                case UserRelationship.Subscriber:
                {
                    var queryResult = await (from subscription in dbContext.Subscriptions
                        where subscription.TargetId == userId &&
                              subscription.TargetType == SubscriptionTargetType.User
                        orderby subscription.Sid descending
                        select new
                        {
                            subscription.Subscriber.Id,
                            subscription.Subscriber.IdCode,
                            subscription.Subscriber.AvatarImage,
                            subscription.Subscriber.UserName,
                            subscription.Subscriber.GamerTag,
                            ArticleCount = dbContext.Articles.Count(a => a.AuthorId == subscription.SubscriberId),
                            ActivityCount = dbContext.Activities.Count(a => a.AuthorId == subscription.SubscriberId)
                        }).TakePage(page, RecordsPerPage).ToListAsync();

                    var result = new UserEntryList(queryResult.Count);
                    foreach (var u in queryResult)
                    {
                        result.Add(new UserEntry
                        {
                            Id = u.Id,
                            IdCode = u.IdCode,
                            AvatarImage = u.AvatarImage,
                            UserName = u.UserName,
                            GamerTag = u.GamerTag,
                            ArticleCount = u.ArticleCount,
                            ActivityCount = u.ActivityCount,
                            LikeCount = await cachedData.Likes.GetUserLikeCountAsync(u.Id)
                        });
                    }
                    var count =
                        await cachedData.Subscriptions.GetSubscriberCountAsync(userId, SubscriptionTargetType.User);
                    return new Tuple<UserEntryList, int>(result,
                        count > 0 ? (int) Math.Ceiling(count/(double) RecordsPerPage) : 1);
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null);
            }
        }
    }

    /// <summary>
    /// 用户关系
    /// </summary>
    public enum UserRelationship
    {
        /// <summary>
        /// 好友
        /// </summary>
        Friend,

        /// <summary>
        /// 关注
        /// </summary>
        SubscribedUser,

        /// <summary>
        /// 听众
        /// </summary>
        Subscriber
    }

    /// <summary>
    /// 人脉用户
    /// </summary>
    public class UserEntry
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 玩家标签
        /// </summary>
        public string GamerTag { get; set; }

        /// <summary>
        /// 文章总数
        /// </summary>
        public int? ArticleCount { get; set; }

        /// <summary>
        /// 动态总数
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// 获得认可总数
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否为好友
        /// </summary>
        public bool? IsFriend { get; set; }
    }
}