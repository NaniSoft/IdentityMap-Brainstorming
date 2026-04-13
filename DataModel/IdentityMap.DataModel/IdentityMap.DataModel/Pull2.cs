// =============================================================================
//  IT Resource Governance Model  —  v3
//
//  Design decisions in this version:
//
//  1. OWNERSHIP INHERITANCE
//     A resource with no explicit owners inherits owners from its nearest
//     ancestor (via HostedIn / BelongsTo chain).  When a resource DOES have
//     explicit owners, AllowParentOwnerAccess=true lets the parent's owners
//     also govern it simultaneously.  OwnershipResolver encodes this logic.
//
//  2. ATTRIBUTE-LEVEL OWNERSHIP
//     AttributeDefinitionOwnership  →  schema-level  (applies to all resources
//       of this type for a given attribute)
//     AttributeValueOwnership       →  instance-level (overrides schema-level
//       for one specific resource instance)
//     Both carry CanRead / CanModify / CanDelegate rights.
//
//  3. CAPABILITY SCOPES
//     SelfManagement  —  operations on the resource's own config and lifecycle
//                        (modify attributes, delete, administer, delegate)
//     ContentAccess   —  operations on content the resource holds or serves
//                        (read, write, execute, delete content)
//     A resource can be BOTH an entity with its own config AND the content
//     of a parent resource.  The parent's ContentAccess capabilities target
//     it as content; its own capabilities govern what's inside it.
//
//  4. CAPABILITY-LEVEL APPROVAL REQUIREMENTS
//     ResourceCapability.ApprovalRequirements lets you name EXACT approvers
//     (accounts, business apps, groups) per capability — not just "any owner"
//     or "all owners".  Approvers sharing an ApproverGroupTag form a group;
//     ApprovalGroupSatisfaction controls whether AnyOne or All in the group
//     suffice.  All distinct groups must be satisfied.  IsVetoPower=true means
//     a rejection from that approver immediately blocks the grant.
//     When no explicit requirements are defined, DefaultApprovalPolicy falls
//     back to the resource ownership list.
//
//  5. BUSINESS APP AS RESOURCE
//     BusinessApp is NOT a separate C# class.  It is a Resource with
//     Type = ResourceType.BusinessApp.  Its type-specific attributes (app code,
//     department, criticality, etc.) are defined through the ResourceTypeSchema
//     for BusinessApp.  BusinessAppMembership is a join table — it links member
//     Accounts/Groups to the BusinessApp resource for governance resolution.
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace ITGovernance.Domain
{
    // =========================================================================
    // ENUMS
    // =========================================================================

    /// <summary>Classifies every concrete IT element modelled as a Resource.</summary>
    public enum ResourceType
    {
        // Organisational / geographic
        Organization,
        Region,
        Datacenter,

        // Infrastructure
        PhysicalMachine,
        VirtualMachine,
        NetworkDevice,
        StorageDevice,
        CloudSubscription,

        // Software / platform
        OperatingSystem,

        /// <summary>
        /// A group/governor resource.  Not a separate C# class — it IS a Resource
        /// with this type.  Members are tracked via BusinessAppMembership.
        /// Type-specific attributes are defined in its ResourceTypeSchema.
        /// </summary>
        BusinessApp,

        ServiceEndpoint,
        Queue,
        Database,
        Container,
        Cluster,

        // Data
        Folder,
        File,
        DataStore,

        // Identity
        Account,           // human user
        ServiceAccount,    // non-human service identity
        Group,
        Role
    }

    /// <summary>Structural / semantic type of a relationship between two resources.</summary>
    public enum RelationshipType
    {
        /// <summary>VM lives inside a PhysicalMachine; File lives inside a Folder.</summary>
        HostedIn,

        /// <summary>Loose logical containment (Region → Datacenter).</summary>
        BelongsTo,

        DependsOn,
        ReplicaOf,
        MemberOf
    }

    /// <summary>Primitive operations that can be declared as capabilities.</summary>
    public enum CapabilityType
    {
        /// <summary>Read the resource's own metadata / config (SelfManagement) or content (ContentAccess).</summary>
        Read,

        /// <summary>Create new content inside the resource. ContentAccess scope.</summary>
        Write,

        /// <summary>Alter the resource's own configuration or attributes. SelfManagement scope.</summary>
        ModifyResource,

        /// <summary>Alter data / content hosted within the resource. ContentAccess scope.</summary>
        ModifyContent,

        /// <summary>Remove the resource itself. SelfManagement scope.</summary>
        Delete,

        /// <summary>Purge content inside the resource (e.g. queue messages). ContentAccess scope.</summary>
        DeleteContent,

        /// <summary>Invoke / execute (call endpoint, run script). ContentAccess scope.</summary>
        Execute,

        /// <summary>Admin-level control: restart, scale, reconfigure. SelfManagement scope.</summary>
        Administer,

        /// <summary>Transfer or delegate ownership. SelfManagement scope.</summary>
        Delegate
    }

    /// <summary>
    /// Determines what a capability operates on.
    /// </summary>
    public enum CapabilityScope
    {
        /// <summary>
        /// Operates on the resource's own state — its attributes, configuration, or
        /// existence.  Think: "manage this IT element as an object."
        /// Approval falls back to the resource's direct owners.
        /// </summary>
        SelfManagement,

        /// <summary>
        /// Operates on the content the resource holds or serves — the data inside
        /// a folder, the payload returned by an endpoint, the rows in a database.
        /// This is also the scope used by a PARENT resource when it treats a child
        /// resource as content (e.g. WebApp deleting one of its Endpoints).
        /// Approval can be routed to content-specific owners via
        /// CapabilityApprovalRequirement, independently of resource owners.
        /// </summary>
        ContentAccess
    }

    /// <summary>How a resource relates to its own content.</summary>
    public enum ContentAccessModel
    {
        /// <summary>The resource IS the content (a File: access the resource = access its bytes).</summary>
        ResourceIsContent,

        /// <summary>The resource is an access surface; content lives dynamically inside or behind it.</summary>
        AccessSurface
    }

    /// <summary>Whether content inside a resource changes at runtime.</summary>
    public enum ContentNature
    {
        Static,
        Dynamic
    }

    public enum GrantStatus
    {
        PendingApproval,
        Active,
        Rejected,
        Revoked,
        Expired
    }

    public enum ApprovalDecision
    {
        Pending,
        Approved,
        Rejected
    }

    public enum OwnerType
    {
        Human,
        BusinessApp,
        Group
    }

    /// <summary>
    /// Fallback approval policy for a ResourceCapability when no explicit
    /// CapabilityApprovalRequirement records are defined.
    /// </summary>
    public enum DefaultApprovalPolicy
    {
        /// <summary>Any single resolved owner may approve.</summary>
        AnyOwner,

        /// <summary>Every resolved owner must approve.</summary>
        AllOwners,

        /// <summary>No approval needed; grant activates immediately.</summary>
        NoApprovalRequired
    }

    /// <summary>
    /// Within an approver group (same ApproverGroupTag), how many members
    /// must approve before the group is considered satisfied?
    /// </summary>
    public enum ApprovalGroupSatisfaction
    {
        /// <summary>Any one member of the group approving satisfies the group.</summary>
        AnyOne,

        /// <summary>Every member of the group must approve.</summary>
        All
    }

    /// <summary>The primitive data type of a ResourceAttributeDefinition.</summary>
    public enum AttributeDataType
    {
        String,
        Integer,
        Double,
        Boolean,
        DateTime,

        /// <summary>Dropdown. Allowed values defined via ResourceAttributeEnumOption.</summary>
        Enum,

        /// <summary>Reference to another Resource by Id. Stored as Guid string.</summary>
        ResourceReference,

        /// <summary>Multi-line text / rich text.</summary>
        Text,

        /// <summary>URL / URI. Validated as a URI.</summary>
        Url
    }


    // =========================================================================
    // BASE ENTITY
    // =========================================================================

    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedByResourceId { get; set; }
    }


    // =========================================================================
    // RESOURCE TYPE SCHEMA  (business-user-defined attribute blueprint)
    // =========================================================================

    /// <summary>
    /// The attribute schema for one ResourceType, defined in the UI by a
    /// business user.  One schema per ResourceType (unique index on
    /// ForResourceType).  BusinessApp has its own schema here — no C# subclass
    /// needed.
    /// </summary>
    public class ResourceTypeSchema : BaseEntity
    {
        [Required]
        public ResourceType ForResourceType { get; set; }

        [Required, MaxLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? Description { get; set; }

        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public ICollection<ResourceAttributeDefinition> AttributeDefinitions { get; set; }
            = new List<ResourceAttributeDefinition>();
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE DEFINITION  (one field in the schema)
    // =========================================================================

    public class ResourceAttributeDefinition : BaseEntity
    {
        [Required]
        public Guid ResourceTypeSchemaId { get; set; }

        /// <summary>Stable machine key (e.g. "ip_address", "cpu_cores").</summary>
        [Required, MaxLength(128)]
        public string Key { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? HelpText { get; set; }

        [MaxLength(128)]
        public string? GroupName { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Required]
        public AttributeDataType DataType { get; set; }

        public bool IsRequired { get; set; } = false;
        public bool IsMultiValue { get; set; } = false;

        [MaxLength(512)]
        public string? DefaultValue { get; set; }

        // String / Text validation
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        [MaxLength(512)]
        public string? RegexPattern { get; set; }

        // Numeric validation
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }

        // ResourceReference hint
        public ResourceType? AllowedReferenceType { get; set; }

        public bool IsDeprecated { get; set; } = false;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceTypeSchemaId))]
        public ResourceTypeSchema ResourceTypeSchema { get; set; } = null!;

        public ICollection<ResourceAttributeEnumOption> EnumOptions { get; set; }
            = new List<ResourceAttributeEnumOption>();

        public ICollection<ResourceAttributeValue> Values { get; set; }
            = new List<ResourceAttributeValue>();

        /// <summary>
        /// Schema-level ownership: applies to this attribute across ALL resources
        /// of its type, unless overridden by AttributeValueOwnership on a
        /// specific resource instance.
        /// </summary>
        public ICollection<AttributeDefinitionOwnership> DefinitionOwnerships { get; set; }
            = new List<AttributeDefinitionOwnership>();
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE ENUM OPTION
    // =========================================================================

    public class ResourceAttributeEnumOption : BaseEntity
    {
        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        [Required, MaxLength(128)]
        public string Value { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(512)]
        public string? Description { get; set; }

        [MaxLength(16)]
        public string? BadgeColour { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;
    }


    // =========================================================================
    // ATTRIBUTE DEFINITION OWNERSHIP  (schema-level)
    // =========================================================================

    /// <summary>
    /// Assigns governance rights over a specific attribute definition to an
    /// owner resource.  This is a schema-level contract: "for ALL resources of
    /// type VirtualMachine, the ip_address attribute is governed by the Network
    /// Team account."
    ///
    /// Instance-level overrides are expressed via AttributeValueOwnership.
    /// </summary>
    public class AttributeDefinitionOwnership : BaseEntity
    {
        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        [Required]
        public Guid OwnerResourceId { get; set; }

        [Required]
        public OwnerType OwnerType { get; set; }

        /// <summary>May this owner read the attribute value?</summary>
        public bool CanRead { get; set; } = true;

        /// <summary>May this owner modify the attribute value?</summary>
        public bool CanModify { get; set; } = true;

        /// <summary>May this owner delegate this attribute's governance to another resource?</summary>
        public bool CanDelegate { get; set; } = false;

        [MaxLength(512)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;

        [ForeignKey(nameof(OwnerResourceId))]
        public Resource OwnerResource { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE VALUE  (typed value on a resource instance)
    // =========================================================================

    public class ResourceAttributeValue : BaseEntity
    {
        [Required]
        public Guid ResourceId { get; set; }

        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        // Typed columns — only the one matching DataType is non-null.
        [MaxLength(2048)]
        public string? ValueString { get; set; }    // String, Text, Url, Enum key, ResourceReference Guid

        public int? ValueInt { get; set; }
        public double? ValueDouble { get; set; }
        public bool? ValueBool { get; set; }
        public DateTime? ValueDateTime { get; set; }

        /// <summary>Position when IsMultiValue = true. Null for single-value attributes.</summary>
        public int? Ordinal { get; set; }

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;

        /// <summary>
        /// Instance-level attribute ownerships for this specific value.
        /// These OVERRIDE any AttributeDefinitionOwnership records for the same
        /// definition when they exist on this resource.
        /// </summary>
        public ICollection<AttributeValueOwnership> ValueOwnerships { get; set; }
            = new List<AttributeValueOwnership>();

        [NotMapped]
        public object? TypedValue => AttributeDefinition?.DataType switch
        {
            AttributeDataType.Integer => ValueInt,
            AttributeDataType.Double => ValueDouble,
            AttributeDataType.Boolean => ValueBool,
            AttributeDataType.DateTime => ValueDateTime,
            _ => ValueString
        };
    }


    // =========================================================================
    // ATTRIBUTE VALUE OWNERSHIP  (instance-level override)
    // =========================================================================

    /// <summary>
    /// Assigns governance rights over one attribute on one specific resource
    /// instance.  Takes precedence over AttributeDefinitionOwnership when both
    /// exist for the same resource + attribute combination.
    ///
    /// Example: by default, Network Team owns the ip_address attribute for all
    /// VMs (schema-level).  For VM-789 specifically, a different team owns it
    /// (instance-level override here).
    /// </summary>
    public class AttributeValueOwnership : BaseEntity
    {
        /// <summary>The specific resource instance this ownership applies to.</summary>
        [Required]
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The attribute definition being governed.  Combined with ResourceId,
        /// this uniquely identifies "attribute X on resource Y."
        /// </summary>
        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        [Required]
        public Guid OwnerResourceId { get; set; }

        [Required]
        public OwnerType OwnerType { get; set; }

        public bool CanRead { get; set; } = true;
        public bool CanModify { get; set; } = true;
        public bool CanDelegate { get; set; } = false;

        [MaxLength(512)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;

        [ForeignKey(nameof(OwnerResourceId))]
        public Resource OwnerResource { get; set; } = null!;
    }


    // =========================================================================
    // ATTRIBUTE VALUE VALIDATOR
    // =========================================================================

    public static class AttributeValueValidator
    {
        public static IReadOnlyList<string> Validate(
            ResourceAttributeValue value,
            ResourceAttributeDefinition definition)
        {
            var errors = new List<string>();

            if (definition.IsRequired && value.TypedValue is null)
            {
                errors.Add($"'{definition.Label}' is required.");
                return errors;
            }

            if (value.TypedValue is null) return errors;

            switch (definition.DataType)
            {
                case AttributeDataType.String:
                case AttributeDataType.Text:
                case AttributeDataType.Url:
                    {
                        var s = value.ValueString ?? string.Empty;
                        if (definition.MinLength.HasValue && s.Length < definition.MinLength)
                            errors.Add($"'{definition.Label}' must be at least {definition.MinLength} characters.");
                        if (definition.MaxLength.HasValue && s.Length > definition.MaxLength)
                            errors.Add($"'{definition.Label}' must not exceed {definition.MaxLength} characters.");
                        if (!string.IsNullOrEmpty(definition.RegexPattern)
                            && !Regex.IsMatch(s, definition.RegexPattern))
                            errors.Add($"'{definition.Label}' does not match the required format.");
                        break;
                    }

                case AttributeDataType.Integer:
                    {
                        var i = (double)(value.ValueInt ?? 0);
                        if (definition.MinValue.HasValue && i < definition.MinValue)
                            errors.Add($"'{definition.Label}' must be ≥ {definition.MinValue}.");
                        if (definition.MaxValue.HasValue && i > definition.MaxValue)
                            errors.Add($"'{definition.Label}' must be ≤ {definition.MaxValue}.");
                        break;
                    }

                case AttributeDataType.Double:
                    {
                        var d = value.ValueDouble ?? 0d;
                        if (definition.MinValue.HasValue && d < definition.MinValue)
                            errors.Add($"'{definition.Label}' must be ≥ {definition.MinValue}.");
                        if (definition.MaxValue.HasValue && d > definition.MaxValue)
                            errors.Add($"'{definition.Label}' must be ≤ {definition.MaxValue}.");
                        break;
                    }

                case AttributeDataType.Enum:
                    {
                        var chosen = value.ValueString;
                        var valid = definition.EnumOptions.Where(o => o.IsActive).Select(o => o.Value).ToHashSet();
                        if (!string.IsNullOrEmpty(chosen) && !valid.Contains(chosen))
                            errors.Add($"'{chosen}' is not a valid option for '{definition.Label}'.");
                        break;
                    }

                case AttributeDataType.ResourceReference:
                    if (!Guid.TryParse(value.ValueString, out _))
                        errors.Add($"'{definition.Label}' must be a valid resource identifier.");
                    break;
            }

            return errors;
        }
    }


    // =========================================================================
    // RESOURCE  (the universal IT element)
    // =========================================================================

    /// <summary>
    /// Every IT element — machine, file, endpoint, business app, account — is
    /// a Resource.  Type drives concrete behaviour; the schema / value system
    /// drives type-specific attributes.
    ///
    /// OWNERSHIP RULE:
    ///   • If Ownerships is empty → owners are inherited from the nearest
    ///     ancestor via the ResourceRelationship chain.
    ///   • If Ownerships is non-empty AND AllowParentOwnerAccess = false →
    ///     only these explicit owners govern the resource.
    ///   • If Ownerships is non-empty AND AllowParentOwnerAccess = true →
    ///     both explicit owners AND the parent's resolved owners govern it.
    ///
    /// BUSINESSAPP NOTE:
    ///   BusinessApp is simply Type = ResourceType.BusinessApp.  Its specific
    ///   attributes (app code, department, criticality tier, …) are declared in
    ///   the ResourceTypeSchema for BusinessApp — no C# subclass is required.
    /// </summary>
    public class Resource : BaseEntity
    {
        [Required, MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? Description { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        [MaxLength(64)]
        public string Status { get; set; } = "Active";

        // ── Ownership flags ───────────────────────────────────────────────────

        /// <summary>
        /// When true AND the resource has its own explicit owners, the parent
        /// resource's owners ALSO have governance access over this resource.
        /// When false (default), explicit ownership is exclusive.
        /// Has no effect when the resource has no explicit owners — parent
        /// ownership is always inherited in that case.
        /// </summary>
        public bool AllowParentOwnerAccess { get; set; } = false;

        // ── Navigation ────────────────────────────────────────────────────────

        public ICollection<ResourceRelationship> InboundRelationships { get; set; }
            = new List<ResourceRelationship>();

        public ICollection<ResourceRelationship> OutboundRelationships { get; set; }
            = new List<ResourceRelationship>();

        public ICollection<ResourceOwnership> Ownerships { get; set; }
            = new List<ResourceOwnership>();

        public ICollection<ResourceOwnership> OwnedResources { get; set; }
            = new List<ResourceOwnership>();

        public ResourceContentDefinition? ContentDefinition { get; set; }

        public ICollection<ResourceCapability> ExposedCapabilities { get; set; }
            = new List<ResourceCapability>();

        public ICollection<CapabilityGrant> ReceivedGrants { get; set; }
            = new List<CapabilityGrant>();

        /// <summary>Typed attribute values for this instance.</summary>
        public ICollection<ResourceAttributeValue> AttributeValues { get; set; }
            = new List<ResourceAttributeValue>();

        /// <summary>
        /// Instance-level attribute ownership overrides for this resource.
        /// These take precedence over the corresponding AttributeDefinitionOwnership
        /// records when both exist.
        /// </summary>
        public ICollection<AttributeValueOwnership> AttributeValueOwnerships { get; set; }
            = new List<AttributeValueOwnership>();

        /// <summary>
        /// If this resource is a BusinessApp (Type = ResourceType.BusinessApp),
        /// its members (Accounts, Groups) are listed here for ownership resolution.
        /// </summary>
        public ICollection<BusinessAppMembership> BusinessAppMembers { get; set; }
            = new List<BusinessAppMembership>();

        public ICollection<BusinessAppMembership> BusinessAppMemberships { get; set; }
            = new List<BusinessAppMembership>();
    }


    // =========================================================================
    // RESOURCE RELATIONSHIP
    // =========================================================================

    /// <summary>
    /// Directed, typed structural link (Parent → Child).
    /// HostedIn and BelongsTo relationships are the primary chain used by
    /// OwnershipResolver when walking up for inheritance.
    /// </summary>
    public class ResourceRelationship : BaseEntity
    {
        [Required] public Guid ParentResourceId { get; set; }
        [Required] public Guid ChildResourceId { get; set; }
        [Required] public RelationshipType Type { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ParentResourceId))]
        public Resource Parent { get; set; } = null!;

        [ForeignKey(nameof(ChildResourceId))]
        public Resource Child { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE OWNERSHIP
    // =========================================================================

    /// <summary>
    /// Associates an owner resource (Account, BusinessApp, or Group) with a
    /// governed resource.  Multiple records per resource = shared ownership.
    /// </summary>
    public class ResourceOwnership : BaseEntity
    {
        [Required] public Guid ResourceId { get; set; }

        [Required] public Guid OwnerResourceId { get; set; }

        [Required] public OwnerType OwnerType { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        [ForeignKey(nameof(OwnerResourceId))]
        public Resource OwnerResource { get; set; } = null!;
    }


    // =========================================================================
    // BUSINESS APP MEMBERSHIP  (join table, NOT a subclass of Resource)
    // =========================================================================

    /// <summary>
    /// Maps member Accounts and Groups to a BusinessApp resource.
    /// Used by OwnershipResolver.ResolveHumanApprovers() to drill down from
    /// a BusinessApp owner to the concrete humans who must act on approvals.
    ///
    /// The BusinessApp itself is a Resource with Type = ResourceType.BusinessApp.
    /// This entity is purely the membership relationship.
    /// </summary>
    public class BusinessAppMembership : BaseEntity
    {
        [Required] public Guid BusinessAppResourceId { get; set; }

        [Required] public Guid MemberResourceId { get; set; }

        /// <summary>
        /// Role within the app.  "Owner" role members participate in approval
        /// resolution; other roles (e.g. "Viewer") do not.
        /// </summary>
        [MaxLength(64)]
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(BusinessAppResourceId))]
        public Resource BusinessAppResource { get; set; } = null!;

        [ForeignKey(nameof(MemberResourceId))]
        public Resource MemberResource { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE CONTENT DEFINITION
    // =========================================================================

    public class ResourceContentDefinition : BaseEntity
    {
        [Required] public Guid ResourceId { get; set; }

        /// <summary>Is the resource itself the content, or an access surface to reach it?</summary>
        [Required] public ContentAccessModel AccessModel { get; set; }

        [Required] public ContentNature Nature { get; set; }

        [MaxLength(1024)]
        public string? ContentSchemaDescription { get; set; }

        public string? ContentSchemaJson { get; set; }

        [MaxLength(128)]
        public string? EstimatedDataVolume { get; set; }

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        public ICollection<ResourceContentItem> ContentItems { get; set; }
            = new List<ResourceContentItem>();
    }


    // =========================================================================
    // RESOURCE CONTENT ITEM
    // =========================================================================

    public class ResourceContentItem : BaseEntity
    {
        [Required] public Guid ContentDefinitionId { get; set; }

        [Required] public Guid ContentResourceId { get; set; }

        public int? Ordinal { get; set; }

        [MaxLength(512)]
        public string? Label { get; set; }

        [ForeignKey(nameof(ContentDefinitionId))]
        public ResourceContentDefinition ContentDefinition { get; set; } = null!;

        [ForeignKey(nameof(ContentResourceId))]
        public Resource ContentResource { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE CAPABILITY
    // =========================================================================

    /// <summary>
    /// Declares a single capability that a resource exposes to potential subjects.
    ///
    /// SCOPE:
    ///   SelfManagement  —  the capability acts on the resource's own config /
    ///                      lifecycle.  Example: Delete (remove the VM itself).
    ///   ContentAccess   —  the capability acts on the content the resource
    ///                      holds or serves.  Example: Read (query the database),
    ///                      Execute (call the endpoint), Write (upload to folder).
    ///                      Also used by parent resources when their children ARE
    ///                      the content: a WebApp's ContentAccess.Delete capability
    ///                      lets a subject remove Endpoint resources from it.
    ///
    /// APPROVAL:
    ///   If ApprovalRequirements is non-empty → those specific approvers govern
    ///   this capability, independently of who the resource owners are.
    ///   If ApprovalRequirements is empty → DefaultApprovalPolicy is applied
    ///   against the resource's resolved owners (which may be inherited).
    /// </summary>
    public class ResourceCapability : BaseEntity
    {
        [Required] public Guid ResourceId { get; set; }

        [Required] public CapabilityType Type { get; set; }

        [Required] public CapabilityScope Scope { get; set; }

        /// <summary>
        /// Fallback policy when no explicit CapabilityApprovalRequirement records
        /// exist for this capability.
        /// </summary>
        [Required]
        public DefaultApprovalPolicy DefaultApprovalPolicy { get; set; }
            = DefaultApprovalPolicy.AnyOwner;

        public int? GrantExpiryDays { get; set; }

        [MaxLength(1024)]
        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        /// <summary>
        /// Explicit approver routing for this capability.  When present, these
        /// requirements fully replace the DefaultApprovalPolicy logic.
        /// Approvers do NOT need to be resource owners — they can be any Resource
        /// (e.g. a dedicated compliance team account, an external auditor group).
        /// </summary>
        public ICollection<CapabilityApprovalRequirement> ApprovalRequirements { get; set; }
            = new List<CapabilityApprovalRequirement>();

        public ICollection<CapabilityGrant> Grants { get; set; }
            = new List<CapabilityGrant>();
    }


    // =========================================================================
    // CAPABILITY APPROVAL REQUIREMENT
    // =========================================================================

    /// <summary>
    /// Names a specific approver for a ResourceCapability.
    ///
    /// GROUPING LOGIC:
    ///   Requirements sharing the same non-null ApproverGroupTag form a group.
    ///   Within a group, GroupSatisfaction says whether AnyOne or All members
    ///   must approve for the group to be satisfied.
    ///   All distinct groups must be satisfied for the overall capability grant
    ///   to advance to Active.
    ///   A null ApproverGroupTag = a standalone requirement (its own group of one).
    ///
    /// VETO:
    ///   IsVetoPower = true means a rejection from this approver immediately
    ///   blocks the grant regardless of other decisions.
    ///
    /// EXAMPLES:
    ///   — "IT Lead OR Security Lead must approve" (AnyOne group, two requirements)
    ///   — "Legal AND Compliance must BOTH approve" (two standalone requirements)
    ///   — "Either of two data stewards, AND the CISO" (one group + one standalone)
    /// </summary>
    public class CapabilityApprovalRequirement : BaseEntity
    {
        [Required] public Guid ResourceCapabilityId { get; set; }

        /// <summary>The approver — Account, BusinessApp, or Group resource.</summary>
        [Required] public Guid ApproverResourceId { get; set; }

        [Required] public OwnerType ApproverType { get; set; }

        /// <summary>
        /// Groups this requirement with others sharing the same tag.
        /// Null = standalone.
        /// </summary>
        [MaxLength(128)]
        public string? ApproverGroupTag { get; set; }

        /// <summary>Approval policy within this requirement's group.</summary>
        public ApprovalGroupSatisfaction GroupSatisfaction { get; set; }
            = ApprovalGroupSatisfaction.AnyOne;

        /// <summary>
        /// When true, a Rejected decision from this approver permanently blocks
        /// the grant, bypassing all other votes.
        /// </summary>
        public bool IsVetoPower { get; set; } = false;

        [MaxLength(512)]
        public string? Notes { get; set; }

        [ForeignKey(nameof(ResourceCapabilityId))]
        public ResourceCapability Capability { get; set; } = null!;

        [ForeignKey(nameof(ApproverResourceId))]
        public Resource ApproverResource { get; set; } = null!;
    }


    // =========================================================================
    // CAPABILITY GRANT
    // =========================================================================

    /// <summary>
    /// A request for a subject resource to exercise a specific capability.
    /// Lifecycle: PendingApproval → Active (or Rejected / Revoked / Expired).
    /// </summary>
    public class CapabilityGrant : BaseEntity
    {
        [Required] public Guid ResourceCapabilityId { get; set; }

        /// <summary>The actionable resource (subject) receiving the capability.</summary>
        [Required] public Guid SubjectResourceId { get; set; }

        [Required]
        public GrantStatus Status { get; set; } = GrantStatus.PendingApproval;

        [MaxLength(2048)]
        public string? Justification { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(1024)]
        public string? RevocationReason { get; set; }

        [ForeignKey(nameof(ResourceCapabilityId))]
        public ResourceCapability ResourceCapability { get; set; } = null!;

        [ForeignKey(nameof(SubjectResourceId))]
        public Resource SubjectResource { get; set; } = null!;

        public ICollection<GrantApprovalVote> ApprovalVotes { get; set; }
            = new List<GrantApprovalVote>();
    }


    // =========================================================================
    // GRANT APPROVAL VOTE
    // =========================================================================

    /// <summary>
    /// One approver's vote on a CapabilityGrant.
    ///
    /// When generated from an explicit CapabilityApprovalRequirement, the
    /// CapabilityApprovalRequirementId is set and ResolvedHumanAccountId carries
    /// the concrete human if the approver was a BusinessApp or Group.
    ///
    /// When generated from the DefaultApprovalPolicy (no explicit requirements),
    /// CapabilityApprovalRequirementId is null and ResourceOwnershipId records
    /// which ownership record triggered the vote.
    /// </summary>
    public class GrantApprovalVote : BaseEntity
    {
        [Required] public Guid CapabilityGrantId { get; set; }

        /// <summary>Set when the vote was generated from an explicit requirement.</summary>
        public Guid? CapabilityApprovalRequirementId { get; set; }

        /// <summary>Set when the vote was generated from the default policy (ownership fallback).</summary>
        public Guid? ResourceOwnershipId { get; set; }

        /// <summary>The direct approver resource (Account, BusinessApp, or Group).</summary>
        [Required] public Guid ApproverResourceId { get; set; }

        /// <summary>
        /// Resolved human Account if the ApproverResource is a BusinessApp or Group.
        /// Null when the approver is already a human Account.
        /// </summary>
        public Guid? ResolvedHumanAccountId { get; set; }

        [Required]
        public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

        [MaxLength(2048)]
        public string? Comments { get; set; }

        public DateTime? DecidedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? ApprovalDeadline { get; set; }

        [ForeignKey(nameof(CapabilityGrantId))]
        public CapabilityGrant CapabilityGrant { get; set; } = null!;

        [ForeignKey(nameof(CapabilityApprovalRequirementId))]
        public CapabilityApprovalRequirement? ApprovalRequirement { get; set; }

        [ForeignKey(nameof(ResourceOwnershipId))]
        public ResourceOwnership? ResourceOwnership { get; set; }

        [ForeignKey(nameof(ApproverResourceId))]
        public Resource ApproverResource { get; set; } = null!;

        [ForeignKey(nameof(ResolvedHumanAccountId))]
        public Resource? ResolvedHumanAccount { get; set; }
    }


    // =========================================================================
    // OWNERSHIP RESOLVER  (domain service)
    // =========================================================================

    /// <summary>
    /// Resolves effective owners for a resource, factoring in:
    ///   1. Explicit ownership records.
    ///   2. The AllowParentOwnerAccess flag.
    ///   3. Automatic parent-chain inheritance when no explicit owners exist.
    ///   4. BusinessApp / Group drill-down to concrete human Accounts.
    /// </summary>
    public static class OwnershipResolver
    {
        /// <summary>
        /// Returns the ResourceOwnership records that effectively govern this
        /// resource, applying inheritance rules.
        /// </summary>
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

        /// <summary>
        /// Drills a single ResourceOwnership record down to the concrete human
        /// Account Ids that must act on approvals.
        /// </summary>
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
                    foreach (var m in memberships.Where(m =>
                        m.BusinessAppResourceId == owner.Id
                        && m.IsActive
                        && m.Role == "Owner"))
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

        /// <summary>
        /// Resolves the effective attribute owner for a specific attribute on a
        /// specific resource.  Instance-level (AttributeValueOwnership) takes
        /// precedence over schema-level (AttributeDefinitionOwnership).
        /// </summary>
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


    // =========================================================================
    // CAPABILITY APPROVAL RESOLVER  (domain service)
    // =========================================================================

    /// <summary>
    /// Evaluates approval state for a CapabilityGrant given the capability's
    /// explicit requirements or its DefaultApprovalPolicy.
    /// </summary>
    public static class CapabilityApprovalResolver
    {
        /// <summary>
        /// Returns true when the grant has gathered sufficient approvals to be
        /// activated.  Returns false if any veto-power approver has rejected.
        /// </summary>
        public static bool IsGrantSatisfied(
            CapabilityGrant grant,
            ResourceCapability capability,
            IEnumerable<ResourceOwnership> effectiveOwners)
        {
            var votes = grant.ApprovalVotes.ToList();
            var requirements = capability.ApprovalRequirements.ToList();

            // ── Veto check (applies regardless of path) ─────────────────────
            foreach (var rejectedVote in votes.Where(v => v.Decision == ApprovalDecision.Rejected))
            {
                if (requirements.Any())
                {
                    var req = requirements.FirstOrDefault(
                        r => r.ApproverResourceId == rejectedVote.ApproverResourceId);
                    if (req?.IsVetoPower == true) return false;
                }
                else
                {
                    // Default policy — any rejection blocks when AllOwners policy.
                    if (capability.DefaultApprovalPolicy == DefaultApprovalPolicy.AllOwners)
                        return false;
                }
            }

            // ── Explicit requirements path ───────────────────────────────────
            if (requirements.Any())
            {
                // All distinct groups must be satisfied.
                var groups = requirements.GroupBy(
                    r => r.ApproverGroupTag ?? r.Id.ToString());

                foreach (var group in groups)
                {
                    var groupReqs = group.ToList();
                    var satisfaction = groupReqs.First().GroupSatisfaction;

                    var groupVotes = votes.Where(v =>
                        groupReqs.Any(r => r.ApproverResourceId == v.ApproverResourceId))
                        .ToList();

                    bool groupSatisfied = satisfaction == ApprovalGroupSatisfaction.AnyOne
                        ? groupVotes.Any(v => v.Decision == ApprovalDecision.Approved)
                        : groupVotes.Any()
                          && groupVotes.All(v => v.Decision == ApprovalDecision.Approved);

                    if (!groupSatisfied) return false;
                }

                return true;
            }

            // ── Default policy path ──────────────────────────────────────────
            return capability.DefaultApprovalPolicy switch
            {
                DefaultApprovalPolicy.NoApprovalRequired
                    => true,
                DefaultApprovalPolicy.AnyOwner
                    => votes.Any(v => v.Decision == ApprovalDecision.Approved),
                DefaultApprovalPolicy.AllOwners
                    => votes.Any()
                       && votes.All(v => v.Decision == ApprovalDecision.Approved),
                _ => false
            };
        }

        /// <summary>
        /// Returns true if any veto-power approver has rejected, permanently
        /// blocking the grant.
        /// </summary>
        public static bool IsGrantVetoed(
            CapabilityGrant grant,
            IReadOnlyList<CapabilityApprovalRequirement> requirements)
        {
            foreach (var vote in grant.ApprovalVotes.Where(
                v => v.Decision == ApprovalDecision.Rejected))
            {
                var req = requirements.FirstOrDefault(
                    r => r.ApproverResourceId == vote.ApproverResourceId);
                if (req?.IsVetoPower == true) return true;
            }
            return false;
        }
    }
}