using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class GrantApprovalVote : BaseEntity
    {
        public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

        public string? Comments { get; set; }

        public DateTime? DecidedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? ApprovalDeadline { get; set; }

        public Guid CapabilityGrantId { get; set; }
        public CapabilityGrant CapabilityGrant { get; set; } = null!;
        public Guid? CapabilityApprovalRequirementId { get; set; }
        public CapabilityApprovalRequirement? ApprovalRequirement { get; set; }
        public Guid? ResourceOwnershipId { get; set; }
        public ResourceOwnership? ResourceOwnership { get; set; }
        public Guid ApproverResourceId { get; set; }
        public Resource ApproverResource { get; set; } = null!;
        public Guid? ResolvedHumanAccountId { get; set; }
        public Resource? ResolvedHumanAccount { get; set; }
    }
}