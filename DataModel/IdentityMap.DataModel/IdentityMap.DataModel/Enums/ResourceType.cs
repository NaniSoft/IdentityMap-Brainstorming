namespace IdentityMap.DataModel.Enums
{
    public enum ResourceType
    {
        // ── Infrastructure ──────────────────────────────────────────────────
        Organization,
        Region,
        Office,
        Datacenter,
        PhysicalMachine,
        VirtualMachine,
        NetworkDevice,
        StorageDevice,
        CloudSubscription,
        CloudProvider,
        CloudRegion,
        OperatingSystem,

        // ── Application tier ────────────────────────────────────────────────
        BusinessApp,
        ServiceEndpoint,
        Queue,
        Container,
        Cluster,

        // ── Data tier ───────────────────────────────────────────────────────
        Database,       // A database instance (e.g. CustomerData on SQLSRV-PROD-01)
        Table,          // A table or view inside a database (e.g. dbo.Customers)
        DataStore,      // Generic data store — kept for backward compatibility
        File,           // Individual record / row with its own sensitivity level
        Folder,

        // ── Identity ────────────────────────────────────────────────────────
        Account,        // Human user account (AD or local)
        ServiceAccount, // Machine / application identity
        Group,          // Security group or distribution list
        Role            // Permission role (DB role, server role, app role)
    }
}
