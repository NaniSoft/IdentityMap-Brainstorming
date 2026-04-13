using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class CapabilityApprovalRequirement : BaseEntity
    {
        public OwnerType ApproverType { get; set; }

        public string? ApproverGroupTag { get; set; }

        public ApprovalGroupSatisfaction GroupSatisfaction { get; set; }
            = ApprovalGroupSatisfaction.AnyOne;

        public bool IsVetoPower { get; set; } = false;

        public string? Notes { get; set; }

        public Guid ResourceCapabilityId { get; set; }
        public ResourceCapability Capability { get; set; } = null!;

        public Guid ApproverResourceId { get; set; }
        public Resource ApproverResource { get; set; } = null!;
    }
}