using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Keylol.Models.DAL
{
    public class KeylolDbConfiguration : DbConfiguration
    {
        public KeylolDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}