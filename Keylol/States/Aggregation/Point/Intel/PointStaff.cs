using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;

namespace Keylol.States.Aggregation.Point.Intel
{
    /// <summary>
    /// 据点职员列表
    /// </summary>
    public class PointStaffList : List<PointStaff>
    {
        private PointStaffList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 创建 <see cref="PointStaffList"/>
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="PointStaffList"/></returns>
        public static async Task<PointStaffList> CreateAsync(string pointId, string currentUserId, KeylolDbContext dbContext,
            CachedDataProvider cachedData)
        {
            var queryResult = await (from staff in dbContext.PointStaff
                where staff.PointId == pointId
                select new
                {
                    staff.Staff.Id,
                    staff.Staff.HeaderImage,
                    staff.Staff.IdCode,
                    staff.Staff.AvatarImage,
                    staff.Staff.UserName
                }).ToListAsync();
            var result = new PointStaffList(queryResult.Count);
            foreach (var u in queryResult)
            {
                result.Add(new PointStaff
                {
                    Id = u.Id,
                    HeaderImage = u.HeaderImage,
                    IdCode = u.IdCode,
                    AvatarImage = u.AvatarImage,
                    UserName = u.UserName,
                    IsFriend = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Users.IsFriendAsync(currentUserId, u.Id),
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, u.Id,
                            SubscriptionTargetType.User)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 据点职员
    /// </summary>
    public class PointStaff
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 头部图
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 是否是好友
        /// </summary>
        public bool? IsFriend { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }
    }
}