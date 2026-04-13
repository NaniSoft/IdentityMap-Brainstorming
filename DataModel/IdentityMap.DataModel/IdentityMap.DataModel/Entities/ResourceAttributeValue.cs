using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceAttributeValue : BaseEntity
    {
        public string? ValueString { get; set; }    // String, Text, Url, Enum key, ResourceReference Guid

        public int? ValueInt { get; set; }
        public double? ValueDouble { get; set; }
        public bool? ValueBool { get; set; }
        public DateTime? ValueDateTime { get; set; }

        public int? Ordinal { get; set; }

        public Guid ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;
        public Guid ResourceAttributeDefinitionId { get; set; }
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;

        public ICollection<AttributeValueOwnership> ValueOwnerships { get; set; }
            = new List<AttributeValueOwnership>();

        public object? TypedValue => AttributeDefinition?.DataType switch
        {
            AttributeDataType.Integer => ValueInt,
            AttributeDataType.Double => ValueDouble,
            AttributeDataType.Boolean => ValueBool,
            AttributeDataType.DateTime => ValueDateTime,
            _ => ValueString
        };
    }
}