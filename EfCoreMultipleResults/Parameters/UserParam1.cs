using EfCoreMultipleResults.Utils;
using System.Collections.Generic;

namespace EfCoreMultipleResults.Parameters
{
    public class UserParam1
    {
        public int UserId { get; set; }

    }
    public class UserParam2
    {
        //used for passing table value parameters
        [SqlParameterConfig(true)]
        public List<TableValueInDB> ListUserId { get; set; }

    }

    //should be an exact map with the table type , in ur sql server
    public class TableValueInDB
    {
        public int TableColoumnId { get; set; }
    }

}
