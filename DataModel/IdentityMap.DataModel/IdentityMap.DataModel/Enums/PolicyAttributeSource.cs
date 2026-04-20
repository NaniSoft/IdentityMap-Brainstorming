using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum PolicyAttributeSource
    {
        Subject,        // attribute is derived from the subject (e.g. user or group)
        Resource,       // attribute is derived from the resource (e.g. sensitivity)
        Environment,     // attribute is derived from the environment/context (e.g. time of day, location)
        Action,          // attribute is derived from the action being performed (e.g. read vs write)
    }
}
