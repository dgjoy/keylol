using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;

namespace Keylol.States.Search
{
    /// <summary>
    /// 据点搜索结果列表
    /// </summary>
    public class PointSearchList : List<PointSearch>
    {
        private PointSearchList()
        {
        }

        private PointSearchList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 通过关键字搜索据点列表
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="searchAll">是否全部查询</param>
        /// <returns><see cref="PointSearchList"/></returns>
        public static async Task<PointSearchList> Get(string keyword,[Injected] KeylolDbContext dbContext, bool searchAll = true)
        {
            //TODO 查询操作
            var searchResult = await dbContext.Points.SqlQuery("", $"\"{keyword}\" OR \"{keyword}*\"").ToListAsync();
            var result = new PointSearchList();
            return result;
        }
    }

    /// <summary>
    /// 据点搜索结果
    /// </summary>
    public class PointSearch
    {
        /// <summary>
        /// 据点中文名称
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 据点英文名称
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 读者数量
        /// </summary>
        public int? ReaderCount { get; set; }

        /// <summary>
        /// 来稿文章数量
        /// </summary>
        public int ArticleCount { get; set; }

        /// <summary>
        /// 动态数量
        /// </summary>
        public int ActivityCount { get; set; }

    }
}
