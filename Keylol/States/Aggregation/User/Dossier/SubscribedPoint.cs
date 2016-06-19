using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Aggregation.User.Dossier
{
    /// <summary>
    /// 订阅的据点列表
    /// </summary>
    public class SubscribedPointList : List<SubscribedPoint>
    {
        private SubscribedPointList([NotNull] IEnumerable<SubscribedPoint> collection) : base(collection)
        {
        }

        /// <summary>
        /// 获取指定用户订阅的据点列表
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SubscribedPointList"/></returns>
        public static async Task<SubscribedPointList> Get(string userId, int page, [Injected] KeylolDbContext dbContext)
        {
            return (await CreateAsync(userId, page, 30, false, dbContext)).Item1;
        }

        /// <summary>
        /// 创建 <see cref="SubscribedPointList"/>
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="returnCount">是否返回总数</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns>Item1 表示 <see cref="SubscribedPointList"/>，Item2 表示总数</returns>
        public static async Task<Tuple<SubscribedPointList, int>> CreateAsync(string userId, int page,
            int recordsPerPage, bool returnCount, KeylolDbContext dbContext)
        {
            var conditionQuery = from subscription in dbContext.Subscriptions
                where subscription.SubscriberId == userId && subscription.TargetType == SubscriptionTargetType.Point
                select subscription;
            var queryResult = await (from subscription in conditionQuery
                join point in dbContext.Points on subscription.TargetId equals point.Id
                orderby subscription.Sid descending
                select new
                {
                    Count = returnCount ? conditionQuery.Count() : 1,
                    point.IdCode,
                    point.AvatarImage,
                    point.ChineseName,
                    point.EnglishName
                }).TakePage(page, recordsPerPage).ToListAsync();

            var result = new SubscribedPointList(queryResult.Select(p => new SubscribedPoint
            {
                IdCode = p.IdCode,
                AvatarImage = p.AvatarImage,
                ChineseName = p.ChineseName,
                EnglishName = p.EnglishName
            }));
            var firstRecord = queryResult.FirstOrDefault();
            return new Tuple<SubscribedPointList, int>(result, firstRecord?.Count ?? 0);
        }
    }

    /// <summary>
    /// 订阅的据点
    /// </summary>
    public class SubscribedPoint
    {
        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }
    }
}