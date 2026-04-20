using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class ResourceCapability : BaseEntity
    {
        public CapabilityType Type { get; set; }

        public CapabilityScope Scope { get; set; }

        public DefaultApprovalPolicy DefaultApprovalPolicy { get; set; }
            = DefaultApprovalPolicy.AnyOwner;

        public int? GrantExpiryDays { get; set; }

        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        public Guid ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public ICollection<CapabilityApprovalRequirement> ApprovalRequirements { get; set; }
            = new List<CapabilityApprovalRequirement>();

        public ICollection<CapabilityGrant> Grants { get; set; }
            = new List<CapabilityGrant>();

        public ICollection<PolicyCondition> PolicyConditions { get; set; }
            = new List<PolicyCondition>();
    }
}