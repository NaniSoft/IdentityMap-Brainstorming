using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{

    public class Resource : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ResourceType Type { get; set; }

        public string Status { get; set; } = "Active";

        public bool AllowParentOwnerAccess { get; set; } = false;

        public ContentAccessModel? ContentAccessModel { get; set; }

        public ContentNature? ContentNature { get; set; }

        public string? ContentSchemaDescription { get; set; }

        public SensitivityClassification Sensitivity { get; set; }
            = SensitivityClassification.None;

        public ICollection<ResourceRelationship> InboundRelationships { get; set; }
            = new List<ResourceRelationship>();

        public ICollection<ResourceRelationship> OutboundRelationships { get; set; }
            = new List<ResourceRelationship>();

        public ICollection<ResourceOwnership> Ownerships { get; set; }
            = new List<ResourceOwnership>();

        public ICollection<ResourceOwnership> OwnedResources { get; set; }
            = new List<ResourceOwnership>();

        public ICollection<ResourceCapability> ExposedCapabilities { get; set; }
            = new List<ResourceCapability>();

        public ICollection<CapabilityGrant> ReceivedGrants { get; set; }
            = new List<CapabilityGrant>();

        public ICollection<ResourceAttributeValue> AttributeValues { get; set; }
            = new List<ResourceAttributeValue>();

        public ICollection<AttributeValueOwnership> AttributeValueOwnerships { get; set; }
            = new List<AttributeValueOwnership>();

        public ICollection<BusinessAppMembership> BusinessAppMembers { get; set; }
            = new List<BusinessAppMembership>();

        public ICollection<BusinessAppMembership> BusinessAppMemberships { get; set; }
            = new List<BusinessAppMembership>();

        public ICollection<ContentBinding> ContentBindingsAsConsumer { get; set; }
            = new List<ContentBinding>();

        public ICollection<ContentBinding> ContentBindingsAsSource { get; set; }
            = new List<ContentBinding>();

        public ICollection<ContentBinding> ContentBindingsAsAccessor { get; set; }
            = new List<ContentBinding>();
    }
}