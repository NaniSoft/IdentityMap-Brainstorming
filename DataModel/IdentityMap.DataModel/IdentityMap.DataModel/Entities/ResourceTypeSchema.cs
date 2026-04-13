using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceTypeSchema : BaseEntity
    {
        public ResourceType ForResourceType { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public ICollection<ResourceAttributeDefinition> AttributeDefinitions { get; set; }
            = new List<ResourceAttributeDefinition>();
    }
}