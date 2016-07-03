using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.Utilities;

namespace Keylol.Controllers.Point
{
    /// <summary>
    /// 据点 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("point")]
    public partial class PointController : ApiController
    {
        private readonly KeylolDbContext _dbContext;
        private readonly CachedDataProvider _cachedData;

        /// <summary>
        /// 创建 <see cref="PointController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        public PointController(KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            _dbContext = dbContext;
            _cachedData = cachedData;
        }

        private async Task<string> GenerateIdCode(string name)
        {
            var convertedName = string.Join("",
                name.ToUpper().Where(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')));
            IEnumerable<string> possiblities;
            if (convertedName.Length < 5)
            {
                possiblities = Enumerable.Range(0, 20)
                    .Select(i =>
                        $"{convertedName}{Guid.NewGuid().ToString().Substring(0, 5 - convertedName.Length).ToUpper()}");
            }
            else
            {
                var combinations = convertedName.AllCombinations(5).Select(idCode => string.Join("", idCode));
                var randomList = Enumerable.Range(0, 20)
                    .Select(i => Guid.NewGuid().ToString().Substring(0, 5).ToUpper());
                possiblities = combinations.Concat(randomList);
            }
            foreach (var idCode in possiblities)
            {
                if (_dbContext.Points.Local.All(p => p.IdCode != idCode) &&
                    await _dbContext.Points.AllAsync(p => p.IdCode != idCode))
                    return idCode;
            }
            throw new Exception("无法找到可用的 IdCode");
        }
    }
}