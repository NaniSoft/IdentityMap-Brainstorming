using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceRelationship : BaseEntity
    {
        public RelationshipType Type { get; set; }

        public string? Notes { get; set; }

        public Guid ParentResourceId { get; set; }
        public Resource Parent { get; set; } = null!;

        public Guid ChildResourceId { get; set; }
        public Resource Child { get; set; } = null!;
    }
}