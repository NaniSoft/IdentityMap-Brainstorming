using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public static class TraversalEdgeBuilder
    {
        private static readonly HashSet<RelationshipType> StructuralTypes = new HashSet<RelationshipType>
        {
            RelationshipType.HostedIn,
            RelationshipType.BelongsTo,
            RelationshipType.RunsOn,
            RelationshipType.ProtectedBy
        };

        private static readonly HashSet<RelationshipType> IdentityTypes = new HashSet<RelationshipType>
        {
            RelationshipType.UsesIdentity,
            RelationshipType.DependsOn,
            RelationshipType.AuthenticatesVia,
            RelationshipType.AuthorizesVia,
            RelationshipType.SynchronizedFrom
        };

        private static readonly HashSet<RelationshipType> ManagementTypes = new HashSet<RelationshipType>
        {
            RelationshipType.Manages
        };

        private static readonly HashSet<RelationshipType> DataFlowTypes = new HashSet<RelationshipType>
        {
            RelationshipType.ReplicaOf,
            RelationshipType.BackupOf
        };

        public static IEnumerable<TraversalEdge> BuildAllEdgesFrom(
            Guid resourceId, AccessGraphContext ctx)
        {
            foreach (var edge in BuildRelationshipEdges(resourceId, ctx))
                yield return edge;
            foreach (var edge in BuildContentBindingEdges(resourceId, ctx))
                yield return edge;
            foreach (var edge in BuildGrantEdges(resourceId, ctx))
                yield return edge;
            foreach (var edge in BuildMembershipEdges(resourceId, ctx))
                yield return edge;
            foreach (var edge in BuildAttributeReferenceEdges(resourceId, ctx))
                yield return edge;
        }

        private static IEnumerable<TraversalEdge> BuildRelationshipEdges(Guid resourceId, AccessGraphContext ctx)
        {
            foreach (var rel in ctx.Relationships.Where(r => r.ChildResourceId == resourceId))
            {
                var category = CategorizeRelationship(rel.Type);
                yield return new TraversalEdge
                {
                    EdgeId = rel.Id,
                    SourceId = rel.ChildResourceId,
                    TargetId = rel.ParentResourceId,
                    Category = category,
                    EdgeType = rel.Type.ToString(),
                    Label = rel.Type.ToString(),
                    IsReversed = false,
                    Notes = rel.Notes
                };
            }
        }

        private static IEnumerable<TraversalEdge> BuildContentBindingEdges(Guid resourceId, AccessGraphContext ctx)
        {
            foreach (var binding in ctx.ContentBindings.Where(b => b.IsActive && b.ContentSourceId == resourceId))
            {
                yield return new TraversalEdge
                {
                    EdgeId = binding.Id,
                    SourceId = binding.ContentSourceId,
                    TargetId = binding.ConsumerResourceId,
                    Category = EdgeCategory.DataFlow,
                    EdgeType = $"ContentBinding:{binding.AccessType}:{binding.Role}",
                    Label = $"ContentSource -> ({binding.Role})",
                    IsReversed = false,
                    BindingAccessType = binding.AccessType,
                    BindingRole = binding.Role,
                    AccessorResourceId = binding.AccessorResourceId,
                    Notes = binding.ContributionDescription
                };
            }

            foreach (var binding in ctx.ContentBindings.Where(b => b.IsActive && b.ConsumerResourceId == resourceId))
            {
                yield return new TraversalEdge
                {
                    EdgeId = binding.Id,
                    SourceId = binding.ConsumerResourceId,
                    TargetId = binding.ContentSourceId,
                    Category = EdgeCategory.DataFlow,
                    EdgeType = $"ContentBinding:{binding.AccessType}:{binding.Role}",
                    Label = $"Consumes <- ({binding.Role})",
                    IsReversed = true,
                    BindingAccessType = binding.AccessType,
                    BindingRole = binding.Role,
                    AccessorResourceId = binding.AccessorResourceId,
                    Notes = binding.ContributionDescription
                };
            }

            foreach (var binding in ctx.ContentBindings.Where(b => b.IsActive && b.AccessorResourceId == resourceId))
            {
                yield return new TraversalEdge
                {
                    EdgeId = binding.Id,
                    SourceId = resourceId,
                    TargetId = binding.ConsumerResourceId,
                    Category = EdgeCategory.Identity,
                    EdgeType = $"AccessorFor",
                    Label = $"Accessor -> Consumer",
                    IsReversed = false,
                    AccessorResourceId = resourceId
                };

                yield return new TraversalEdge
                {
                    EdgeId = binding.Id,
                    SourceId = resourceId,
                    TargetId = binding.ContentSourceId,
                    Category = EdgeCategory.Access,
                    EdgeType = $"AccessorAccesses:{binding.AccessType}",
                    Label = $"Accessor -> Source ({binding.AccessType})",
                    IsReversed = false,
                    BindingAccessType = binding.AccessType
                };
            }
        }

        private static IEnumerable<TraversalEdge> BuildGrantEdges(Guid resourceId, AccessGraphContext ctx)
        {
            var capabilityMap = ctx.Capabilities.ToDictionary(c => c.Id);

            foreach (var grant in ctx.Grants.Where(g => g.SubjectResourceId == resourceId))
            {
                if (!capabilityMap.TryGetValue(grant.ResourceCapabilityId, out var cap))
                    continue;

                var policyConditions = ctx.PolicyConditions?
                    .Where(p => p.CapabilityGrantId == grant.Id || p.ResourceCapabilityId == cap.Id)
                    .Select(p => p.ConditionExpression)
                    .ToList() ?? new List<string>();

                yield return new TraversalEdge
                {
                    EdgeId = grant.Id,
                    SourceId = grant.SubjectResourceId,
                    TargetId = cap.ResourceId,
                    Category = EdgeCategory.Access,
                    EdgeType = $"Grant:{cap.Type}",
                    Label = $"Has {cap.Type} Grant",
                    IsReversed = false,
                    GrantStatus = grant.Status,
                    CapabilityType = cap.Type,
                    CapabilityScope = cap.Scope,
                    Notes = grant.Justification,
                    PolicyConditions = policyConditions
                };
            }

            var capsOnResource = ctx.Capabilities.Where(c => c.ResourceId == resourceId).Select(c => c.Id).ToHashSet();

            foreach (var grant in ctx.Grants.Where(g => capsOnResource.Contains(g.ResourceCapabilityId)))
            {
                if (!capabilityMap.TryGetValue(grant.ResourceCapabilityId, out var cap))
                    continue;

                var policyConditions = ctx.PolicyConditions?
                    .Where(p => p.CapabilityGrantId == grant.Id || p.ResourceCapabilityId == cap.Id)
                    .Select(p => p.ConditionExpression)
                    .ToList() ?? new List<string>();

                yield return new TraversalEdge
                {
                    EdgeId = grant.Id,
                    SourceId = resourceId,
                    TargetId = grant.SubjectResourceId,
                    Category = EdgeCategory.Access,
                    EdgeType = $"GrantedTo:{cap.Type}",
                    Label = $"Granted {cap.Type} To",
                    IsReversed = true,
                    GrantStatus = grant.Status,
                    CapabilityType = cap.Type,
                    CapabilityScope = cap.Scope,
                    Notes = grant.Justification,
                    PolicyConditions = policyConditions
                };
            }
        }

        private static IEnumerable<TraversalEdge> BuildMembershipEdges(Guid resourceId, AccessGraphContext ctx)
        {
            foreach (var membership in ctx.Memberships.Where(m => m.IsActive && m.MemberResourceId == resourceId))
            {
                yield return new TraversalEdge
                {
                    EdgeId = membership.Id,
                    SourceId = membership.MemberResourceId,
                    TargetId = membership.BusinessAppResourceId,
                    Category = EdgeCategory.Access,
                    EdgeType = $"Membership: {membership.Role}",
                    Label = $"Member ({membership.Role})",
                    IsReversed = false
                };
            }

            foreach (var membership in ctx.Memberships.Where(m => m.IsActive && m.BusinessAppResourceId == resourceId))
            {
                yield return new TraversalEdge
                {
                    EdgeId = membership.Id,
                    SourceId = membership.BusinessAppResourceId,
                    TargetId = membership.MemberResourceId,
                    Category = EdgeCategory.Access,
                    EdgeType = $"HasMember: {membership.Role}",
                    Label = $"Has Member {membership.Role}",
                    IsReversed = true
                };
            }
        }

        private static IEnumerable<TraversalEdge> BuildAttributeReferenceEdges(Guid resourceId, AccessGraphContext ctx)
        {
            var outgoingRefs = ctx.AttributeValues
                .Where(av => av.ResourceId == resourceId && av.AttributeDefinition?.DataType == AttributeDataType.ResourceReference
                && Guid.TryParse(av.ValueString, out _));

            foreach(var attrVal in outgoingRefs)
            {
                var targetId = Guid.Parse(attrVal.ValueString!);
                var attrKey = attrVal.AttributeDefinition?.Key ?? "reference";
                
                yield return new TraversalEdge
                {
                    EdgeId = attrVal.Id,
                    SourceId = resourceId,
                    TargetId = targetId,
                    Category = EdgeCategory.Identity,
                    EdgeType = $"LinkedTo:{attrKey}",
                    Label = $"Linked via {attrKey}",
                    IsReversed = false
                };
            }

            var incomingRefs = ctx.AttributeValues
                .Where(av => av.ValueString == resourceId.ToString() && av.AttributeDefinition?.DataType == AttributeDataType.ResourceReference);

            foreach(var attrVal in incomingRefs)
            {
                var attrKey = attrVal.AttributeDefinition?.Key ?? "reference";

                yield return new TraversalEdge
                {
                    EdgeId = attrVal.Id,
                    SourceId = resourceId,
                    TargetId = attrVal.ResourceId,
                    Category = EdgeCategory.Identity,
                    EdgeType = $"LinkedFrom:{attrKey}",
                    Label = $"Referenced by {attrKey}",
                    IsReversed = true
                };
            }
        }

        private static EdgeCategory CategorizeRelationship(RelationshipType type)
        {
            if (StructuralTypes.Contains(type)) return EdgeCategory.Structural;
            if (IdentityTypes.Contains(type)) return EdgeCategory.Identity;
            if (ManagementTypes.Contains(type)) return EdgeCategory.Management;
            if (DataFlowTypes.Contains(type)) return EdgeCategory.DataFlow;
            return EdgeCategory.Structural;
        }
    }
}
