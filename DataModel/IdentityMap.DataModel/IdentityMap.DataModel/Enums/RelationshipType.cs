namespace IdentityMap.DataModel.Enums
{
    public enum RelationshipType
    {
        // Existing
        HostedIn,           // child lives inside parent (table in database)
        BelongsTo,          // child is owned/administered by parent (domain)
        DependsOn,          // child requires parent to function (DB user ← server login)
        ReplicaOf,          // child replicates parent (read replica, DR copy)
        MemberOf,           // child is a member of a group-type parent
        UsesIdentity,       // child authenticates to something using parent's identity

        // NEW ↓
        Manages,            // parent operationally manages child (DBA manages database)
        SynchronizedFrom,   // child was imported / synced from parent system
                            // (SQL login synced from AD group via Windows Auth)
        AuthenticatesVia,   // child authenticates to parent (webapp → IdP)
        AuthorizesVia,      // child delegates authorization decisions to parent
                            // (K8s namespace → cluster RBAC policy)
        RunsOn,             // child process/app runs on parent host
        BackupOf,           // child is a backup or point-in-time snapshot of parent
        ProtectedBy         // child is guarded by parent (resource behind firewall zone)
    }
}