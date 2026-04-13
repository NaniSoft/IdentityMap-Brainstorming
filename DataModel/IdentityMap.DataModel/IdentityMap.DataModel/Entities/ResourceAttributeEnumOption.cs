namespace IdentityMap.DataModel.Entities
{
    public class ResourceAttributeEnumOption : BaseEntity
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? BadgeColour { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public Guid ResourceAttributeDefinitionId { get; set; }
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;
    }
}