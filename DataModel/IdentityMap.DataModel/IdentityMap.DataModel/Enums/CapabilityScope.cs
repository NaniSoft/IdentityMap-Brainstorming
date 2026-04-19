namespace IdentityMap.DataModel.Enums
{
    public enum CapabilityScope
    {
        // Existing
        SelfManagement,     // manage the resource itself (rename, reconfigure)
        ContentAccess,      // access the content the resource holds or serves

        // NEW ↓
        IdentityLifecycle,  // create/disable/unlock accounts within this resource's scope
        NetworkAccess,      // connect to a network segment or through a firewall rule
        Audit,              // read audit logs or trigger compliance reports
        Privileged,         // elevated / break-glass access (PAM-managed, time-limited)
        Configuration       // change settings, policies, or access-control rules
    }
}