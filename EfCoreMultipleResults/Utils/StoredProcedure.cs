
using System.Data.SqlClient;

namespace EfCoreMultipleResults.Utils
{
    public class StoredProcedure
    {
        public SqlParameter[] Parameters { get; set; }
        public string Name { get; set; }
    }
}
