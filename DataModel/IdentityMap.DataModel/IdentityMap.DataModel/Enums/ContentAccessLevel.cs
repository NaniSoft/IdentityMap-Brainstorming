using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum ContentAccessLevel
    {
        None = 0,
        Execute = 1,   // invoke only — does not imply data read/write
        Read = 2,   // view data
        Write = 3,   // create or update (implies Read)
        Delete = 4,   // remove data (implies Read; may require Write first by policy)
        Admin = 5    // full control (implies Delete, Write, Read, Execute)
    }
}
