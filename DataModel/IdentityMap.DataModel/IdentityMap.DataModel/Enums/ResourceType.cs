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

        // NEW ↓
        NetworkZone,            // VLAN, subnet, security zone (for firewall rules)
        FirewallRule,           // network-level access control entry
        VpnProfile,             // VPN connection profile / tunnel definition

        // ── Application tier ────────────────────────────────────────────────
        BusinessApp,
        ServiceEndpoint,
        Queue,
        Container,
        Cluster,

        // NEW ↓
        Topic,                  // message topic (Kafka, SNS, Event Grid)
        K8sNamespace,           // Kubernetes namespace — scopes K8s RBAC
        Report,                 // BI / analytics report (Power BI, Tableau)

        // ── Data tier ───────────────────────────────────────────────────────
        Database,
        Table,
        DataStore,
        File,
        Folder,

        // NEW ↓
        DatabaseView,           // SQL VIEW — an AccessSurface into one or more tables
        StoredProcedure,        // executes with the definer's privilege set
        FileShare,              // UNC share / NFS export — entry point into a filesystem
        NetworkShare,           // alias for FileShare (Windows DFS / Samba)

        // ── Identity ────────────────────────────────────────────────────────
        Account,
        ServiceAccount,
        Group,
        Role,

        // NEW ↓
        LinuxUser,              // local /etc/passwd entry on a Linux host
        LinuxGroup,             // local /etc/group entry
        SudoersRule,            // a sudoers entry granting privilege escalation
        WindowsLocalUser,       // local SAM account on a Windows machine
        WindowsLocalGroup,      // local SAM group
        GroupPolicyObject,      // GPO — governs settings applied to OUs/computers/users

        // ── Credential / secret ─────────────────────────────────────────────
        // NEW ↓ (all are "actionable resources" that can authenticate)
        Certificate,            // X.509 certificate (client auth, code signing, TLS)
        ApiKey,                 // bearer key used by services/scripts
        OAuthClient,            // OAuth 2.0 client_id + client_secret pair
        Secret,                 // generic secret (vault entry, password, SSH key)
        SshKey,                 // SSH public/private key pair

        // ── Governance ──────────────────────────────────────────────────────
        // NEW ↓
        IdentityProvider,       // AD, Okta, Azure AD, LDAP directory
        PolicyObject            // abstract policy (ABAC policy, SELinux module, etc.)
    }
}
