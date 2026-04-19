using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    /// <summary>
    /// A single certification decision within a certification campaign.
    /// A campaign is represented by a shared CampaignId across all records.
    ///
    /// Typical usage:
    ///   • Mover event: trigger a campaign for all grants held by the moved account.
    ///   • Leaver event: automatically revoke uncertified grants after deadline.
    ///   • Periodic review: annual access review across all high-sensitivity resources.
    /// </summary>
    public class IdentityCertification : BaseEntity
    {
        public Guid CampaignId { get; set; }                // groups records into a campaign
        public string CampaignName { get; set; } = string.Empty;

        public CertificationDecision Decision { get; set; } = CertificationDecision.Pending;
        public string? Comments { get; set; }

        public DateTime? DecidedAt { get; set; }
        public DateTime CertificationDeadline { get; set; }

        // What is being certified: could be a CapabilityGrant or a membership.
        public Guid? CapabilityGrantId { get; set; }
        public CapabilityGrant? CapabilityGrant { get; set; }

        public Guid? BusinessAppMembershipId { get; set; }
        public BusinessAppMembership? BusinessAppMembership { get; set; }

        // Who is certifying (the owner or data steward).
        public Guid ReviewerResourceId { get; set; }
        public Resource ReviewerResource { get; set; } = null!;

        // The account whose access is being reviewed.
        public Guid SubjectResourceId { get; set; }
        public Resource SubjectResource { get; set; } = null!;
    }
}
