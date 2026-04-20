using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum TraversalMode
    {
        ContentToAccessor,      // find who can access a sensitive resource
        AccessorToContent,       // find what sensitive resources a given identity can access
        ResourceToInfrastructure,   // find the infrastructure dependencies of a resource (e.g. for impact analysis)
        InfrastructureToChildren,   // find all resources that depend on a given infrastructure component (e.g. for risk analysis)
        Unrestricted,             // traverse all relationships regardless of direction or type (e.g. for graph visualization)
        PolicyEvaluation,           // special mode for evaluating CapabilityGrants with custom logic in AccessGraphResolver
    }
}
