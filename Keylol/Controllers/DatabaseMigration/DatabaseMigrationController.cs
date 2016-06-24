using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DAL;

namespace Keylol.Controllers.DatabaseMigration
{
    /// <summary>
    ///     数据库迁移 Controller，迁移方法必须要保证幂等性
    /// </summary>
    [Authorize(Users = "Stackia")]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        /// 创建 <see cref="DatabaseMigrationController"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        public DatabaseMigrationController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 添加一个据点职员
        /// </summary>
        /// <param name="pointId">据点 ID</param>
        /// <param name="staffId">职员 ID</param>
        /// <returns></returns>
        [Route("add-point-staff")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPointStaff(string pointId, string staffId)
        {
            _dbContext.PointStaff.Add(new PointStaff
            {
                PointId = pointId,
                StaffId = staffId
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}