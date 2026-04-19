using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class Resource : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ResourceType Type { get; set; }
        public ResourceCategory Category { get; set; }      // NEW — derived grouping
        public string Status { get; set; } = "Active";

        // ── External system sync ──────────────────────────────────────────────
        // When resources are imported from AD, HR, SIEM etc., record the
        // originating system and its native identifier so re-sync is idempotent.
        public string? ExternalSystem { get; set; }         // NEW — e.g. "ActiveDirectory"
        public string? ExternalId { get; set; }             // NEW — e.g. "S-1-5-21-..."

        // ── Content classification ────────────────────────────────────────────
        // Make ContentAccessModel non-nullable with a default so every resource
        // is explicitly classified as content or a surface.
        public ContentAccessModel ContentAccessModel { get; set; }   // was nullable
            = ContentAccessModel.ResourceIsContent;
        public ContentNature? ContentNature { get; set; }
        public string? ContentSchemaDescription { get; set; }

        // ── Sensitivity ───────────────────────────────────────────────────────
        // Declared: set by the data owner for this specific resource.
        public SensitivityClassification Sensitivity { get; set; }
            = SensitivityClassification.None;

        // NEW: EffectiveSensitivity is the MAX of Sensitivity and the highest
        // sensitivity among all content sources bound to this resource (via
        // ContentBinding). Computed by SensitivityBubbleUpHelper and cached here
        // so the frontend/API never needs to traverse the whole graph in real time.
        public SensitivityClassification EffectiveSensitivity { get; set; }
            = SensitivityClassification.None;

        // NEW: When true this resource can be the SubjectResource of a
        // CapabilityGrant (i.e. it can "hold" permissions). True for Account,
        // ServiceAccount, Group, Role. False for Table, File, Endpoint etc.
        // Prevents nonsensical grants like granting "Read" TO a table.
        public bool IsActionable { get; set; } = false;

        // NEW: The resource that operationally manages this resource.
        // E.g. a DBA account manages a database, an IdP manages accounts in it.
        // Different from ownership: management is operational, ownership is governance.
        public Guid? ManagedByResourceId { get; set; }
        public Resource? ManagedBy { get; set; }

        public bool AllowParentOwnerAccess { get; set; } = false;

        public List<string> Tags { get; set; } = new List<string>();

        // ... (all existing navigation properties remain unchanged)
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
        public ICollection<ResourcePolicy> Policies { get; set; }  // NEW
            = new List<ResourcePolicy>();
    }
}