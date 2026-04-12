// =============================================================================
//  IT Resource Governance Model
//  Models IT elements, ownership, relationships, content, capabilities,
//  capability grants, and multi-owner approval workflows.
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITGovernance.Domain
{
    // =========================================================================
    // ENUMS
    // =========================================================================

    /// <summary>
    /// Classifies every concrete IT element that can be modelled as a Resource.
    /// </summary>
    public enum ResourceType
    {
        // Infrastructure
        Organization,
        Region,
        Datacenter,
        PhysicalMachine,
        VirtualMachine,
        NetworkDevice,
        StorageDevice,
        CloudSubscription,

        // Software / Platform
        OperatingSystem,
        BusinessApp,        // special: acts as a group/governor for owned resources
        ServiceEndpoint,    // REST / gRPC / SOAP endpoint
        Queue,
        Database,
        Container,
        Cluster,

        // Data
        Folder,
        File,
        DataStore,

        // Identity
        Account,            // human user account
        ServiceAccount,     // non-human service identity
        Group,
        Role
    }

    /// <summary>
    /// Structural / semantic type of a relationship between two resources.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>VM lives inside a PhysicalMachine; File lives inside a Folder.</summary>
        HostedIn,

        /// <summary>Loose logical containment (e.g. Region contains Datacenters).</summary>
        BelongsTo,

        /// <summary>One resource depends on another to function.</summary>
        DependsOn,

        /// <summary>Peer / replica relationship.</summary>
        ReplicaOf,

        /// <summary>A resource is a member of a group/cluster resource.</summary>
        MemberOf
    }

    /// <summary>
    /// The operations that can be performed on a resource or its content.
    /// </summary>
    public enum CapabilityType
    {
        /// <summary>Read / list the resource or its content.</summary>
        Read,

        /// <summary>Write / create new content inside the resource.</summary>
        Write,

        /// <summary>Modify the resource's own configuration or metadata.</summary>
        ModifyResource,

        /// <summary>Modify the data / content hosted by the resource.</summary>
        ModifyContent,

        /// <summary>Delete the resource itself.</summary>
        Delete,

        /// <summary>Delete content inside the resource (e.g. purge messages in a queue).</summary>
        DeleteContent,

        /// <summary>Invoke / execute the resource (e.g. call an endpoint, run a script).</summary>
        Execute,

        /// <summary>Administrative control: change settings, restart, scale.</summary>
        Administer,

        /// <summary>Delegate ownership or grant capabilities to others.</summary>
        Delegate
    }

    /// <summary>
    /// Whether the capability targets the resource itself or the content it holds.
    /// </summary>
    public enum CapabilityScope
    {
        /// <summary>The operation acts on the resource object itself (metadata, config).</summary>
        Resource,

        /// <summary>The operation acts on content hosted within the resource.</summary>
        Content
    }

    /// <summary>
    /// How the resource relates to its own content.
    /// </summary>
    public enum ContentAccessModel
    {
        /// <summary>
        /// The resource IS the content (e.g. a File: accessing the resource means
        /// accessing its bytes directly).
        /// </summary>
        ResourceIsContent,

        /// <summary>
        /// The resource is an access surface for content that lives elsewhere
        /// (e.g. a ServiceEndpoint whose dynamic payload changes on every call).
        /// </summary>
        AccessSurface
    }

    /// <summary>
    /// Whether content inside a resource changes at runtime.
    /// </summary>
    public enum ContentNature
    {
        /// <summary>Content is fixed / version-controlled (files in a Folder).</summary>
        Static,

        /// <summary>Content changes independently of the resource (Endpoint response, Queue messages).</summary>
        Dynamic
    }

    /// <summary>
    /// How many owners must approve before a CapabilityGrant becomes active.
    /// </summary>
    public enum ApprovalPolicy
    {
        /// <summary>Any single resolved owner may approve.</summary>
        AnyOwner,

        /// <summary>Every resolved owner must approve.</summary>
        AllOwners
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
        /// <summary>A human Account resource.</summary>
        Human,

        /// <summary>A BusinessApp resource; human owners are resolved transitively.</summary>
        BusinessApp,

        /// <summary>A team / Group resource; members are resolved for notifications.</summary>
        Group
    }

    /// <summary>
    /// The primitive data type of a <see cref="ResourceAttributeDefinition"/>.
    /// Drives UI rendering (text box, number spinner, date picker, dropdown)
    /// and determines which typed column of <see cref="ResourceAttributeValue"/>
    /// holds the live value.
    /// </summary>
    public enum AttributeDataType
    {
        /// <summary>Free-form text. Stored in <c>ValueString</c>.</summary>
        String,

        /// <summary>32-bit signed integer. Stored in <c>ValueInt</c>.</summary>
        Integer,

        /// <summary>64-bit floating-point. Stored in <c>ValueDouble</c>.</summary>
        Double,

        /// <summary>Boolean flag. Stored in <c>ValueBool</c>. Rendered as toggle.</summary>
        Boolean,

        /// <summary>UTC date/time. Stored in <c>ValueDateTime</c>. Rendered as date-picker.</summary>
        DateTime,

        /// <summary>
        /// Constrained set of values defined by <see cref="ResourceAttributeEnumOption"/>
        /// records. Rendered as a dropdown. Stored in <c>ValueString</c>
        /// (the chosen option's <c>Value</c> key).
        /// </summary>
        Enum,

        /// <summary>
        /// A reference to another Resource by its Id.
        /// Stored in <c>ValueString</c> (Guid as string).
        /// Rendered as a searchable resource picker.
        /// </summary>
        ResourceReference,

        /// <summary>Multi-line text / rich text. Stored in <c>ValueString</c>.</summary>
        Text,

        /// <summary>URL / URI. Stored in <c>ValueString</c>, validated as a URI.</summary>
        Url
    }


    // =========================================================================
    // BASE ENTITY
    // =========================================================================

    /// <summary>
    /// Shared audit and identity fields inherited by every entity in the model.
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Resource that created / last modified this record.</summary>
        public Guid? CreatedByResourceId { get; set; }
    }


    // =========================================================================
    // RESOURCE TYPE SCHEMA  (business-user-defined attribute blueprint)
    // =========================================================================

    /// <summary>
    /// The attribute schema for one <see cref="ResourceType"/>.
    /// A business user creates or edits this schema through the UI; the
    /// resulting attribute definitions then act as a template for creating and
    /// ingesting concrete <see cref="Resource"/> instances of that type.
    /// <br/>
    /// One <see cref="ResourceTypeSchema"/> exists per <see cref="ResourceType"/>
    /// (enforced by a unique index on <see cref="ForResourceType"/>).
    /// </summary>
    public class ResourceTypeSchema : BaseEntity
    {
        /// <summary>The resource type this schema describes.</summary>
        [Required]
        public ResourceType ForResourceType { get; set; }

        [Required, MaxLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? Description { get; set; }

        /// <summary>Schema version — increment when attributes are added/removed.</summary>
        public int Version { get; set; } = 1;

        /// <summary>Whether new resources of this type can still be created.</summary>
        public bool IsActive { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────────

        /// <summary>Ordered list of attribute definitions that form this schema.</summary>
        public ICollection<ResourceAttributeDefinition> AttributeDefinitions { get; set; }
            = new List<ResourceAttributeDefinition>();
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE DEFINITION  (one field in the schema)
    // =========================================================================

    /// <summary>
    /// Defines a single typed attribute (field) within a
    /// <see cref="ResourceTypeSchema"/>. Business users configure these in the UI;
    /// they control label, data type, validation constraints, display order,
    /// and — for Enum attributes — the allowed dropdown options.
    /// </summary>
    public class ResourceAttributeDefinition : BaseEntity
    {
        [Required]
        public Guid ResourceTypeSchemaId { get; set; }

        // ── Identity & display ────────────────────────────────────────────────

        /// <summary>
        /// Stable machine key used in code and ingestion pipelines.
        /// Snake_case recommended (e.g. "ip_address", "cpu_cores").
        /// </summary>
        [Required, MaxLength(128)]
        public string Key { get; set; } = string.Empty;

        /// <summary>Human-readable label shown in the UI form.</summary>
        [Required, MaxLength(256)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? HelpText { get; set; }

        /// <summary>
        /// Optional grouping header shown in the UI (e.g. "Network", "Hardware").
        /// Attributes sharing the same group are rendered together.
        /// </summary>
        [MaxLength(128)]
        public string? GroupName { get; set; }

        /// <summary>Render order within <see cref="GroupName"/> (or globally if no group).</summary>
        public int DisplayOrder { get; set; } = 0;

        // ── Type & validation ─────────────────────────────────────────────────

        [Required]
        public AttributeDataType DataType { get; set; }

        /// <summary>Must a value be provided when creating a resource?</summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>Can the same resource carry multiple values for this attribute?</summary>
        public bool IsMultiValue { get; set; } = false;

        /// <summary>
        /// Default value serialised as string (parsed according to <see cref="DataType"/>
        /// at read time).  Null = no default.
        /// </summary>
        [MaxLength(512)]
        public string? DefaultValue { get; set; }

        // ── String / Text constraints ─────────────────────────────────────────

        /// <summary>Minimum string length (applies to String and Text types).</summary>
        public int? MinLength { get; set; }

        /// <summary>Maximum string length (applies to String, Text, Url types).</summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Optional regex pattern for String / Url types
        /// (e.g. IPv4 pattern for an "ip_address" attribute).
        /// </summary>
        [MaxLength(512)]
        public string? RegexPattern { get; set; }

        // ── Numeric constraints ───────────────────────────────────────────────

        /// <summary>Inclusive minimum (applies to Integer and Double types).</summary>
        public double? MinValue { get; set; }

        /// <summary>Inclusive maximum (applies to Integer and Double types).</summary>
        public double? MaxValue { get; set; }

        // ── ResourceReference hints ───────────────────────────────────────────

        /// <summary>
        /// When <see cref="DataType"/> is <c>ResourceReference</c>, restricts the
        /// picker to resources of this type.  Null = any type.
        /// </summary>
        public ResourceType? AllowedReferenceType { get; set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        /// <summary>
        /// Deprecated definitions are hidden in the creation UI but retained
        /// so existing values remain readable.
        /// </summary>
        public bool IsDeprecated { get; set; } = false;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceTypeSchemaId))]
        public ResourceTypeSchema ResourceTypeSchema { get; set; } = null!;

        /// <summary>
        /// Allowed dropdown options — only populated when
        /// <see cref="DataType"/> is <see cref="AttributeDataType.Enum"/>.
        /// </summary>
        public ICollection<ResourceAttributeEnumOption> EnumOptions { get; set; }
            = new List<ResourceAttributeEnumOption>();

        /// <summary>All concrete values recorded for this definition across all resources.</summary>
        public ICollection<ResourceAttributeValue> Values { get; set; }
            = new List<ResourceAttributeValue>();
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE ENUM OPTION  (dropdown choice)
    // =========================================================================

    /// <summary>
    /// A single allowed option for an <see cref="AttributeDataType.Enum"/> attribute.
    /// Business users manage this list in the UI; the chosen option's
    /// <see cref="Value"/> key is stored in <see cref="ResourceAttributeValue.ValueString"/>.
    /// </summary>
    public class ResourceAttributeEnumOption : BaseEntity
    {
        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        /// <summary>Stable machine key stored as the attribute value (e.g. "windows_server_2022").</summary>
        [Required, MaxLength(128)]
        public string Value { get; set; } = string.Empty;

        /// <summary>Human-readable label shown in the dropdown (e.g. "Windows Server 2022").</summary>
        [Required, MaxLength(256)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(512)]
        public string? Description { get; set; }

        /// <summary>Optional hex colour for badge/chip rendering in the UI (e.g. "#4CAF50").</summary>
        [MaxLength(16)]
        public string? BadgeColour { get; set; }

        /// <summary>Render order in the dropdown list.</summary>
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE ATTRIBUTE VALUE  (one typed field value on a resource instance)
    // =========================================================================

    /// <summary>
    /// Stores the actual value of one attribute on one <see cref="Resource"/>
    /// instance.  The appropriate typed column is populated based on the
    /// attribute definition's <see cref="AttributeDataType"/>; all other typed
    /// columns are null.
    /// <br/>
    /// Multi-value attributes (IsMultiValue = true) are modelled as multiple
    /// <see cref="ResourceAttributeValue"/> rows sharing the same
    /// <see cref="ResourceId"/> and <see cref="ResourceAttributeDefinitionId"/>.
    /// </summary>
    public class ResourceAttributeValue : BaseEntity
    {
        [Required]
        public Guid ResourceId { get; set; }

        [Required]
        public Guid ResourceAttributeDefinitionId { get; set; }

        // ── Typed value columns ───────────────────────────────────────────────
        // Only the column matching the definition's DataType will be non-null.
        // ─ String, Text, Url, Enum (option key), ResourceReference (Guid string):
        [MaxLength(2048)]
        public string? ValueString { get; set; }

        // ─ Integer:
        public int? ValueInt { get; set; }

        // ─ Double:
        public double? ValueDouble { get; set; }

        // ─ Boolean:
        public bool? ValueBool { get; set; }

        // ─ DateTime (always stored as UTC):
        public DateTime? ValueDateTime { get; set; }

        // ── Multi-value ordering ──────────────────────────────────────────────

        /// <summary>
        /// Position when the same resource has multiple values for the same
        /// attribute (IsMultiValue = true).  Null for single-value attributes.
        /// </summary>
        public int? Ordinal { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        [ForeignKey(nameof(ResourceAttributeDefinitionId))]
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;

        // ── Convenience accessor ──────────────────────────────────────────────

        /// <summary>
        /// Returns the stored value box-typed to <c>object</c> based on the
        /// definition's <see cref="AttributeDataType"/>.  Requires
        /// <see cref="AttributeDefinition"/> to be loaded.
        /// </summary>
        [NotMapped]
        public object? TypedValue => AttributeDefinition?.DataType switch
        {
            AttributeDataType.Integer => ValueInt,
            AttributeDataType.Double => ValueDouble,
            AttributeDataType.Boolean => ValueBool,
            AttributeDataType.DateTime => ValueDateTime,
            _ => ValueString   // String, Text, Url, Enum, ResourceReference
        };
    }


    // =========================================================================
    // ATTRIBUTE VALUE VALIDATOR  (domain helper)
    // =========================================================================

    /// <summary>
    /// Validates a <see cref="ResourceAttributeValue"/> against its
    /// <see cref="ResourceAttributeDefinition"/> before persistence.
    /// Returns a list of human-readable error messages (empty = valid).
    /// </summary>
    public static class AttributeValueValidator
    {
        public static IReadOnlyList<string> Validate(
            ResourceAttributeValue value,
            ResourceAttributeDefinition definition)
        {
            var errors = new List<string>();

            // ── Required check ────────────────────────────────────────────────
            if (definition.IsRequired && value.TypedValue is null)
            {
                errors.Add($"'{definition.Label}' is required.");
                return errors; // further checks meaningless without a value
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
                            && !System.Text.RegularExpressions.Regex.IsMatch(s, definition.RegexPattern))
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
                        var validKeys = definition.EnumOptions
                            .Where(o => o.IsActive)
                            .Select(o => o.Value)
                            .ToHashSet();
                        if (!string.IsNullOrEmpty(chosen) && !validKeys.Contains(chosen))
                            errors.Add($"'{chosen}' is not a valid option for '{definition.Label}'.");
                        break;
                    }

                case AttributeDataType.ResourceReference:
                    {
                        if (!Guid.TryParse(value.ValueString, out _))
                            errors.Add($"'{definition.Label}' must be a valid resource identifier (GUID).");
                        break;
                    }
            }

            return errors;
        }
    }


    // =========================================================================
    // RESOURCE  (the universal IT element)
    // =========================================================================

    /// <summary>
    /// The central entity. Every IT element — machine, file, app, account — is
    /// a Resource. Concrete behaviour is driven by <see cref="Type"/> and the
    /// attached optional entities.
    /// </summary>
    public class Resource : BaseEntity
    {
        [Required, MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1024)]
        public string? Description { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        /// <summary>
        /// Logical status: Active, Decommissioned, Archived, etc.
        /// Kept as a free string so teams can extend it without migration.
        /// </summary>
        [MaxLength(64)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Arbitrary JSON blob for type-specific attributes
        /// (IP address for an Endpoint, ARN for a cloud resource, etc.).
        /// </summary>
        // ── Typed attribute values ────────────────────────────────────────────

        /// <summary>
        /// The concrete typed attribute values for this resource instance.
        /// Each value corresponds to a <see cref="ResourceAttributeDefinition"/>
        /// declared on the <see cref="ResourceTypeSchema"/> for <see cref="Type"/>.
        /// </summary>
        public ICollection<ResourceAttributeValue> AttributeValues { get; set; } = new List<ResourceAttributeValue>();

        // ── Relationships ─────────────────────────────────────────────────────

        /// <summary>
        /// Structural relationships where THIS resource is the child
        /// (e.g. "this VM is HostedIn PhysicalMachine X").
        /// </summary>
        public ICollection<ResourceRelationship> InboundRelationships { get; set; } = new List<ResourceRelationship>();

        /// <summary>
        /// Structural relationships where THIS resource is the parent
        /// (e.g. "this PhysicalMachine hosts VM Y, VM Z").
        /// </summary>
        public ICollection<ResourceRelationship> OutboundRelationships { get; set; } = new List<ResourceRelationship>();

        // ── Ownership ─────────────────────────────────────────────────────────

        /// <summary>
        /// Owner records attached to this resource.
        /// Owners are themselves Resources (Account, BusinessApp, Group).
        /// </summary>
        public ICollection<ResourceOwnership> Ownerships { get; set; } = new List<ResourceOwnership>();

        /// <summary>
        /// Resources that this resource owns (reverse nav for owner resources).
        /// </summary>
        public ICollection<ResourceOwnership> OwnedResources { get; set; } = new List<ResourceOwnership>();

        // ── Content ───────────────────────────────────────────────────────────

        /// <summary>
        /// Describes how this resource relates to the content it holds.
        /// Null means the resource has no hosted content concept.
        /// </summary>
        public ResourceContentDefinition? ContentDefinition { get; set; }

        // ── Capabilities ──────────────────────────────────────────────────────

        /// <summary>
        /// All capabilities that THIS resource exposes to potential subjects.
        /// </summary>
        public ICollection<ResourceCapability> ExposedCapabilities { get; set; } = new List<ResourceCapability>();

        /// <summary>
        /// All capability grants where THIS resource is the subject (actionable resource)
        /// that received access to another resource.
        /// </summary>
        public ICollection<CapabilityGrant> ReceivedGrants { get; set; } = new List<CapabilityGrant>();

        // ── BusinessApp membership ────────────────────────────────────────────

        /// <summary>
        /// If this resource is a BusinessApp, the human/group members it governs
        /// are linked here so ownership can be resolved transitively.
        /// </summary>
        public ICollection<BusinessAppMembership> BusinessAppMembers { get; set; } = new List<BusinessAppMembership>();

        /// <summary>
        /// BusinessApps that this resource (e.g. an Account) is a member of.
        /// </summary>
        public ICollection<BusinessAppMembership> BusinessAppMemberships { get; set; } = new List<BusinessAppMembership>();
    }


    // =========================================================================
    // RESOURCE RELATIONSHIP  (structural links)
    // =========================================================================

    /// <summary>
    /// A directed, typed structural link between two resources.
    /// <br/>
    /// Examples:
    ///   Parent=PhysicalMachine, Child=VirtualMachine, Type=HostedIn
    ///   Parent=Region,          Child=Datacenter,     Type=BelongsTo
    ///   Parent=Folder,          Child=File,           Type=HostedIn
    /// </summary>
    public class ResourceRelationship : BaseEntity
    {
        [Required]
        public Guid ParentResourceId { get; set; }

        [Required]
        public Guid ChildResourceId { get; set; }

        [Required]
        public RelationshipType Type { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

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
    /// governed resource.  Multiple ownership records can exist per resource,
    /// enabling shared ownership (e.g. both the IT team and the Business App
    /// team own the same VM).
    /// </summary>
    public class ResourceOwnership : BaseEntity
    {
        [Required]
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The owning entity — may be an Account, a BusinessApp, or a Group.
        /// </summary>
        [Required]
        public Guid OwnerResourceId { get; set; }

        [Required]
        public OwnerType OwnerType { get; set; }

        /// <summary>
        /// When ownership expires. Null = indefinite.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        [ForeignKey(nameof(OwnerResourceId))]
        public Resource OwnerResource { get; set; } = null!;
    }


    // =========================================================================
    // BUSINESS APP MEMBERSHIP
    // =========================================================================

    /// <summary>
    /// Maps human Accounts (or nested Groups) to a BusinessApp resource.
    /// When approval logic needs to resolve who a BusinessApp owner really is,
    /// it walks this table to find the concrete human Account(s).
    /// </summary>
    public class BusinessAppMembership : BaseEntity
    {
        /// <summary>The BusinessApp resource acting as the governing group.</summary>
        [Required]
        public Guid BusinessAppResourceId { get; set; }

        /// <summary>The member resource — typically an Account or a Group.</summary>
        [Required]
        public Guid MemberResourceId { get; set; }

        /// <summary>
        /// Role within the BusinessApp: e.g. "Owner", "Contributor", "Viewer".
        /// Only members with an appropriate role participate in approval resolution.
        /// </summary>
        [MaxLength(64)]
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(BusinessAppResourceId))]
        public Resource BusinessAppResource { get; set; } = null!;

        [ForeignKey(nameof(MemberResourceId))]
        public Resource MemberResource { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE CONTENT DEFINITION
    // =========================================================================

    /// <summary>
    /// Describes the content model of a resource: how the resource relates to
    /// its content, whether that content is static or dynamic, and (for static
    /// resources whose content is itself other resources) links to those child
    /// content resources.
    /// <br/>
    /// One-to-one with Resource.
    /// </summary>
    public class ResourceContentDefinition : BaseEntity
    {
        [Required]
        public Guid ResourceId { get; set; }

        /// <summary>
        /// Is the resource itself the content, or merely a surface to reach content?
        /// </summary>
        [Required]
        public ContentAccessModel AccessModel { get; set; }

        /// <summary>
        /// Does the content change at runtime (Dynamic) or is it version-controlled (Static)?
        /// </summary>
        [Required]
        public ContentNature Nature { get; set; }

        /// <summary>
        /// Human-readable description of the content schema or payload format
        /// (e.g. "JSON array of log entries", "PDF documents", "MP4 video files").
        /// </summary>
        [MaxLength(1024)]
        public string? ContentSchemaDescription { get; set; }

        /// <summary>
        /// Optional JSON Schema / OpenAPI fragment describing the dynamic content
        /// structure (most useful for ServiceEndpoint and Queue resources).
        /// </summary>
        public string? ContentSchemaJson { get; set; }

        /// <summary>
        /// Estimated maximum data volume — free text (e.g. "~50 GB", "millions of rows").
        /// </summary>
        [MaxLength(128)]
        public string? EstimatedDataVolume { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        /// <summary>
        /// For STATIC resources whose content consists of other Resources
        /// (e.g. files inside a Folder, databases on a DB Server).
        /// The child resources themselves are linked via ResourceRelationship;
        /// this collection is a convenience projection of those HostedIn links.
        /// </summary>
        public ICollection<ResourceContentItem> ContentItems { get; set; } = new List<ResourceContentItem>();
    }

    /// <summary>
    /// Links a static content resource (e.g. a File) as a content item of a
    /// parent resource's ContentDefinition.  Allows ordering and labelling.
    /// </summary>
    public class ResourceContentItem : BaseEntity
    {
        [Required]
        public Guid ContentDefinitionId { get; set; }

        /// <summary>The resource that constitutes this piece of content.</summary>
        [Required]
        public Guid ContentResourceId { get; set; }

        /// <summary>Optional display order within the parent.</summary>
        public int? Ordinal { get; set; }

        [MaxLength(512)]
        public string? Label { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ContentDefinitionId))]
        public ResourceContentDefinition ContentDefinition { get; set; } = null!;

        [ForeignKey(nameof(ContentResourceId))]
        public Resource ContentResource { get; set; } = null!;
    }


    // =========================================================================
    // RESOURCE CAPABILITY  (what can be done on / with the resource)
    // =========================================================================

    /// <summary>
    /// Declares a single capability that a resource exposes.
    /// Capabilities are owned by the resource and can be granted to subjects
    /// (other resources) through <see cref="CapabilityGrant"/>.
    /// </summary>
    public class ResourceCapability : BaseEntity
    {
        [Required]
        public Guid ResourceId { get; set; }

        [Required]
        public CapabilityType Type { get; set; }

        /// <summary>
        /// Does the capability apply to the resource itself or to its content?
        /// </summary>
        [Required]
        public CapabilityScope Scope { get; set; }

        /// <summary>
        /// How many owners must sign-off before a grant for this capability
        /// becomes active.
        /// </summary>
        [Required]
        public ApprovalPolicy ApprovalPolicy { get; set; }

        /// <summary>
        /// Optional: the capability expires this many days after being granted.
        /// Null = no automatic expiry.
        /// </summary>
        public int? GrantExpiryDays { get; set; }

        [MaxLength(1024)]
        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; } = null!;

        public ICollection<CapabilityGrant> Grants { get; set; } = new List<CapabilityGrant>();
    }


    // =========================================================================
    // CAPABILITY GRANT  (assigning a capability to a subject resource)
    // =========================================================================

    /// <summary>
    /// Represents a request (and, if approved, an active entitlement) for a
    /// subject resource to exercise a specific <see cref="ResourceCapability"/>
    /// on the target resource.
    /// <br/>
    /// Lifecycle: PendingApproval → Active (or Rejected / Revoked / Expired).
    /// </summary>
    public class CapabilityGrant : BaseEntity
    {
        /// <summary>Which capability is being granted.</summary>
        [Required]
        public Guid ResourceCapabilityId { get; set; }

        /// <summary>
        /// The actionable resource (subject) that will receive the capability —
        /// could be an Account, a ServiceAccount, a BusinessApp, a VM, etc.
        /// </summary>
        [Required]
        public Guid SubjectResourceId { get; set; }

        [Required]
        public GrantStatus Status { get; set; } = GrantStatus.PendingApproval;

        /// <summary>Business justification for the request.</summary>
        [MaxLength(2048)]
        public string? Justification { get; set; }

        /// <summary>When the grant was requested (defaults to CreatedAt).</summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>When the grant was activated (all approvals received).</summary>
        public DateTime? ActivatedAt { get; set; }

        /// <summary>When the grant expires. Null = until explicitly revoked.</summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>Reason for revocation, if applicable.</summary>
        [MaxLength(1024)]
        public string? RevocationReason { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(ResourceCapabilityId))]
        public ResourceCapability ResourceCapability { get; set; } = null!;

        [ForeignKey(nameof(SubjectResourceId))]
        public Resource SubjectResource { get; set; } = null!;

        /// <summary>
        /// One approval vote is created per resolved owner at the time of the
        /// grant request.
        /// </summary>
        public ICollection<GrantApprovalVote> ApprovalVotes { get; set; } = new List<GrantApprovalVote>();
    }


    // =========================================================================
    // GRANT APPROVAL VOTE  (per-owner approval record)
    // =========================================================================

    /// <summary>
    /// Represents the approval obligation and eventual decision of a single
    /// resolved owner for a <see cref="CapabilityGrant"/>.
    /// <br/>
    /// When an owner is a BusinessApp, <see cref="ResolvedHumanAccountId"/> is
    /// populated with the concrete human Account that will act on its behalf.
    /// When an owner is a Group, all active members may be resolved.
    /// </summary>
    public class GrantApprovalVote : BaseEntity
    {
        [Required]
        public Guid CapabilityGrantId { get; set; }

        /// <summary>
        /// The ownership record that generated this vote obligation
        /// (preserves which owner role triggered it).
        /// </summary>
        [Required]
        public Guid ResourceOwnershipId { get; set; }

        /// <summary>
        /// The actual owner resource (Account, BusinessApp, or Group) as
        /// recorded in the ownership entry.
        /// </summary>
        [Required]
        public Guid OwnerResourceId { get; set; }

        /// <summary>
        /// If the owner resource is a BusinessApp or Group, this is resolved to
        /// the concrete human Account that was assigned to act as approver.
        /// Null when the owner is already a human Account.
        /// </summary>
        public Guid? ResolvedHumanAccountId { get; set; }

        [Required]
        public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

        [MaxLength(2048)]
        public string? Comments { get; set; }

        public DateTime? DecidedAt { get; set; }

        /// <summary>When the approver was notified.</summary>
        public DateTime? NotifiedAt { get; set; }

        /// <summary>When the approval request expires if no action is taken.</summary>
        public DateTime? ApprovalDeadline { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey(nameof(CapabilityGrantId))]
        public CapabilityGrant CapabilityGrant { get; set; } = null!;

        [ForeignKey(nameof(ResourceOwnershipId))]
        public ResourceOwnership ResourceOwnership { get; set; } = null!;

        [ForeignKey(nameof(OwnerResourceId))]
        public Resource OwnerResource { get; set; } = null!;

        [ForeignKey(nameof(ResolvedHumanAccountId))]
        public Resource? ResolvedHumanAccount { get; set; }
    }


    // =========================================================================
    // DOMAIN SERVICE HELPERS  (logic that operates on the entities)
    // =========================================================================

    /// <summary>
    /// Resolves the concrete human Account(s) responsible for approving a
    /// capability grant, given a set of ownership records.
    /// BusinessApp owners are unwrapped via their BusinessAppMembership table.
    /// Group owners can similarly be expanded (pattern shown for BusinessApp).
    /// </summary>
    public static class OwnershipResolver
    {
        /// <summary>
        /// Given an ownership record, returns the Ids of all human Accounts
        /// that should receive an approval vote obligation.
        /// </summary>
        public static IEnumerable<Guid> ResolveHumanApprovers(
            ResourceOwnership ownership,
            IEnumerable<BusinessAppMembership> businessAppMemberships,
            IEnumerable<Resource> allResources)
        {
            var ownerResource = allResources.First(r => r.Id == ownership.OwnerResourceId);

            switch (ownership.OwnerType)
            {
                case OwnerType.Human:
                    // Direct human account — no resolution needed.
                    yield return ownerResource.Id;
                    break;

                case OwnerType.BusinessApp:
                    // Drill into BusinessApp members with "Owner" role.
                    foreach (var membership in businessAppMemberships
                        .Where(m => m.BusinessAppResourceId == ownerResource.Id
                                 && m.IsActive
                                 && m.Role == "Owner"))
                    {
                        var member = allResources.First(r => r.Id == membership.MemberResourceId);

                        if (member.Type == ResourceType.Account)
                        {
                            yield return member.Id;
                        }
                        else if (member.Type == ResourceType.Group)
                        {
                            // Recursively expand group members (simplified: one level).
                            foreach (var groupMember in businessAppMemberships
                                .Where(m => m.BusinessAppResourceId == member.Id && m.IsActive))
                            {
                                yield return groupMember.MemberResourceId;
                            }
                        }
                    }
                    break;

                case OwnerType.Group:
                    // Expand group members directly.
                    foreach (var membership in businessAppMemberships
                        .Where(m => m.BusinessAppResourceId == ownerResource.Id && m.IsActive))
                    {
                        yield return membership.MemberResourceId;
                    }
                    break;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="CapabilityGrant"/> can be activated
        /// based on the <see cref="ApprovalPolicy"/> of its capability and the
        /// current state of its <see cref="GrantApprovalVote"/> records.
        /// </summary>
        public static bool IsGrantApprovable(
            CapabilityGrant grant,
            ResourceCapability capability)
        {
            var votes = grant.ApprovalVotes;

            if (capability.ApprovalPolicy == ApprovalPolicy.AnyOwner)
            {
                return votes.Any(v => v.Decision == ApprovalDecision.Approved);
            }

            // AllOwners: every vote must be Approved; none Rejected.
            return votes.Any()
                && votes.All(v => v.Decision == ApprovalDecision.Approved);
        }

        /// <summary>
        /// Returns true if ANY vote has been Rejected (grant is permanently blocked).
        /// </summary>
        public static bool IsGrantRejected(CapabilityGrant grant) =>
            grant.ApprovalVotes.Any(v => v.Decision == ApprovalDecision.Rejected);
    }
}