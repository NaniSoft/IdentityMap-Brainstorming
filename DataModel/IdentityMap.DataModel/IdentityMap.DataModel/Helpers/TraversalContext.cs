using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public class TraversalContext
    {
        public TraversalMode Mode { get; }
        public HashSet<Guid> Visited { get; } = new();
        public List<string> AccumulatedPolicyConditions { get; } = new();
        public int MaxDepth { get; init; } = 15;
        public bool IncludeInactiveGrants { get; init; } = false;
        public bool IncludeExpiredGrants { get; init; } = false;
        public TraversalContext(TraversalMode mode)
        {
            Mode = mode;
        }

        public bool ShouldFollowEdge(TraversalEdge edge)
        {
            if (edge.GrantStatus.HasValue && !IncludeInactiveGrants)
            {
                if (edge.GrantStatus != GrantStatus.Active &&
                    edge.GrantStatus != GrantStatus.PendingApproval)
                {
                    return false;
                }
            }

            return Mode switch
            {
                TraversalMode.ContentToAccessor => ShouldFollowForContentToAccessor(edge),
                TraversalMode.AccessorToContent => ShouldFollowForAccessorToContent(edge),
                TraversalMode.ResourceToInfrastructure => ShouldFollowForInfrastructure(edge),
                TraversalMode.InfrastructureToChildren => ShouldFollowForChildren(edge),
                TraversalMode.PolicyEvaluation => ShouldFollowForPolicy(edge),
                TraversalMode.Unrestricted => true,
                _ => true
            };
        }

        private bool ShouldFollowForContentToAccessor(TraversalEdge edge) =>
            edge.Category switch
            {
                EdgeCategory.Structural => !edge.IsReversed,
                EdgeCategory.Identity => true, // follow in both directions (e.g. ResourceMembership: Member → Resource)
                EdgeCategory.Access => edge.IsReversed, // only follow in the reverse direction (e.g. CapabilityGrant: Grantee → Resource)
                EdgeCategory.DataFlow => !edge.IsReversed, // only follow in the natural direction (e.g. DataBinding: DataSource → DataTarget)
                EdgeCategory.Management => true, // follow in both directions (e.g. ResourceOwnership: Owner ↔ Resource)
                _ => false// only follow in the natural direction (e.g. ResourceMembership: Resource → Member)
            };

        private bool ShouldFollowForAccessorToContent(TraversalEdge edge) =>
            edge.Category switch
            {
                EdgeCategory.Structural => edge.IsReversed, // only follow in the reverse direction (e.g. ResourceMembership: Member → Resource)
                EdgeCategory.Identity => !edge.IsReversed, // follow in both directions (e.g. ResourceMembership: Member → Resource)
                EdgeCategory.Access => !edge.IsReversed, // only follow in the natural direction (e.g. CapabilityGrant: Grantee → Resource)
                EdgeCategory.DataFlow => edge.IsReversed, // only follow in the reverse direction (e.g. DataBinding: DataSource → DataTarget)
                EdgeCategory.Management => false,
                _ => false
            };

        private bool ShouldFollowForInfrastructure(TraversalEdge edge) =>
            edge.Category == EdgeCategory.Structural && !edge.IsReversed;

        private bool ShouldFollowForChildren(TraversalEdge edge) =>
            edge.Category == EdgeCategory.Structural && edge.IsReversed;

        private bool ShouldFollowForPolicy(TraversalEdge edge) =>
            edge.Category == EdgeCategory.Access || edge.Category == EdgeCategory.Identity;
    }
}
