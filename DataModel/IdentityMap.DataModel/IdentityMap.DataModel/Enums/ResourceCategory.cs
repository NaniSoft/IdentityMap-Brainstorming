using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum ResourceCategory
    {
        Infrastructure,     // VMs, network devices, storage
        Data,               // databases, tables, files, queues
        Application,        // web apps, endpoints, APIs
        Identity,           // accounts, groups, roles, credentials
        Network,            // network zones, firewall rules, VPN profiles
        Governance          // policies, GPOs, compliance objects
    }
}
