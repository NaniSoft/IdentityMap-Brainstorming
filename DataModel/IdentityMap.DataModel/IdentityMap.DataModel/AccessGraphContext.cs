using IdentityMap.DataModel.Entities;

namespace IdentityMap.DataModel
{
    public class AccessGraphContext
    {
        public List<Resource> Resources { get; } = new();
        public List<ResourceRelationship> Relationships { get; } = new();
        public List<BusinessAppMembership> Memberships { get; } = new();
        public List<ResourceCapability> Capabilities { get; } = new();
        public List<CapabilityGrant> Grants { get; } = new();
        public List<ResourceAttributeValue> AttributeValues { get; } = new();
        public List<ResourceAttributeDefinition> AttributeDefinitions { get; } = new();
        public List<ResourceOwnership> Ownerships { get; } = new();

        public Resource? FindResource(Guid id) =>
            Resources.FirstOrDefault(r => r.Id == id);
    }
}