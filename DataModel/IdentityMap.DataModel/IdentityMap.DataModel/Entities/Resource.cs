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

        // ── Tags ─────────────────────────────────────────────────────────────
        // Free-form string tags used by the frontend view-plane compiler to:
        //   • determine compound container grouping  (e.g. "tier:database")
        //   • drive icon / colour selection          (e.g. "kind:human")
        //   • apply quick filters without traversing relationships
        //
        // Naming convention:
        //   tier:<tier>          tier:identity | tier:database | tier:web
        //   kind:<kind>          kind:human | kind:svc-account | kind:ad-group |
        //                        kind:sql-server | kind:database | kind:table |
        //                        kind:row | kind:server-role | kind:server-login |
        //                        kind:db-role | kind:db-user | kind:webapp |
        //                        kind:endpoint | kind:idp
        //   container:<slug>     derived at export time from HostedIn relationships
        //                        e.g. container:corp-ad | container:sqlsrv-prod-01
        //   env:<env>            env:prod | env:staging | env:dev
        //   sensitivity:<label>  derived at export time from the Sensitivity field
        public List<string> Tags { get; set; } = new List<string>();

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