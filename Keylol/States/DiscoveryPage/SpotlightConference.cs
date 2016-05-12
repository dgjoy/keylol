using System.Collections.Generic;
using System.Threading.Tasks;
using Keylol.Models.DAL;

namespace Keylol.States.DiscoveryPage
{
    /// <summary>
    /// Spotlight Conference List
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
    /// Spotlight Conference
    /// </summary>
    public class SpotlightConference
    {
    }
}