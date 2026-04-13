using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Helpers
{
    public static class CapabilityApprovalResolver
    {
        public static bool IsGrantSatisfied(
            CapabilityGrant grant,
            ResourceCapability capability,
            IEnumerable<ResourceOwnership> effectiveOwners)
        {
            var votes = grant.ApprovalVotes.ToList();
            var requirements = capability.ApprovalRequirements.ToList();

            // ── Veto check (applies regardless of path) ─────────────────────
            foreach (var rejectedVote in votes.Where(v => v.Decision == ApprovalDecision.Rejected))
            {
                if (requirements.Any())
                {
                    var req = requirements.FirstOrDefault(
                        r => r.ApproverResourceId == rejectedVote.ApproverResourceId);
                    if (req?.IsVetoPower == true) return false;
                }
                else
                {
                    // Default policy — any rejection blocks when AllOwners policy.
                    if (capability.DefaultApprovalPolicy == DefaultApprovalPolicy.AllOwners)
                        return false;
                }
            }

            // ── Explicit requirements path ───────────────────────────────────
            if (requirements.Any())
            {
                // All distinct groups must be satisfied.
                var groups = requirements.GroupBy(
                    r => r.ApproverGroupTag ?? r.Id.ToString());

                foreach (var group in groups)
                {
                    var groupReqs = group.ToList();
                    var satisfaction = groupReqs.First().GroupSatisfaction;

                    var groupVotes = votes.Where(v =>
                        groupReqs.Any(r => r.ApproverResourceId == v.ApproverResourceId))
                        .ToList();

                    bool groupSatisfied = satisfaction == ApprovalGroupSatisfaction.AnyOne
                        ? groupVotes.Any(v => v.Decision == ApprovalDecision.Approved)
                        : groupVotes.Any()
                          && groupVotes.All(v => v.Decision == ApprovalDecision.Approved);

                    if (!groupSatisfied) return false;
                }

                return true;
            }

            // ── Default policy path ──────────────────────────────────────────
            return capability.DefaultApprovalPolicy switch
            {
                DefaultApprovalPolicy.NoApprovalRequired
                    => true,
                DefaultApprovalPolicy.AnyOwner
                    => votes.Any(v => v.Decision == ApprovalDecision.Approved),
                DefaultApprovalPolicy.AllOwners
                    => votes.Any()
                       && votes.All(v => v.Decision == ApprovalDecision.Approved),
                _ => false
            };
        }

        public static bool IsGrantVetoed(
            CapabilityGrant grant,
            IReadOnlyList<CapabilityApprovalRequirement> requirements)
        {
            foreach (var vote in grant.ApprovalVotes.Where(
                v => v.Decision == ApprovalDecision.Rejected))
            {
                var req = requirements.FirstOrDefault(
                    r => r.ApproverResourceId == vote.ApproverResourceId);
                if (req?.IsVetoPower == true) return true;
            }
            return false;
        }
    }
}