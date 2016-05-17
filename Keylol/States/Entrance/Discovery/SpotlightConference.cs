using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;

namespace Keylol.States.Entrance.Discovery
{
    /// <summary>
    /// 精选专题列表
    /// </summary>
    public class SpotlightConferenceList : List<SpotlightConference>
    {
        /// <summary>
        /// 创建 <see cref="SpotlightConferenceList"/>
        /// </summary>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="SpotlightConferenceList"/></returns>
        public static Task<SpotlightConferenceList> CreateAsync(KeylolDbContext dbContext)
        {
            return Task.FromResult(new SpotlightConferenceList());
        }
    }

    /// <summary>
    /// 精选专题
    /// </summary>
    public class SpotlightConference
    {
    }
}