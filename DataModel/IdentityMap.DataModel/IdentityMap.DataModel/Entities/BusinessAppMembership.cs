using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class BusinessAppMembership : BaseEntity
    {
        // Was: public string Role { get; set; } = "Member";
        public MembershipRole Role { get; set; } = MembershipRole.Member;

        public bool IsActive { get; set; } = true;

        // NEW: When this membership was deactivated (for JML audit trail).
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivationReason { get; set; }   // "Leaver", "RoleChange", "Revoked"

        public Guid BusinessAppResourceId { get; set; }
        public Resource BusinessAppResource { get; set; } = null!;

        public Guid MemberResourceId { get; set; }
        public Resource MemberResource { get; set; } = null!;
    }
}