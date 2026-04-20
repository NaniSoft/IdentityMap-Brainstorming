using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum EdgeCategory
    {
        Structural,     // inherent relationships that define the resource graph (e.g. ownership, hierarchy)
        Identity,       // relationships that confer effective permissions (e.g. capability grants, implications)
        Access,         // relationships that indicate actual access paths (e.g. content bindings, group membership)
        DataFlow,       // relationships that indicate data flow between resources (e.g. data source to data consumer)
        Management,     // relationships that indicate operational management (e.g. resource A manages resource B)
    }
}
