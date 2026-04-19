namespace IdentityMap.DataModel.Enums
{
    public enum GrantStatus
    {
        // Existing
        PendingApproval,
        Active,
        Rejected,
        Revoked,
        Expired,

        // NEW ↓
        SuspendedPendingCertification,  // JML: Mover triggered certification; grant frozen
        SuspendedDueToLeaver,           // account deactivated; grant preserved for audit
        PendingProvisioningConfirmation // grant approved but provisioning not yet confirmed
    }
}