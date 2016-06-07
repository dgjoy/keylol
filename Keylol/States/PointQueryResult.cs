using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States
{
    /// <summary>
    /// 据点查询结果列表
    /// </summary>
    public class PointQueryResultList : List<PointQueryResult>
    {
        private PointQueryResultList()
        {
        }

        private PointQueryResultList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字查询据点列表
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="headerImage">返回结果是否包含页眉图片</param>
        /// <param name="playedTime">返回结果是否包含在档时间</param>
        /// <param name="typeWhitelist">据点类型白名单，逗号分隔</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="PointQueryResultList"/></returns>
        public static async Task<PointQueryResultList> Get(string keyword, [Injected] KeylolDbContext dbContext,
            bool headerImage = false, bool playedTime = false, string typeWhitelist = null)
        {
            List<PointType> types = null;
            if (!string.IsNullOrWhiteSpace(typeWhitelist))
                types = typeWhitelist.Split(',')
                    .Select(s => s.Trim()
                        .ToCase(NameConventionCase.CamelCase, NameConventionCase.PascalCase)
                        .ToEnum<PointType>())
                    .ToList();
            var currentUserId = StateTreeHelper.GetCurrentUserId();
            return await CreateAsync(currentUserId, keyword, headerImage, playedTime, types, dbContext);
        }

        /// <summary>
        /// 创建 <see cref="PointQueryResultList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="headerImage">返回结果是否包含页眉图片</param>
        /// <param name="playedTime">返回结果是否包含在档时间</param>
        /// <param name="typeWhitelist">据点类型白名单</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="PointQueryResultList"/></returns>
        public static async Task<PointQueryResultList> CreateAsync(string currentUserId, string keyword,
            bool headerImage, bool playedTime, List<PointType> typeWhitelist, KeylolDbContext dbContext)
        {
            var typeFilterSql = string.Empty;
            if (typeWhitelist != null && typeWhitelist.Count > 0)
            {
                var inTypes = string.Join(", ", typeWhitelist.Select(t => (int) t));
                typeFilterSql = $"WHERE [t1].[Type] IN ({inTypes})";
            }
            try
            {
                var queryResult = await dbContext.Points.SqlQuery(
                    @"SELECT TOP(4) * FROM [dbo].[Points] AS [t1] INNER JOIN (
                        SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
		                    SELECT * FROM CONTAINSTABLE([dbo].[Points], ([EnglishName], [EnglishAliases]), {0})
		                    UNION ALL
		                    SELECT * FROM CONTAINSTABLE([dbo].[Points], ([ChineseName], [ChineseAliases]), {0})
	                    ) AS [t2] GROUP BY [t2].[KEY]
                    ) AS [t3] ON [t1].[Id] = [t3].[KEY] " + typeFilterSql + @"
                    ORDER BY [t3].[RANK] DESC",
                    $"\"{keyword}\" OR \"{keyword}*\"")
                    .ToListAsync();

                var result = new PointQueryResultList(queryResult.Count);
                foreach (var p in queryResult)
                {
                    result.Add(new PointQueryResult
                    {
                        Id = p.Id,
                        AvatarImage = p.AvatarImage,
                        ChineseName = p.ChineseName,
                        EnglishName = p.EnglishName,
                        HeaderImage = headerImage ? p.HeaderImage : null,
                        TotalPlayedTime = playedTime && p.SteamAppId.HasValue
                            ? (await dbContext.UserSteamGameRecords
                                .Where(r => r.UserId == currentUserId && r.SteamAppId == p.SteamAppId.Value)
                                .SingleOrDefaultAsync())?.TotalPlayedTime
                            : null
                    });
                }
                return result;
            }
            catch (SqlException)
            {
                return new PointQueryResultList();
            }
        }
    }

    /// <summary>
    /// 据点查询结果
    /// </summary>
    public class PointQueryResult
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

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

        /// <summary>
        /// 页眉图片
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 在档时间
        /// </summary>
        public double? TotalPlayedTime { get; set; }
    }
}