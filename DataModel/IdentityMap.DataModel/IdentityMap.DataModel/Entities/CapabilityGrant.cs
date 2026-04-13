using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class CapabilityGrant : BaseEntity
    {
        public GrantStatus Status { get; set; } = GrantStatus.PendingApproval;

        public string? Justification { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string? RevocationReason { get; set; }

        public Guid ResourceCapabilityId { get; set; }
        public ResourceCapability ResourceCapability { get; set; } = null!;
        public Guid SubjectResourceId { get; set; }
        public Resource SubjectResource { get; set; } = null!;

        public ICollection<GrantApprovalVote> ApprovalVotes { get; set; }
            = new List<GrantApprovalVote>();
    }
}