using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Helpers
{
    public static class OwnershipResolver
    {
        public static IEnumerable<ResourceOwnership> ResolveEffectiveOwners(
            Resource resource,
            IEnumerable<ResourceOwnership> allOwnerships,
            IEnumerable<ResourceRelationship> allRelationships)
        {
            var direct = allOwnerships
                .Where(o => o.ResourceId == resource.Id)
                .ToList();

            if (direct.Any())
            {
                foreach (var o in direct) yield return o;

                if (resource.AllowParentOwnerAccess)
                {
                    // Also surface parent owners (additive, not replacing).
                    foreach (var po in WalkParentOwnerships(
                        resource.Id, allOwnerships, allRelationships))
                        yield return po;
                }
            }
            else
            {
                // No explicit owners — inherit up the chain.
                foreach (var po in WalkParentOwnerships(
                    resource.Id, allOwnerships, allRelationships))
                    yield return po;
            }
        }

        private static IEnumerable<ResourceOwnership> WalkParentOwnerships(
            Guid resourceId,
            IEnumerable<ResourceOwnership> allOwnerships,
            IEnumerable<ResourceRelationship> allRelationships)
        {
            // Primary parent: first HostedIn or BelongsTo relationship.
            var parentRel = allRelationships
                .FirstOrDefault(r => r.ChildResourceId == resourceId
                                  && (r.Type == RelationshipType.HostedIn
                                   || r.Type == RelationshipType.BelongsTo));

            if (parentRel == null) yield break;

            var parentOwners = allOwnerships
                .Where(o => o.ResourceId == parentRel.ParentResourceId)
                .ToList();

            if (parentOwners.Any())
            {
                foreach (var o in parentOwners) yield return o;
            }
            else
            {
                // Recurse up.
                foreach (var o in WalkParentOwnerships(
                    parentRel.ParentResourceId, allOwnerships, allRelationships))
                    yield return o;
            }
        }

        public static IEnumerable<Guid> ResolveHumanApprovers(
            ResourceOwnership ownership,
            IEnumerable<BusinessAppMembership> memberships,
            IEnumerable<Resource> allResources)
        {
            var owner = allResources.First(r => r.Id == ownership.OwnerResourceId);

            switch (ownership.OwnerType)
            {
                case OwnerType.Human:
                    yield return owner.Id;
                    break;

                case OwnerType.BusinessApp:
                    foreach (var m in memberships.Where(m => m.BusinessAppResourceId == owner.Id
                    && m.IsActive
                    && m.Role is MembershipRole.Owner or MembershipRole.Delegate))
                    {
                        var member = allResources.First(r => r.Id == m.MemberResourceId);
                        if (member.Type == ResourceType.Account)
                            yield return member.Id;
                        else if (member.Type == ResourceType.Group)
                            foreach (var gm in memberships.Where(
                                g => g.BusinessAppResourceId == member.Id && g.IsActive))
                                yield return gm.MemberResourceId;
                    }
                    break;

                case OwnerType.Group:
                    foreach (var m in memberships.Where(
                        m => m.BusinessAppResourceId == owner.Id && m.IsActive))
                        yield return m.MemberResourceId;
                    break;
            }
        }

        public static IEnumerable<AttributeValueOwnership> ResolveAttributeOwners(
            Guid resourceId,
            Guid attributeDefinitionId,
            IEnumerable<AttributeValueOwnership> instanceOwnerships,
            IEnumerable<AttributeDefinitionOwnership> schemaOwnerships)
        {
            var instanceLevel = instanceOwnerships
                .Where(o => o.ResourceId == resourceId
                         && o.ResourceAttributeDefinitionId == attributeDefinitionId)
                .ToList();

            if (instanceLevel.Any())
                return instanceLevel;

            // Fall back to schema-level.
            return schemaOwnerships
                .Where(o => o.ResourceAttributeDefinitionId == attributeDefinitionId)
                .Select(o => new AttributeValueOwnership
                {
                    ResourceId = resourceId,
                    ResourceAttributeDefinitionId = attributeDefinitionId,
                    OwnerResourceId = o.OwnerResourceId,
                    OwnerType = o.OwnerType,
                    CanRead = o.CanRead,
                    CanModify = o.CanModify,
                    CanDelegate = o.CanDelegate
                });
        }
    }
}