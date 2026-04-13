using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceOwnership : BaseEntity
    {
        public OwnerType OwnerType { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public string? Notes { get; set; }

        public Guid ResourceId { get; set; }

        public Resource Resource { get; set; } = null!;

        public Guid OwnerResourceId { get; set; }
        public Resource OwnerResource { get; set; } = null!;
    }
}