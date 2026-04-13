using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class AttributeValueOwnership : BaseEntity
    {
        public OwnerType OwnerType { get; set; }
        public bool CanRead { get; set; } = true;
        public bool CanModify { get; set; } = true;
        public bool CanDelegate { get; set; } = false;
        public string? Notes { get; set; }
        
        public Guid ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;
        public Guid ResourceAttributeDefinitionId { get; set; }
        public ResourceAttributeDefinition AttributeDefinition { get; set; } = null!;
        public Guid OwnerResourceId { get; set; }
        public Resource OwnerResource { get; set; } = null!;
    }
}