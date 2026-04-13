using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceAttributeDefinition : BaseEntity
    {
        public Guid ResourceTypeSchemaId { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string? HelpText { get; set; }

        public string? GroupName { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public AttributeDataType DataType { get; set; }

        public bool IsRequired { get; set; } = false;
        public bool IsMultiValue { get; set; } = false;

        public string? DefaultValue { get; set; }

        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        public string? RegexPattern { get; set; }

        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }

        public ResourceType? AllowedReferenceType { get; set; }

        public bool IsDeprecated { get; set; } = false;

        public ResourceTypeSchema ResourceTypeSchema { get; set; } = null!;

        public ICollection<ResourceAttributeEnumOption> EnumOptions { get; set; }
            = new List<ResourceAttributeEnumOption>();

        public ICollection<ResourceAttributeValue> Values { get; set; }
            = new List<ResourceAttributeValue>();

        public ICollection<AttributeDefinitionOwnership> DefinitionOwnerships { get; set; }
            = new List<AttributeDefinitionOwnership>();
    }
}