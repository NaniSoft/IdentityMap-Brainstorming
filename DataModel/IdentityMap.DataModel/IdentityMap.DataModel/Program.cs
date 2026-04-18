using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using IdentityMap.DataModel.Helpers;
using Spectre.Console;

namespace IdentityMap.DataModel
{
    internal class Program
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // WELL-KNOWN IDs  — fixed so the scenario is repeatable and readable in tests
        // ═══════════════════════════════════════════════════════════════════════════

        // ── Identity Provider ──────────────────────────────────────────────────────
        static readonly Guid Id_AD = new("10000000-0000-0000-0000-000000000001");

        // ── Human User Accounts (10 users) ────────────────────────────────────────
        //    Primary low-privilege AD accounts used for day-to-day work.
        static readonly Guid Id_Alice = new("10000000-0000-0000-0000-000000000010"); // Senior Developer
        static readonly Guid Id_Bob = new("10000000-0000-0000-0000-000000000011"); // Data Analyst
        static readonly Guid Id_Carol = new("10000000-0000-0000-0000-000000000012"); // DBA
        static readonly Guid Id_Dave = new("10000000-0000-0000-0000-000000000013"); // App Admin
        static readonly Guid Id_Eve = new("10000000-0000-0000-0000-000000000014"); // Security Officer
        static readonly Guid Id_Frank = new("10000000-0000-0000-0000-000000000015"); // Developer
        static readonly Guid Id_Grace = new("10000000-0000-0000-0000-000000000016"); // Business Analyst
        static readonly Guid Id_Harry = new("10000000-0000-0000-0000-000000000017"); // IT Manager / Executive
        static readonly Guid Id_Ivan = new("10000000-0000-0000-0000-000000000018"); // DevOps Engineer
        static readonly Guid Id_Judy = new("10000000-0000-0000-0000-000000000019"); // Internal Auditor

        // ── Elevated / Secondary AD Accounts  (same humans, separate privileged identity)
        //    Real-world pattern: carol-adm@ is Carol's admin account used only for DBA work.
        static readonly Guid Id_Alice_Adm = new("10000000-0000-0000-0000-000000000020");
        static readonly Guid Id_Carol_Adm = new("10000000-0000-0000-0000-000000000021");
        static readonly Guid Id_Dave_Adm = new("10000000-0000-0000-0000-000000000022");

        // ── AD Service Accounts  (automated / machine identities in AD) ───────────
        static readonly Guid Id_SvcAcct_WebApp = new("10000000-0000-0000-0000-000000000030");
        static readonly Guid Id_SvcAcct_Reporting = new("10000000-0000-0000-0000-000000000031");
        static readonly Guid Id_SvcAcct_HrApp = new("10000000-0000-0000-0000-000000000032");
        static readonly Guid Id_SvcAcct_Orders = new("10000000-0000-0000-0000-000000000033");

        // ── AD Groups ─────────────────────────────────────────────────────────────
        //    Id_Grp_DataAccess wraps Id_Grp_DbReaders — demonstrating group-of-groups.
        static readonly Guid Id_Grp_WebAppUsers = new("10000000-0000-0000-0000-000000000040");
        static readonly Guid Id_Grp_DbReaders = new("10000000-0000-0000-0000-000000000041");
        static readonly Guid Id_Grp_DbAdmins = new("10000000-0000-0000-0000-000000000042");
        static readonly Guid Id_Grp_HrPortalUsers = new("10000000-0000-0000-0000-000000000043");
        static readonly Guid Id_Grp_Auditors = new("10000000-0000-0000-0000-000000000044");
        static readonly Guid Id_Grp_DataAccess = new("10000000-0000-0000-0000-000000000045"); // GROUP-OF-GROUPS
        static readonly Guid Id_Grp_Executives = new("10000000-0000-0000-0000-000000000046");
        static readonly Guid Id_Grp_DevOps = new("10000000-0000-0000-0000-000000000047");

        // ── SQL Server Instances ───────────────────────────────────────────────────
        static readonly Guid Id_SqlSrv_Prod01 = new("20000000-0000-0000-0000-000000000001");
        static readonly Guid Id_SqlSrv_Prod02 = new("20000000-0000-0000-0000-000000000002");

        // ── Databases ─────────────────────────────────────────────────────────────
        //    master databases are explicit resources — server-level logins/roles live here.
        static readonly Guid Id_Db_Master_Prod01 = new("20000000-0000-0000-0000-000000000010");
        static readonly Guid Id_Db_Master_Prod02 = new("20000000-0000-0000-0000-000000000011");
        static readonly Guid Id_Db_CustomerData = new("20000000-0000-0000-0000-000000000012");
        static readonly Guid Id_Db_Orders = new("20000000-0000-0000-0000-000000000013");
        static readonly Guid Id_Db_CountryRef = new("20000000-0000-0000-0000-000000000014");
        static readonly Guid Id_Db_HRData = new("20000000-0000-0000-0000-000000000015");
        static readonly Guid Id_Db_AuditLog = new("20000000-0000-0000-0000-000000000016");

        // ── Tables  (DataStore — actual tabular data surfaces) ─────────────────────
        static readonly Guid Id_Tbl_Customers = new("20000000-0000-0000-0000-000000000020"); // Restricted / PII
        static readonly Guid Id_Tbl_Payments = new("20000000-0000-0000-0000-000000000021"); // TopSecret / financial
        static readonly Guid Id_Tbl_Addresses = new("20000000-0000-0000-0000-000000000022"); // Confidential
        static readonly Guid Id_Tbl_Orders = new("20000000-0000-0000-0000-000000000023"); // Confidential
        static readonly Guid Id_Tbl_OrderItems = new("20000000-0000-0000-0000-000000000024"); // Internal
        static readonly Guid Id_Tbl_Countries = new("20000000-0000-0000-0000-000000000025"); // None / public
        static readonly Guid Id_Tbl_Employees = new("20000000-0000-0000-0000-000000000026"); // TopSecret / HR
        static readonly Guid Id_Tbl_Salaries = new("20000000-0000-0000-0000-000000000027"); // TopSecret / salary
        static readonly Guid Id_Tbl_AccessLog = new("20000000-0000-0000-0000-000000000028"); // Restricted / audit

        // ── Sensitive Rows  (File — individual records with elevated sensitivity) ──
        //    Real-world: a row in dbo.Customers containing SSN should be TopSecret even
        //    if the table is only Restricted at the aggregate level.
        static readonly Guid Id_Row_CustomerSSN = new("20000000-0000-0000-0000-000000000030");
        static readonly Guid Id_Row_SalaryRecord = new("20000000-0000-0000-0000-000000000031");

        // ── Server Roles  (live in master DB — server-level permission sets) ───────
        static readonly Guid Id_SrvRole_Sysadmin = new("20000000-0000-0000-0000-000000000040");
        static readonly Guid Id_SrvRole_DbCreator = new("20000000-0000-0000-0000-000000000041");
        static readonly Guid Id_SrvRole_SecurityAdmin = new("20000000-0000-0000-0000-000000000042");

        // ── Server Logins  (authenticate at server level, live in master DB) ───────
        //    SA              — native SQL sa login (sysadmin)
        //    WebApp          — AD-integrated, delegated from svc-webapp@corp.com
        //    Reporting       — AD group login: AD\DB-Readers mapped directly
        //    HrApp           — AD-integrated for HR portal
        //    CountryRef      — native SQL, read-only, no AD linkage
        //    CarolDba        — AD-backed from carol-adm@, has SecurityAdmin server role
        //    Orders          — native SQL login for order processing service
        static readonly Guid Id_Login_SA = new("20000000-0000-0000-0000-000000000050");
        static readonly Guid Id_Login_WebApp = new("20000000-0000-0000-0000-000000000051");
        static readonly Guid Id_Login_Reporting = new("20000000-0000-0000-0000-000000000052");
        static readonly Guid Id_Login_HrApp = new("20000000-0000-0000-0000-000000000053");
        static readonly Guid Id_Login_CountryRef = new("20000000-0000-0000-0000-000000000054");
        static readonly Guid Id_Login_CarolDba = new("20000000-0000-0000-0000-000000000055");
        static readonly Guid Id_Login_Orders = new("20000000-0000-0000-0000-000000000056");

        // ── Database Roles  (scoped per database, group database-level permissions) ─
        static readonly Guid Id_DbRole_CustDataReader = new("20000000-0000-0000-0000-000000000060");
        static readonly Guid Id_DbRole_CustDataWriter = new("20000000-0000-0000-0000-000000000061");
        static readonly Guid Id_DbRole_OrdersReadWrite = new("20000000-0000-0000-0000-000000000062");
        static readonly Guid Id_DbRole_HrReadOnly = new("20000000-0000-0000-0000-000000000063");
        static readonly Guid Id_DbRole_HrAdmin = new("20000000-0000-0000-0000-000000000064");

        // ── Database Users  (per-database principal, maps to a server login) ────────
        static readonly Guid Id_DbUser_WebApp = new("20000000-0000-0000-0000-000000000070");
        static readonly Guid Id_DbUser_Reporting = new("20000000-0000-0000-0000-000000000071");
        static readonly Guid Id_DbUser_HrApp = new("20000000-0000-0000-0000-000000000072");
        static readonly Guid Id_DbUser_CountryRef = new("20000000-0000-0000-0000-000000000073");
        static readonly Guid Id_DbUser_Orders = new("20000000-0000-0000-0000-000000000074");
        static readonly Guid Id_DbUser_AuditUser = new("20000000-0000-0000-0000-000000000075");

        // ── Web Applications ──────────────────────────────────────────────────────
        static readonly Guid Id_WebApp_CustomerPortal = new("30000000-0000-0000-0000-000000000001");
        static readonly Guid Id_WebApp_HrPortal = new("30000000-0000-0000-0000-000000000002");

        // ── Service Endpoints ─────────────────────────────────────────────────────
        static readonly Guid Id_Ep_GetCustomers = new("30000000-0000-0000-0000-000000000010");
        static readonly Guid Id_Ep_GetCustomerById = new("30000000-0000-0000-0000-000000000011");
        static readonly Guid Id_Ep_PostOrder = new("30000000-0000-0000-0000-000000000012");
        static readonly Guid Id_Ep_GetEmployees = new("30000000-0000-0000-0000-000000000013");
        static readonly Guid Id_Ep_GetSalaries = new("30000000-0000-0000-0000-000000000014");

        // ── Attribute Definition IDs ───────────────────────────────────────────────
        static readonly Guid AttrDef_LinkedAdIdentity = new("40000000-0000-0000-0000-000000000001");
        static readonly Guid AttrDef_LinkedAdGroup = new("40000000-0000-0000-0000-000000000002");
        static readonly Guid AttrDef_LinkedDbUser = new("40000000-0000-0000-0000-000000000003");
        static readonly Guid AttrDef_ParentTable = new("40000000-0000-0000-0000-000000000004");

        // ── Capability IDs ─────────────────────────────────────────────────────────
        // Endpoint capabilities
        static readonly Guid Cap_Ep_GetCustomers_Exec = new("50000000-0000-0000-0000-000000000001");
        static readonly Guid Cap_Ep_GetCustomerById_Exec = new("50000000-0000-0000-0000-000000000002");
        static readonly Guid Cap_Ep_PostOrder_Exec = new("50000000-0000-0000-0000-000000000003");
        static readonly Guid Cap_Ep_PostOrder_Write = new("50000000-0000-0000-0000-000000000004");
        static readonly Guid Cap_Ep_GetEmployees_Exec = new("50000000-0000-0000-0000-000000000005");
        static readonly Guid Cap_Ep_GetSalaries_Exec = new("50000000-0000-0000-0000-000000000006");

        // Table-level capabilities  (granular — the core of SQL IAM governance)
        static readonly Guid Cap_Tbl_Customers_Read = new("50000000-0000-0000-0000-000000000010");
        static readonly Guid Cap_Tbl_Customers_Write = new("50000000-0000-0000-0000-000000000011");
        static readonly Guid Cap_Tbl_Customers_Delete = new("50000000-0000-0000-0000-000000000012");
        static readonly Guid Cap_Tbl_Payments_Read = new("50000000-0000-0000-0000-000000000013");
        static readonly Guid Cap_Tbl_Addresses_Read = new("50000000-0000-0000-0000-000000000014");
        static readonly Guid Cap_Tbl_Addresses_Write = new("50000000-0000-0000-0000-000000000015");
        static readonly Guid Cap_Tbl_Orders_Read = new("50000000-0000-0000-0000-000000000016");
        static readonly Guid Cap_Tbl_Orders_Write = new("50000000-0000-0000-0000-000000000017");
        static readonly Guid Cap_Tbl_OrderItems_Read = new("50000000-0000-0000-0000-000000000018");
        static readonly Guid Cap_Tbl_OrderItems_Write = new("50000000-0000-0000-0000-000000000019");
        static readonly Guid Cap_Tbl_Countries_Read = new("50000000-0000-0000-0000-000000000020");
        static readonly Guid Cap_Tbl_Employees_Read = new("50000000-0000-0000-0000-000000000021");
        static readonly Guid Cap_Tbl_Employees_Write = new("50000000-0000-0000-0000-000000000022");
        static readonly Guid Cap_Tbl_Salaries_Read = new("50000000-0000-0000-0000-000000000023");
        static readonly Guid Cap_Tbl_AccessLog_Read = new("50000000-0000-0000-0000-000000000024");
        static readonly Guid Cap_Tbl_AccessLog_Write = new("50000000-0000-0000-0000-000000000025");

        // Database-level capabilities
        static readonly Guid Cap_Db_CustomerData_Administer = new("50000000-0000-0000-0000-000000000030");
        static readonly Guid Cap_Db_HRData_Administer = new("50000000-0000-0000-0000-000000000031");

        // Server role capability
        static readonly Guid Cap_SrvRole_Sysadmin_Administer = new("50000000-0000-0000-0000-000000000035");

        // ── Grant IDs ─────────────────────────────────────────────────────────────
        // Endpoint access grants
        static readonly Guid Grant_WebAppUsers_GetCustomers = new("60000000-0000-0000-0000-000000000001");
        static readonly Guid Grant_WebAppUsers_GetCustomerById = new("60000000-0000-0000-0000-000000000002");
        static readonly Guid Grant_DevOps_PostOrder = new("60000000-0000-0000-0000-000000000003");
        static readonly Guid Grant_HrPortalUsers_GetEmployees = new("60000000-0000-0000-0000-000000000004");
        static readonly Guid Grant_Auditors_GetEmployees = new("60000000-0000-0000-0000-000000000005");
        static readonly Guid Grant_Executives_GetSalaries = new("60000000-0000-0000-0000-000000000006");

        // Table-level grants (database user/role → table capability)
        static readonly Guid Grant_DbUser_WebApp_Cust_Read = new("60000000-0000-0000-0000-000000000010");
        static readonly Guid Grant_DbUser_WebApp_Addr_Read = new("60000000-0000-0000-0000-000000000011");
        static readonly Guid Grant_DbUser_WebApp_Pay_Read = new("60000000-0000-0000-0000-000000000012");
        static readonly Guid Grant_DbRole_CustReader_Cust_Read = new("60000000-0000-0000-0000-000000000013");
        static readonly Guid Grant_DbUser_Reporting_Cust_Read = new("60000000-0000-0000-0000-000000000014");
        static readonly Guid Grant_DbUser_CountryRef_Countries = new("60000000-0000-0000-0000-000000000015");
        static readonly Guid Grant_DbUser_Orders_Orders_Read = new("60000000-0000-0000-0000-000000000016");
        static readonly Guid Grant_DbUser_Orders_Orders_Write = new("60000000-0000-0000-0000-000000000017");
        static readonly Guid Grant_DbUser_Orders_Items_Read = new("60000000-0000-0000-0000-000000000018");
        static readonly Guid Grant_DbUser_Orders_Items_Write = new("60000000-0000-0000-0000-000000000019");
        static readonly Guid Grant_DbUser_HrApp_Emp_Read = new("60000000-0000-0000-0000-000000000020");
        static readonly Guid Grant_DbUser_HrApp_Emp_Write = new("60000000-0000-0000-0000-000000000021");
        static readonly Guid Grant_DbUser_HrApp_Sal_Read = new("60000000-0000-0000-0000-000000000022");
        static readonly Guid Grant_DbUser_Audit_Emp_Read = new("60000000-0000-0000-0000-000000000023");
        static readonly Guid Grant_DbUser_Audit_Log_Read = new("60000000-0000-0000-0000-000000000024");
        
        // BYPASS PATH: AD\DB-Readers group has direct read on Customers table
        static readonly Guid Grant_Grp_DbReaders_Cust_Read = new("60000000-0000-0000-0000-000000000025");
        
        // DB admin
        static readonly Guid Grant_Carol_Administer_CustData = new("60000000-0000-0000-0000-000000000026");
        static readonly Guid Grant_Carol_Administer_HRData = new("60000000-0000-0000-0000-000000000027");
        
        // Server role grants
        static readonly Guid Grant_Login_SA_Sysadmin = new("60000000-0000-0000-0000-000000000030");

        // ── Content Binding IDs ───────────────────────────────────────────────────
        static readonly Guid Binding_GetCustomers_Customers = new("70000000-0000-0000-0000-000000000001");
        static readonly Guid Binding_GetCustomers_Addresses = new("70000000-0000-0000-0000-000000000002");
        static readonly Guid Binding_GetCustomers_Countries = new("70000000-0000-0000-0000-000000000003");
        static readonly Guid Binding_GetCustomerById_Customers = new("70000000-0000-0000-0000-000000000004");
        static readonly Guid Binding_GetCustomerById_Payments = new("70000000-0000-0000-0000-000000000005");
        static readonly Guid Binding_PostOrder_Orders = new("70000000-0000-0000-0000-000000000006");
        static readonly Guid Binding_PostOrder_OrderItems = new("70000000-0000-0000-0000-000000000007");
        static readonly Guid Binding_GetEmployees_Employees = new("70000000-0000-0000-0000-000000000008");
        static readonly Guid Binding_GetSalaries_Salaries = new("70000000-0000-0000-0000-000000000009");
        
        // Row → Table bindings  (sensitivity bubbles from row up to the containing table)
        static readonly Guid Binding_Tbl_Customers_Row_SSN = new("70000000-0000-0000-0000-000000000010");
        static readonly Guid Binding_Tbl_Salaries_Row_Salary = new("70000000-0000-0000-0000-000000000011");

        static void Main(string[] args)
        {
            AnsiConsole.MarkupLine("[bold cyan]╔══════════════════════════════════════════════════════════╗[/]");
            AnsiConsole.MarkupLine("[bold cyan]║  IdentityMap — Enterprise IAM Governance Demo           ║[/]");
            AnsiConsole.MarkupLine("[bold cyan]╚══════════════════════════════════════════════════════════╝[/]");

            var ctx = new AccessGraphContext();

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 1 — ACTIVE DIRECTORY: IDP, USERS, ADMIN ACCOUNTS, GROUPS
            // ─────────────────────────────────────────────────────────────────────

            var ad = new Resource
            {
                Id = Id_AD,
                Name = "corp.AD",
                Type = ResourceType.BusinessApp,
                Description = "Corporate Active Directory. Authoritative identity provider for all human " +
                              "and service accounts, plus all AD security groups.",
                Status = "Active"
            };

            // ── 10 Human Accounts ─────────────────────────────────────────────────
            var alice = MakeAccount(Id_Alice, "alice@corp.com", "Senior Developer — CustomerPortal team.");
            var bob = MakeAccount(Id_Bob, "bob@corp.com", "Data Analyst — reads customer reports.");
            var carol = MakeAccount(Id_Carol, "carol@corp.com", "DBA — day-to-day login; uses carol-adm for admin tasks.");
            var dave = MakeAccount(Id_Dave, "dave@corp.com", "App Admin — manages CustomerPortal deployments.");
            var eve = MakeAccount(Id_Eve, "eve@corp.com", "Security Officer — reviews audit logs and policy.");
            var frank = MakeAccount(Id_Frank, "frank@corp.com", "Developer — CustomerPortal feature work.");
            var grace = MakeAccount(Id_Grace, "grace@corp.com", "Business Analyst — reads customer data for reporting.");
            var harry = MakeAccount(Id_Harry, "harry@corp.com", "IT Manager / Executive — requires salary visibility.");
            var ivan = MakeAccount(Id_Ivan, "ivan@corp.com", "DevOps — manages order-processing pipeline.");
            var judy = MakeAccount(Id_Judy, "judy@corp.com", "Internal Auditor — read-only access to HR and audit.");

            // ── Elevated / Secondary AD Accounts  (same person, different AD principal)
            var alice_adm = new Resource
            {
                Id = Id_Alice_Adm,
                Name = "alice-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Alice's privileged AD account used only for release-approval workflows.",
                Status = "Active"
            };
            var carol_adm = new Resource
            {
                Id = Id_Carol_Adm,
                Name = "carol-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Carol's DBA admin account. Member of AD\\DB-Admins. " +
                              "Never used for browsing — only for DBA operations.",
                Status = "Active"
            };
            var dave_adm = new Resource
            {
                Id = Id_Dave_Adm,
                Name = "dave-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Dave's admin account. Member of AD\\DB-Admins for deployment scripts.",
                Status = "Active"
            };

            // ── AD Service Accounts ────────────────────────────────────────────────
            var svc_webapp = new Resource
            {
                Id = Id_SvcAcct_WebApp,
                Name = "svc-webapp@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for CustomerPortal runtime. " +
                              "Delegated to SQL login CORP\\svc-webapp on SQLSRV-PROD-01.",
                ContentNature = ContentNature.Dynamic,
                Status = "Active"
            };
            var svc_reporting = new Resource
            {
                Id = Id_SvcAcct_Reporting,
                Name = "svc-reporting@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for the reporting service. " +
                              "Mapped to AD\\DB-Readers group for data warehouse queries.",
                Status = "Active"
            };
            var svc_hrapp = new Resource
            {
                Id = Id_SvcAcct_HrApp,
                Name = "svc-hrapp@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for HRPortal. " +
                              "Delegated to SQL login CORP\\svc-hrapp on SQLSRV-PROD-02.",
                Status = "Active"
            };
            var svc_orders = new Resource
            {
                Id = Id_SvcAcct_Orders,
                Name = "svc-orders@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for the order-processing microservice. " +
                              "Delegated to native SQL login svc-orders on SQLSRV-PROD-01.",
                Status = "Active"
            };

            // ── AD Security Groups ─────────────────────────────────────────────────
            var grp_webAppUsers = MakeGroup(Id_Grp_WebAppUsers, "AD\\WebApp-Users",
                "Members may call CustomerPortal endpoints. Gateway for customer data.");
            var grp_dbReaders = MakeGroup(Id_Grp_DbReaders, "AD\\DB-Readers",
                "Direct DB read group. Mapped to SQL db-readers-role on SQLSRV-PROD-01. " +
                "BYPASS PATH: members can query CustomerData without going through the portal.");
            var grp_dbAdmins = MakeGroup(Id_Grp_DbAdmins, "AD\\DB-Admins",
                "DBA administrators. Can administer CustomerData and HRData databases.");
            var grp_hrPortalUsers = MakeGroup(Id_Grp_HrPortalUsers, "AD\\HRPortal-Users",
                "Members may call HRPortal employee endpoints. No salary access.");
            var grp_auditors = MakeGroup(Id_Grp_Auditors, "AD\\Auditors",
                "Internal auditors. Read-only on HR employees and audit logs.");
            var grp_dataAccess = MakeGroup(Id_Grp_DataAccess, "AD\\Data-Access",
                "GROUP-OF-GROUPS: wraps AD\\DB-Readers. " +
                "Effectively grants DB read to all members of child groups.");
            var grp_executives = MakeGroup(Id_Grp_Executives, "AD\\Executives",
                "Executive leadership. Can view salary data via HRPortal.");
            var grp_devops = MakeGroup(Id_Grp_DevOps, "AD\\DevOps",
                "DevOps engineers. May post orders via the order-processing endpoint.");

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 2 — SQL INFRASTRUCTURE: SERVERS, MASTER DBs, DATABASES, TABLES, ROWS
            // ─────────────────────────────────────────────────────────────────────

            var sqlSrv_prod01 = new Resource
            {
                Id = Id_SqlSrv_Prod01,
                Name = "SQLSRV-PROD-01",
                Type = ResourceType.VirtualMachine,
                Description = "Primary SQL Server instance. Hosts CustomerData, Orders, CountryRef. " +
                              "AD-integrated via Windows Authentication.",
                Status = "Active"
            };
            var sqlSrv_prod02 = new Resource
            {
                Id = Id_SqlSrv_Prod02,
                Name = "SQLSRV-PROD-02",
                Type = ResourceType.VirtualMachine,
                Description = "Secondary SQL Server. Hosts HRData and AuditLog. " +
                              "Stricter firewall rules — no public subnet access.",
                Status = "Active"
            };

            // master databases — server-level logins and roles are hosted here
            var db_master_prod01 = MakeDatabase(Id_Db_Master_Prod01, "master (PROD-01)",
                "System database on SQLSRV-PROD-01. All server-level logins and server roles live here.",
                SensitivityClassification.Confidential);
            var db_master_prod02 = MakeDatabase(Id_Db_Master_Prod02, "master (PROD-02)",
                "System database on SQLSRV-PROD-02. All server-level logins and server roles live here.",
                SensitivityClassification.Confidential);

            // User databases
            var db_customerData = MakeDatabase(Id_Db_CustomerData, "CustomerData",
                "Customer PII database. Normalised schema: Customers, Payments, Addresses. " +
                "Contains highly sensitive personal and financial data.",
                SensitivityClassification.Restricted);
            var db_orders = MakeDatabase(Id_Db_Orders, "OrdersDB",
                "Transactional order processing database. Orders and OrderItems tables.",
                SensitivityClassification.Confidential);
            var db_countryRef = MakeDatabase(Id_Db_CountryRef, "CountryRef",
                "ISO country lookup database. Public reference data only. " +
                "Used by CustomerPortal to resolve country_code → country_name.",
                SensitivityClassification.None);
            var db_hrData = MakeDatabase(Id_Db_HRData, "HRData",
                "Human resources database on SQLSRV-PROD-02. Contains employee PII and salary data. " +
                "Highest sensitivity — segregated server.",
                SensitivityClassification.TopSecret);
            var db_auditLog = MakeDatabase(Id_Db_AuditLog, "AuditLog",
                "Centralised audit logging database. Read by auditors and SIEM. " +
                "Tamper-evident — no UPDATE/DELETE grants issued.",
                SensitivityClassification.Restricted);

            // ── Tables (DataStore) ────────────────────────────────────────────────
            var tbl_customers = MakeTable(Id_Tbl_Customers, "dbo.Customers",
                "Core customer table: id, name, email, phone, country_code, created_at. " +
                "PII — name, email, phone are personal data under GDPR.",
                SensitivityClassification.Restricted);
            var tbl_payments = MakeTable(Id_Tbl_Payments, "dbo.Payments",
                "Payment instrument table: id, customer_id, card_last4, card_token, billing_zip. " +
                "Financial PCI-DSS data. TopSecret — only svc-webapp with explicit grant may read.",
                SensitivityClassification.TopSecret);
            var tbl_addresses = MakeTable(Id_Tbl_Addresses, "dbo.Addresses",
                "Customer address table: id, customer_id, line1, line2, city, postcode, country_code. " +
                "Confidential — used for shipping; disclosed only to fulfilment processes.",
                SensitivityClassification.Confidential);
            var tbl_orders = MakeTable(Id_Tbl_Orders, "dbo.Orders",
                "Order header table: id, customer_id, placed_at, status, total_amount.",
                SensitivityClassification.Confidential);
            var tbl_orderItems = MakeTable(Id_Tbl_OrderItems, "dbo.OrderItems",
                "Order line items: id, order_id, product_id, qty, unit_price.",
                SensitivityClassification.Internal);
            var tbl_countries = MakeTable(Id_Tbl_Countries, "dbo.Countries",
                "ISO 3166 country lookup: country_code CHAR(2), country_name NVARCHAR(100). " +
                "Public reference data — no sensitivity.",
                SensitivityClassification.None);
            var tbl_employees = MakeTable(Id_Tbl_Employees, "hr.Employees",
                "Employee master: id, name, email, job_title, department, manager_id, hire_date. " +
                "TopSecret HR PII.",
                SensitivityClassification.TopSecret);
            var tbl_salaries = MakeTable(Id_Tbl_Salaries, "hr.Salaries",
                "Salary records: employee_id, base_salary, bonus, effective_date. " +
                "TopSecret — most sensitive table in the estate.",
                SensitivityClassification.TopSecret);
            var tbl_accessLog = MakeTable(Id_Tbl_AccessLog, "audit.AccessLog",
                "Security access events: id, principal, resource, action, timestamp, outcome. " +
                "Restricted — auditors and SIEM only.",
                SensitivityClassification.Restricted);

            // ── Sensitive Rows (File — individual records with elevated sensitivity) ─
            //    These rows demonstrate that a single record can carry higher sensitivity
            //    than its parent table aggregate, and that this propagates upward to endpoints.
            var row_customerSSN = new Resource
            {
                Id = Id_Row_CustomerSSN,
                Name = "dbo.Customers#Row:SSN_Field",
                Type = ResourceType.File,
                Description = "Row in dbo.Customers for a customer who provided their SSN during KYC. " +
                              "The SSN column is populated only for this cohort. " +
                              "Sensitivity is TopSecret — higher than the parent table (Restricted).",
                ContentAccessModel = ContentAccessModel.ResourceIsContent,
                ContentNature = ContentNature.Static,
                ContentSchemaDescription = "{ customer_id, ssn_encrypted, ssn_last4, kyc_date }",
                Sensitivity = SensitivityClassification.TopSecret,
                Status = "Active"
            };
            var row_salaryRecord = new Resource
            {
                Id = Id_Row_SalaryRecord,
                Name = "hr.Salaries#Row:Executive",
                Type = ResourceType.File,
                Description = "Executive compensation record in hr.Salaries. " +
                              "Salary + LTIP + bonus breakdown. TopSecret. " +
                              "Only CFO (harry) and payroll admin are authorised consumers.",
                ContentAccessModel = ContentAccessModel.ResourceIsContent,
                ContentNature = ContentNature.Static,
                ContentSchemaDescription = "{ employee_id, base_salary, bonus, ltip_units, effective_date }",
                Sensitivity = SensitivityClassification.TopSecret,
                Status = "Active"
            };

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 3 — SQL IAM LAYER: SERVER ROLES, SERVER LOGINS, DB ROLES, DB USERS
            // ─────────────────────────────────────────────────────────────────────

            // Server Roles  (live in master DB — server-wide permission sets)
            var srvRole_sysadmin = MakeRole(Id_SrvRole_Sysadmin, "SQL\\sysadmin",
                "Fixed server role. Full control over the SQL Server instance. " +
                "Assigned to: SQL\\sa (native), and implicitly to carol-dba via securityadmin chain.");
            var srvRole_dbCreator = MakeRole(Id_SrvRole_DbCreator, "SQL\\dbcreator",
                "Fixed server role. Can create, alter, and drop databases. " +
                "Assigned to carol-dba for managed schema rollouts.");
            var srvRole_securityAdmin = MakeRole(Id_SrvRole_SecurityAdmin, "SQL\\securityadmin",
                "Fixed server role. Can manage server logins and their grants. " +
                "Assigned to carol-dba for provisioning service account logins.");

            // Server Logins  (hosted in master DB — authenticate at the server level)
            var login_sa = new Resource
            {
                Id = Id_Login_SA,
                Name = "SQL\\sa",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL Server administrator login. Not AD-integrated. " +
                              "Should be disabled in production — modelled here for completeness.",
                Status = "Disabled"
            };
            var login_webapp = new Resource
            {
                Id = Id_Login_WebApp,
                Name = "CORP\\svc-webapp",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login for CustomerPortal. " +
                              "Delegates from AD service account svc-webapp@corp.com. " +
                              "Has db-users in CustomerData and OrdersDB.",
                Status = "Active"
            };
            var login_reporting = new Resource
            {
                Id = Id_Login_Reporting,
                Name = "CORP\\DB-Readers",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login mapped from AD group AD\\DB-Readers. " +
                              "Any member of that AD group authenticates under this login. " +
                              "Grants db_datareader on CustomerData. BYPASS PATH.",
                Status = "Active"
            };
            var login_hrapp = new Resource
            {
                Id = Id_Login_HrApp,
                Name = "CORP\\svc-hrapp",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login for HRPortal on SQLSRV-PROD-02. " +
                              "Delegates from svc-hrapp@corp.com.",
                Status = "Active"
            };
            var login_countryRef = new Resource
            {
                Id = Id_Login_CountryRef,
                Name = "SQL\\svc-countryref-ro",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL login. Read-only access to CountryRef.dbo.Countries only. " +
                              "No AD mapping — password-rotated monthly by vault.",
                Status = "Active"
            };
            var login_carolDba = new Resource
            {
                Id = Id_Login_CarolDba,
                Name = "CORP\\carol-adm",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login for carol-adm@corp.com. " +
                              "Has SecurityAdmin and DbCreator server roles. " +
                              "Also mapped as db_owner in CustomerData and HRData for schema management.",
                Status = "Active"
            };
            var login_orders = new Resource
            {
                Id = Id_Login_Orders,
                Name = "SQL\\svc-orders",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL login for the order-processing service. " +
                              "Read/write on OrdersDB only. No access to CustomerData or HRData.",
                Status = "Active"
            };

            // Database Roles  (scoped per database)
            var dbRole_custDataReader = MakeRole(Id_DbRole_CustDataReader, "CustomerData\\db-readers-role",
                "Custom DB role in CustomerData. Members inherit READ on dbo.Customers and dbo.Addresses. " +
                "Mapped to AD\\DB-Readers via login_reporting (Windows Auth group login).");
            var dbRole_custDataWriter = MakeRole(Id_DbRole_CustDataWriter, "CustomerData\\db-writers-role",
                "Custom DB role in CustomerData. Members may INSERT/UPDATE dbo.Customers. " +
                "Reserved for future batch import service — no active members yet.");
            var dbRole_ordersReadWrite = MakeRole(Id_DbRole_OrdersReadWrite, "OrdersDB\\orders-rw-role",
                "Custom DB role in OrdersDB. READ and WRITE on Orders and OrderItems tables.");
            var dbRole_hrReadOnly = MakeRole(Id_DbRole_HrReadOnly, "HRData\\hr-readonly-role",
                "Custom DB role in HRData. READ on hr.Employees only. No salary access.");
            var dbRole_hrAdmin = MakeRole(Id_DbRole_HrAdmin, "HRData\\hr-admin-role",
                "Custom DB role in HRData. Full READ/WRITE on hr.Employees and hr.Salaries. " +
                "Reserved for HR system service account only.");

            // Database Users  (per-database principals, each maps to a server login)
            var dbUser_webapp = MakeDbUser(Id_DbUser_WebApp, "CustomerData\\db_user:svc-webapp",
                "Database user in CustomerData for CORP\\svc-webapp login. " +
                "Has explicit GRANT on Customers(SELECT), Addresses(SELECT), Payments(SELECT).");
            var dbUser_reporting = MakeDbUser(Id_DbUser_Reporting, "CustomerData\\db_user:DB-Readers",
                "Database user in CustomerData for the AD group login CORP\\DB-Readers. " +
                "Member of db-readers-role, which grants SELECT on Customers.");
            var dbUser_hrapp = MakeDbUser(Id_DbUser_HrApp, "HRData\\db_user:svc-hrapp",
                "Database user in HRData for CORP\\svc-hrapp login. " +
                "Member of hr-admin-role — can read and write Employees and Salaries.");
            var dbUser_countryRef = MakeDbUser(Id_DbUser_CountryRef, "CountryRef\\db_user:svc-countryref",
                "Database user in CountryRef for SQL\\svc-countryref-ro. " +
                "Single explicit GRANT: Countries(SELECT). Nothing else.");
            var dbUser_orders = MakeDbUser(Id_DbUser_Orders, "OrdersDB\\db_user:svc-orders",
                "Database user in OrdersDB for SQL\\svc-orders. " +
                "Member of orders-rw-role. READ/WRITE on Orders and OrderItems.");
            var dbUser_auditUser = MakeDbUser(Id_DbUser_AuditUser, "AuditLog\\db_user:auditors",
                "Database user in AuditLog for the AD\\Auditors group login. " +
                "SELECT on audit.AccessLog only. No INSERT/UPDATE/DELETE.");

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 4 — WEB APPLICATIONS AND ENDPOINTS
            // ─────────────────────────────────────────────────────────────────────

            var webApp_customerPortal = new Resource
            {
                Id = Id_WebApp_CustomerPortal,
                Name = "CustomerPortal",
                Type = ResourceType.BusinessApp,
                Description = "Customer-facing web application. Serves customer data through REST endpoints. " +
                              "Uses svc-webapp AD identity for all database calls.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active"
            };
            var webApp_hrPortal = new Resource
            {
                Id = Id_WebApp_HrPortal,
                Name = "HRPortal",
                Type = ResourceType.BusinessApp,
                Description = "Internal HR portal. Serves employee and salary data. " +
                              "Uses svc-hrapp AD identity. Only reachable from corporate network.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active"
            };

            var ep_getCustomers = MakeEndpoint(Id_Ep_GetCustomers, "GET /api/customers",
                "Returns paginated customer list. DTO: { id, name, email, address, countryName }. " +
                "Assembles from dbo.Customers (primary) + dbo.Addresses (secondary) + dbo.Countries (lookup).");
            var ep_getCustomerById = MakeEndpoint(Id_Ep_GetCustomerById, "GET /api/customers/{id}",
                "Returns single customer with full detail including payment summary. " +
                "Joins dbo.Customers + dbo.Payments. SENSITIVE: exposes card_last4.");
            var ep_postOrder = MakeEndpoint(Id_Ep_PostOrder, "POST /api/orders",
                "Creates a new order. Inserts into dbo.Orders and dbo.OrderItems transactionally. " +
                "Write endpoint — requires both Execute and Write capabilities.");
            var ep_getEmployees = MakeEndpoint(Id_Ep_GetEmployees, "GET /api/hr/employees",
                "Returns employee directory: name, title, department, manager. " +
                "No salary data. Accessible to HR portal users and auditors.");
            var ep_getSalaries = MakeEndpoint(Id_Ep_GetSalaries, "GET /api/hr/salaries",
                "Returns salary report. TopSecret — restricted to executives and payroll admin.");

            // Register all resources in context
            ctx.Resources.AddRange(new[]
            {
                // AD and identity
                ad,
                alice, bob, carol, dave, eve, frank, grace, harry, ivan, judy,
                alice_adm, carol_adm, dave_adm,
                svc_webapp, svc_reporting, svc_hrapp, svc_orders,
                grp_webAppUsers, grp_dbReaders, grp_dbAdmins, grp_hrPortalUsers,
                grp_auditors, grp_dataAccess, grp_executives, grp_devops,

                // SQL infrastructure
                sqlSrv_prod01, sqlSrv_prod02,
                db_master_prod01, db_master_prod02,
                db_customerData, db_orders, db_countryRef, db_hrData, db_auditLog,
                tbl_customers, tbl_payments, tbl_addresses,
                tbl_orders, tbl_orderItems, tbl_countries,
                tbl_employees, tbl_salaries, tbl_accessLog,
                row_customerSSN, row_salaryRecord,

                // SQL IAM
                srvRole_sysadmin, srvRole_dbCreator, srvRole_securityAdmin,
                login_sa, login_webapp, login_reporting, login_hrapp,
                login_countryRef, login_carolDba, login_orders,
                dbRole_custDataReader, dbRole_custDataWriter, dbRole_ordersReadWrite,
                dbRole_hrReadOnly, dbRole_hrAdmin,
                dbUser_webapp, dbUser_reporting, dbUser_hrapp,
                dbUser_countryRef, dbUser_orders, dbUser_auditUser,

                // Web tier
                webApp_customerPortal, webApp_hrPortal,
                ep_getCustomers, ep_getCustomerById, ep_postOrder,
                ep_getEmployees, ep_getSalaries
            });

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 5 — STRUCTURAL RELATIONSHIPS
            // ─────────────────────────────────────────────────────────────────────

            ctx.Relationships.AddRange(new[]
            {
                // AD hosts all identities
                Rel(Id_AD, Id_Alice,              RelationshipType.HostedIn, "alice@corp.com lives in corp.AD"),
                Rel(Id_AD, Id_Bob,                RelationshipType.HostedIn),
                Rel(Id_AD, Id_Carol,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Dave,               RelationshipType.HostedIn),
                Rel(Id_AD, Id_Eve,                RelationshipType.HostedIn),
                Rel(Id_AD, Id_Frank,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grace,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Harry,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Ivan,               RelationshipType.HostedIn),
                Rel(Id_AD, Id_Judy,               RelationshipType.HostedIn),
                Rel(Id_AD, Id_Alice_Adm,          RelationshipType.HostedIn, "alice-adm@ is a secondary AD account for alice@"),
                Rel(Id_AD, Id_Carol_Adm,          RelationshipType.HostedIn, "carol-adm@ is the DBA elevated account for carol@"),
                Rel(Id_AD, Id_Dave_Adm,           RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_WebApp,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_Reporting,  RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_HrApp,      RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_Orders,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_WebAppUsers,    RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DbReaders,      RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DbAdmins,       RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_HrPortalUsers,  RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_Auditors,       RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DataAccess,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_Executives,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DevOps,         RelationshipType.HostedIn),

                // SQLSRV-PROD-01 hosts its master DB, user DBs, server-level objects
                Rel(Id_SqlSrv_Prod01, Id_Db_Master_Prod01, RelationshipType.HostedIn, "master DB on PROD-01"),
                Rel(Id_SqlSrv_Prod01, Id_Db_CustomerData,  RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod01, Id_Db_Orders,        RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod01, Id_Db_CountryRef,    RelationshipType.HostedIn),
                // Server roles and logins live in master DB
                Rel(Id_Db_Master_Prod01, Id_SrvRole_Sysadmin,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_SrvRole_DbCreator,     RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_SrvRole_SecurityAdmin, RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_SA,              RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_WebApp,          RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_Reporting,       RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_CountryRef,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_CarolDba,        RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_Orders,          RelationshipType.HostedIn),

                // SQLSRV-PROD-02 hosts its master DB and HR databases
                Rel(Id_SqlSrv_Prod02, Id_Db_Master_Prod02, RelationshipType.HostedIn, "master DB on PROD-02"),
                Rel(Id_SqlSrv_Prod02, Id_Db_HRData,        RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod02, Id_Db_AuditLog,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod02, Id_Login_HrApp,   RelationshipType.HostedIn),

                // Tables are hosted in their parent databases
                Rel(Id_Db_CustomerData, Id_Tbl_Customers,  RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_Tbl_Payments,   RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_Tbl_Addresses,  RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_Tbl_Orders,     RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_Tbl_OrderItems, RelationshipType.HostedIn),
                Rel(Id_Db_CountryRef,   Id_Tbl_Countries,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_Tbl_Employees,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_Tbl_Salaries,   RelationshipType.HostedIn),
                Rel(Id_Db_AuditLog,     Id_Tbl_AccessLog,  RelationshipType.HostedIn),

                // Database roles live in their database
                Rel(Id_Db_CustomerData, Id_DbRole_CustDataReader,  RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_DbRole_CustDataWriter,  RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_DbRole_OrdersReadWrite, RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbRole_HrReadOnly,      RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbRole_HrAdmin,         RelationshipType.HostedIn),

                // Database users live in their database and depend on their login
                Rel(Id_Db_CustomerData, Id_DbUser_WebApp,     RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_DbUser_Reporting,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbUser_HrApp,      RelationshipType.HostedIn),
                Rel(Id_Db_CountryRef,   Id_DbUser_CountryRef, RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_DbUser_Orders,     RelationshipType.HostedIn),
                Rel(Id_Db_AuditLog,     Id_DbUser_AuditUser,  RelationshipType.HostedIn),

                // DB users depend on (are backed by) their server login
                Rel(Id_DbUser_WebApp,     Id_Login_WebApp,     RelationshipType.DependsOn, "CustomerData\\svc-webapp user ← CORP\\svc-webapp login"),
                Rel(Id_DbUser_Reporting,  Id_Login_Reporting,  RelationshipType.DependsOn, "CustomerData\\DB-Readers user ← CORP\\DB-Readers login"),
                Rel(Id_DbUser_HrApp,      Id_Login_HrApp,      RelationshipType.DependsOn),
                Rel(Id_DbUser_CountryRef, Id_Login_CountryRef, RelationshipType.DependsOn),
                Rel(Id_DbUser_Orders,     Id_Login_Orders,     RelationshipType.DependsOn),

                // Endpoints are hosted in their web application
                Rel(Id_WebApp_CustomerPortal, Id_Ep_GetCustomers,    RelationshipType.HostedIn),
                Rel(Id_WebApp_CustomerPortal, Id_Ep_GetCustomerById, RelationshipType.HostedIn),
                Rel(Id_WebApp_CustomerPortal, Id_Ep_PostOrder,       RelationshipType.HostedIn),
                Rel(Id_WebApp_HrPortal,       Id_Ep_GetEmployees,    RelationshipType.HostedIn),
                Rel(Id_WebApp_HrPortal,       Id_Ep_GetSalaries,     RelationshipType.HostedIn),

                // Web apps use AD service accounts as their runtime identity
                Rel(Id_WebApp_CustomerPortal, Id_SvcAcct_WebApp,  RelationshipType.UsesIdentity,
                    "CustomerPortal authenticates to SQL as svc-webapp@corp.com (Windows Auth)"),
                Rel(Id_WebApp_HrPortal,       Id_SvcAcct_HrApp,   RelationshipType.UsesIdentity,
                    "HRPortal authenticates to SQL as svc-hrapp@corp.com (Windows Auth)"),

                // SQL logins that are AD-integrated also use the AD service account identity
                Rel(Id_Login_WebApp,    Id_SvcAcct_WebApp,    RelationshipType.UsesIdentity,
                    "CORP\\svc-webapp SQL login delegates from svc-webapp@corp.com AD identity"),
                Rel(Id_Login_Reporting, Id_Grp_DbReaders,     RelationshipType.UsesIdentity,
                    "CORP\\DB-Readers SQL login delegates from AD\\DB-Readers group — all members authenticate under this login"),
                Rel(Id_Login_HrApp,     Id_SvcAcct_HrApp,     RelationshipType.UsesIdentity),
                Rel(Id_Login_CarolDba,  Id_Carol_Adm,         RelationshipType.UsesIdentity,
                    "CORP\\carol-adm SQL login delegates from carol-adm@corp.com elevated AD account")
            });

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 6 — GROUP MEMBERSHIPS
            // ─────────────────────────────────────────────────────────────────────

            ctx.Memberships.AddRange(new[]
            {
                // AD\WebApp-Users  → alice, frank, grace, bob can call CustomerPortal
                Membership(Id_Grp_WebAppUsers, Id_Alice, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Frank, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Grace, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Bob,   "Member"),

                // AD\DB-Readers  → bob, grace, judy have direct DB read (BYPASS PATH)
                Membership(Id_Grp_DbReaders, Id_Bob,   "Member"),
                Membership(Id_Grp_DbReaders, Id_Grace, "Member"),
                Membership(Id_Grp_DbReaders, Id_Judy,  "Member"),

                // AD\DB-Admins  → carol-adm, dave-adm can administer databases
                Membership(Id_Grp_DbAdmins, Id_Carol_Adm, "Member"),
                Membership(Id_Grp_DbAdmins, Id_Dave_Adm,  "Member"),

                // AD\HRPortal-Users → carol, dave can access employee directory
                Membership(Id_Grp_HrPortalUsers, Id_Carol, "Member"),
                Membership(Id_Grp_HrPortalUsers, Id_Dave,  "Member"),

                // AD\Auditors → judy, eve audit HR data and logs
                Membership(Id_Grp_Auditors, Id_Judy, "Member"),
                Membership(Id_Grp_Auditors, Id_Eve,  "Member"),

                // AD\Executives → harry can view salary report
                Membership(Id_Grp_Executives, Id_Harry, "Member"),

                // AD\DevOps → ivan can post orders
                Membership(Id_Grp_DevOps, Id_Ivan, "Member"),

                // ── GROUP-OF-GROUPS ────────────────────────────────────────────────
                // AD\Data-Access contains AD\DB-Readers as a child group.
                // Any member of AD\DB-Readers is transitively a member of AD\Data-Access.
                // The AccessGraphResolver Rule 5 resolves this recursively.
                Membership(Id_Grp_DataAccess, Id_Grp_DbReaders, "Member"),

                // DB role memberships  (who belongs to which database role)
                Membership(Id_DbRole_CustDataReader,  Id_DbUser_Reporting, "Member"),
                Membership(Id_DbRole_OrdersReadWrite, Id_DbUser_Orders,    "Member"),
                Membership(Id_DbRole_HrAdmin,         Id_DbUser_HrApp,     "Member"),
                Membership(Id_DbRole_HrReadOnly,      Id_DbUser_AuditUser, "Member"),

                // Server role memberships
                Membership(Id_SrvRole_Sysadmin,      Id_Login_SA,       "Member"),
                Membership(Id_SrvRole_SecurityAdmin, Id_Login_CarolDba, "Member"),
                Membership(Id_SrvRole_DbCreator,     Id_Login_CarolDba, "Member")
            });

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 7 — ATTRIBUTE DEFINITIONS AND VALUES  (cross-system linkages)
            // ─────────────────────────────────────────────────────────────────────

            var attrDef_linkedAdIdentity = new ResourceAttributeDefinition
            {
                Id = AttrDef_LinkedAdIdentity,
                Key = "linked_ad_identity",
                Label = "Linked AD Identity",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.ServiceAccount,
                HelpText = "The AD ServiceAccount that this SQL login delegates from.",
                IsRequired = false
            };
            var attrDef_linkedAdGroup = new ResourceAttributeDefinition
            {
                Id = AttrDef_LinkedAdGroup,
                Key = "linked_ad_group",
                Label = "Linked AD Group",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.Group,
                HelpText = "The AD Group this SQL login or role is mapped to via Windows Auth.",
                IsRequired = false
            };
            var attrDef_linkedDbUser = new ResourceAttributeDefinition
            {
                Id = AttrDef_LinkedDbUser,
                Key = "linked_db_user",
                Label = "Linked Database User",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.Account,
                HelpText = "The database-scoped user resource that corresponds to this server login.",
                IsRequired = false
            };
            var attrDef_parentTable = new ResourceAttributeDefinition
            {
                Id = AttrDef_ParentTable,
                Key = "parent_table",
                Label = "Parent Table",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.DataStore,
                HelpText = "For row-level resources: the DataStore (table) that contains this row. " +
                           "Used by the traversal engine to bubble row sensitivity up to the table.",
                IsRequired = false
            };

            ctx.AttributeDefinitions.AddRange(new[]
            {
                attrDef_linkedAdIdentity, attrDef_linkedAdGroup,
                attrDef_linkedDbUser, attrDef_parentTable
            });

            // Attribute Values  (actual cross-system links)

            // SQL login ← AD service account
            AddAttrValue(ctx, Id_Login_WebApp, AttrDef_LinkedAdIdentity, Id_SvcAcct_WebApp.ToString(), attrDef_linkedAdIdentity);
            AddAttrValue(ctx, Id_Login_HrApp, AttrDef_LinkedAdIdentity, Id_SvcAcct_HrApp.ToString(), attrDef_linkedAdIdentity);
            AddAttrValue(ctx, Id_Login_CarolDba, AttrDef_LinkedAdIdentity, Id_Carol_Adm.ToString(), attrDef_linkedAdIdentity);

            // SQL login (group login) ← AD group
            AddAttrValue(ctx, Id_Login_Reporting, AttrDef_LinkedAdGroup, Id_Grp_DbReaders.ToString(), attrDef_linkedAdGroup);

            // DB roles that map to AD groups
            AddAttrValue(ctx, Id_DbRole_CustDataReader, AttrDef_LinkedAdGroup, Id_Grp_DbReaders.ToString(), attrDef_linkedAdGroup);

            // Row → parent table (enables row-level sensitivity to bubble to table)
            AddAttrValue(ctx, Id_Row_CustomerSSN, AttrDef_ParentTable, Id_Tbl_Customers.ToString(), attrDef_parentTable);
            AddAttrValue(ctx, Id_Row_SalaryRecord, AttrDef_ParentTable, Id_Tbl_Salaries.ToString(), attrDef_parentTable);

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 8 — CAPABILITIES
            // ─────────────────────────────────────────────────────────────────────

            // ── Endpoint capabilities ──────────────────────────────────────────────
            ctx.Capabilities.AddRange(new[]
            {
                MakeCap(Cap_Ep_GetCustomers_Exec,    Id_Ep_GetCustomers,    CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/customers. Returns customer list with country enrichment."),
                MakeCap(Cap_Ep_GetCustomerById_Exec, Id_Ep_GetCustomerById, CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/customers/{id}. Returns customer detail including payment summary."),
                MakeCap(Cap_Ep_PostOrder_Exec,       Id_Ep_PostOrder,       CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke POST /api/orders to create an order. Requires Write capability too."),
                // POST /api/orders also needs an explicit Write capability because it modifies data
                MakeCap(Cap_Ep_PostOrder_Write,      Id_Ep_PostOrder,       CapabilityType.Write, CapabilityScope.ContentAccess,
                    "POST /api/orders write path. Inserts into Orders and OrderItems."),
                MakeCap(Cap_Ep_GetEmployees_Exec,    Id_Ep_GetEmployees,    CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/hr/employees. Returns employee directory (no salary)."),
                MakeCap(Cap_Ep_GetSalaries_Exec,     Id_Ep_GetSalaries,     CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/hr/salaries. Returns salary report. TopSecret — exec-only.")
            });

            // ── Table-level capabilities  (granular SQL IAM) ───────────────────────
            ctx.Capabilities.AddRange(new[]
            {
                // dbo.Customers
                MakeCap(Cap_Tbl_Customers_Read,   Id_Tbl_Customers, CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on dbo.Customers. Requires DBA approval."),
                MakeCap(Cap_Tbl_Customers_Write,  Id_Tbl_Customers, CapabilityType.Write,  CapabilityScope.ContentAccess,
                    "INSERT/UPDATE on dbo.Customers. Restricted to batch import role."),
                MakeCap(Cap_Tbl_Customers_Delete, Id_Tbl_Customers, CapabilityType.Delete, CapabilityScope.ContentAccess,
                    "DELETE on dbo.Customers. DBA-only for GDPR erasure workflows."),

                // dbo.Payments
                MakeCap(Cap_Tbl_Payments_Read,    Id_Tbl_Payments,  CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on dbo.Payments. PCI-DSS restricted. Explicit grant required per principal."),

                // dbo.Addresses
                MakeCap(Cap_Tbl_Addresses_Read,   Id_Tbl_Addresses, CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on dbo.Addresses."),
                MakeCap(Cap_Tbl_Addresses_Write,  Id_Tbl_Addresses, CapabilityType.Write,  CapabilityScope.ContentAccess,
                    "INSERT/UPDATE on dbo.Addresses. Fulfilment service only."),

                // dbo.Orders / dbo.OrderItems
                MakeCap(Cap_Tbl_Orders_Read,      Id_Tbl_Orders,     CapabilityType.Read,  CapabilityScope.ContentAccess,
                    "SELECT on dbo.Orders."),
                MakeCap(Cap_Tbl_Orders_Write,     Id_Tbl_Orders,     CapabilityType.Write, CapabilityScope.ContentAccess,
                    "INSERT/UPDATE on dbo.Orders. Order-processing service only."),
                MakeCap(Cap_Tbl_OrderItems_Read,  Id_Tbl_OrderItems, CapabilityType.Read,  CapabilityScope.ContentAccess,
                    "SELECT on dbo.OrderItems."),
                MakeCap(Cap_Tbl_OrderItems_Write, Id_Tbl_OrderItems, CapabilityType.Write, CapabilityScope.ContentAccess,
                    "INSERT/UPDATE on dbo.OrderItems."),

                // dbo.Countries
                MakeCap(Cap_Tbl_Countries_Read,   Id_Tbl_Countries, CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on dbo.Countries. Public reference data."),

                // hr.Employees / hr.Salaries
                MakeCap(Cap_Tbl_Employees_Read,   Id_Tbl_Employees, CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on hr.Employees."),
                MakeCap(Cap_Tbl_Employees_Write,  Id_Tbl_Employees, CapabilityType.Write,  CapabilityScope.ContentAccess,
                    "INSERT/UPDATE on hr.Employees. HR admin only."),
                MakeCap(Cap_Tbl_Salaries_Read,    Id_Tbl_Salaries,  CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on hr.Salaries. TopSecret — hr-admin-role only."),

                // audit.AccessLog
                MakeCap(Cap_Tbl_AccessLog_Read,   Id_Tbl_AccessLog, CapabilityType.Read,   CapabilityScope.ContentAccess,
                    "SELECT on audit.AccessLog. Auditors and SIEM."),
                MakeCap(Cap_Tbl_AccessLog_Write,  Id_Tbl_AccessLog, CapabilityType.Write,  CapabilityScope.ContentAccess,
                    "INSERT on audit.AccessLog. SIEM writer account only — no human grant.")
            });

            // ── Database and server-level administrative capabilities ───────────────
            ctx.Capabilities.AddRange(new[]
            {
                MakeCap(Cap_Db_CustomerData_Administer, Id_Db_CustomerData, CapabilityType.Administer, CapabilityScope.SelfManagement,
                    "DBA-level administration of CustomerData: schema changes, index builds, backup."),
                MakeCap(Cap_Db_HRData_Administer,       Id_Db_HRData,       CapabilityType.Administer, CapabilityScope.SelfManagement,
                    "DBA-level administration of HRData."),
                MakeCap(Cap_SrvRole_Sysadmin_Administer, Id_SrvRole_Sysadmin, CapabilityType.Administer, CapabilityScope.SelfManagement,
                    "Full server administration via sysadmin role membership.")
            });

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 9 — CAPABILITY GRANTS  (active entitlements)
            // ─────────────────────────────────────────────────────────────────────

            // ── Endpoint grants  (AD groups → endpoint capabilities) ──────────────
            ctx.Grants.AddRange(new[]
            {
                // AD\WebApp-Users → GET /api/customers and GET /api/customers/{id}
                MakeGrant(Grant_WebAppUsers_GetCustomers,    Cap_Ep_GetCustomers_Exec,    Id_Grp_WebAppUsers,
                    "WebApp-Users AD group is the authorised consumer of the customer list endpoint.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_WebAppUsers_GetCustomerById, Cap_Ep_GetCustomerById_Exec, Id_Grp_WebAppUsers,
                    "WebApp-Users can retrieve individual customer records including payment summary.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),

                // AD\DevOps → POST /api/orders  (both Execute and Write required)
                MakeGrant(Grant_DevOps_PostOrder, Cap_Ep_PostOrder_Exec, Id_Grp_DevOps,
                    "DevOps team manages order ingestion pipeline via this endpoint.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),

                // AD\HRPortal-Users → GET /api/hr/employees
                MakeGrant(Grant_HrPortalUsers_GetEmployees, Cap_Ep_GetEmployees_Exec, Id_Grp_HrPortalUsers,
                    "HR portal users access the employee directory.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),

                // AD\Auditors → GET /api/hr/employees  (read-only audit access)
                MakeGrant(Grant_Auditors_GetEmployees, Cap_Ep_GetEmployees_Exec, Id_Grp_Auditors,
                    "Auditors need employee list for compliance cross-checks.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),

                // AD\Executives → GET /api/hr/salaries  (restricted to exec leadership)
                MakeGrant(Grant_Executives_GetSalaries, Cap_Ep_GetSalaries_Exec, Id_Grp_Executives,
                    "Executive leadership requires salary visibility for compensation review.",
                    new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc))
            });

            // ── Table-level grants  (database users/roles → table capabilities) ───
            ctx.Grants.AddRange(new[]
            {
                // CustomerData: svc-webapp db user  → Customers(SELECT), Addresses(SELECT), Payments(SELECT)
                MakeGrant(Grant_DbUser_WebApp_Cust_Read,  Cap_Tbl_Customers_Read,  Id_DbUser_WebApp,
                    "svc-webapp db user requires SELECT on Customers to serve customer list endpoint.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_WebApp_Addr_Read,  Cap_Tbl_Addresses_Read,  Id_DbUser_WebApp,
                    "svc-webapp requires SELECT on Addresses for customer detail responses.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_WebApp_Pay_Read,   Cap_Tbl_Payments_Read,   Id_DbUser_WebApp,
                    "svc-webapp requires SELECT on Payments for GET /api/customers/{id} payment summary.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),

                // CustomerData: db-readers-role → Customers(SELECT)
                // NOTE: This role is mapped to AD\DB-Readers — all group members get this grant transitively.
                MakeGrant(Grant_DbRole_CustReader_Cust_Read, Cap_Tbl_Customers_Read, Id_DbRole_CustDataReader,
                    "db-readers-role grants SELECT on Customers to all role members (incl. AD\\DB-Readers group).",
                    new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)),

                // CustomerData: Reporting db user → Customers(SELECT)  (explicit user-level grant)
                MakeGrant(Grant_DbUser_Reporting_Cust_Read, Cap_Tbl_Customers_Read, Id_DbUser_Reporting,
                    "Reporting db user has direct SELECT on Customers in addition to role membership.",
                    new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)),

                // CountryRef: svc-countryref-ro → Countries(SELECT)
                MakeGrant(Grant_DbUser_CountryRef_Countries, Cap_Tbl_Countries_Read, Id_DbUser_CountryRef,
                    "Read-only lookup access to ISO country reference data.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),

                // OrdersDB: svc-orders → Orders(SELECT/INSERT), OrderItems(SELECT/INSERT)
                MakeGrant(Grant_DbUser_Orders_Orders_Read,  Cap_Tbl_Orders_Read,      Id_DbUser_Orders,
                    "svc-orders requires SELECT on Orders for idempotency checks.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Orders_Write, Cap_Tbl_Orders_Write,     Id_DbUser_Orders,
                    "svc-orders requires INSERT/UPDATE on Orders for order creation.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Items_Read,   Cap_Tbl_OrderItems_Read,  Id_DbUser_Orders,
                    "svc-orders requires SELECT on OrderItems.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Items_Write,  Cap_Tbl_OrderItems_Write, Id_DbUser_Orders,
                    "svc-orders requires INSERT on OrderItems.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),

                // HRData: svc-hrapp → Employees(SELECT/INSERT), Salaries(SELECT)
                MakeGrant(Grant_DbUser_HrApp_Emp_Read,  Cap_Tbl_Employees_Read,  Id_DbUser_HrApp,
                    "svc-hrapp requires SELECT on Employees.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_HrApp_Emp_Write, Cap_Tbl_Employees_Write, Id_DbUser_HrApp,
                    "svc-hrapp requires INSERT/UPDATE on Employees for HR record management.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_HrApp_Sal_Read,  Cap_Tbl_Salaries_Read,   Id_DbUser_HrApp,
                    "svc-hrapp requires SELECT on Salaries for salary report endpoint.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),

                // AuditLog: audit-user → AccessLog(SELECT), Employees(SELECT)
                MakeGrant(Grant_DbUser_Audit_Emp_Read,  Cap_Tbl_Employees_Read,  Id_DbUser_AuditUser,
                    "Auditors need employee reference data to correlate audit log entries.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Audit_Log_Read,  Cap_Tbl_AccessLog_Read,  Id_DbUser_AuditUser,
                    "Auditors have SELECT on the audit access log.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),

                // BYPASS PATH: AD\DB-Readers group directly on Customers(SELECT) via role grant
                // This is the direct DB bypass — members can query without going through the portal.
                MakeGrant(Grant_Grp_DbReaders_Cust_Read, Cap_Tbl_Customers_Read, Id_Grp_DbReaders,
                    "LEGACY BYPASS: AD\\DB-Readers inherited SELECT on Customers via Windows Auth group login. " +
                    "Members (bob, grace, judy) can query CustomerData directly — not just via endpoint.",
                    new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)),

                // DB admin grants
                MakeGrant(Grant_Carol_Administer_CustData, Cap_Db_CustomerData_Administer, Id_Login_CarolDba,
                    "carol-adm DBA login has full administrative access to CustomerData.",
                    new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_Carol_Administer_HRData,   Cap_Db_HRData_Administer,       Id_Login_CarolDba,
                    "carol-adm DBA login has full administrative access to HRData.",
                    new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc)),

                // Server role grant
                MakeGrant(Grant_Login_SA_Sysadmin, Cap_SrvRole_Sysadmin_Administer, Id_Login_SA,
                    "SA native login has sysadmin role. Should be disabled in prod.",
                    new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            });

            // ─────────────────────────────────────────────────────────────────────
            // SECTION 10 — CONTENT BINDINGS
            // ─────────────────────────────────────────────────────────────────────
            //
            // Each binding declares one source-to-consumer relationship, stating
            // WHAT data flows from WHERE and USING WHICH accessor identity.
            //
            // For AccessSurface endpoints, bindings make the composite content graph
            // explicit — the sensitivity resolver uses them to bubble sensitivity
            // from tables up to endpoints, and from rows up to tables.

            ctx.ContentBindings.AddRange(new[]
            {
                // ── GET /api/customers  (3 sources: Customers PrimarySource, Addresses Secondary, Countries Lookup)
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Customers,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Customers,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "GET /api/customers reads paginated rows from dbo.Customers via svc-webapp db user.",
                    ContributionDescription = "id, name, email, phone — core customer fields. PII. Sensitivity: Restricted."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Addresses,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Addresses,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.SecondarySource,
                    IsActive           = true,
                    Description        = "Address details joined into the customer DTO.",
                    ContributionDescription = "line1, city, postcode, country_code — shipping address fields."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Countries,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Countries,
                    AccessorResourceId = Id_DbUser_CountryRef,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.LookupSource,
                    IsActive           = true,
                    Description        = "Country name resolved from CountryRef DB via svc-countryref-ro db user.",
                    ContributionDescription = "countryName — display label for country_code. Non-sensitive public data."
                },

                // ── GET /api/customers/{id}  (Customers + Payments — note: Payments is TopSecret)
                new ContentBinding
                {
                    Id = Binding_GetCustomerById_Customers,
                    ConsumerResourceId = Id_Ep_GetCustomerById,
                    ContentSourceId    = Id_Tbl_Customers,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "Primary customer record for the single-customer detail view.",
                    ContributionDescription = "Full customer fields."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomerById_Payments,
                    ConsumerResourceId = Id_Ep_GetCustomerById,
                    ContentSourceId    = Id_Tbl_Payments,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.SecondarySource,
                    IsActive           = true,
                    Description        = "Payment summary joined into the customer detail DTO. Elevates endpoint sensitivity to TopSecret.",
                    ContributionDescription = "card_last4, billing_zip — PCI-DSS fields. TopSecret."
                },

                // ── POST /api/orders  (Orders + OrderItems as WriteTargets)
                new ContentBinding
                {
                    Id = Binding_PostOrder_Orders,
                    ConsumerResourceId = Id_Ep_PostOrder,
                    ContentSourceId    = Id_Tbl_Orders,
                    AccessorResourceId = Id_DbUser_Orders,
                    AccessType         = ContentBindingAccessType.Write,
                    Role               = ContentBindingRole.WriteTarget,
                    IsActive           = true,
                    Description        = "POST /api/orders inserts an order header into dbo.Orders.",
                    ContributionDescription = "order_id, customer_id, status, total_amount — written on POST."
                },
                new ContentBinding
                {
                    Id = Binding_PostOrder_OrderItems,
                    ConsumerResourceId = Id_Ep_PostOrder,
                    ContentSourceId    = Id_Tbl_OrderItems,
                    AccessorResourceId = Id_DbUser_Orders,
                    AccessType         = ContentBindingAccessType.Write,
                    Role               = ContentBindingRole.WriteTarget,
                    IsActive           = true,
                    Description        = "POST /api/orders inserts line items into dbo.OrderItems.",
                    ContributionDescription = "line items — product_id, qty, unit_price written on POST."
                },

                // ── GET /api/hr/employees
                new ContentBinding
                {
                    Id = Binding_GetEmployees_Employees,
                    ConsumerResourceId = Id_Ep_GetEmployees,
                    ContentSourceId    = Id_Tbl_Employees,
                    AccessorResourceId = Id_DbUser_HrApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "Employee directory read from hr.Employees via svc-hrapp db user.",
                    ContributionDescription = "name, title, department, manager_id — no salary fields."
                },

                // ── GET /api/hr/salaries
                new ContentBinding
                {
                    Id = Binding_GetSalaries_Salaries,
                    ConsumerResourceId = Id_Ep_GetSalaries,
                    ContentSourceId    = Id_Tbl_Salaries,
                    AccessorResourceId = Id_DbUser_HrApp,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "Salary report reads hr.Salaries via svc-hrapp. TopSecret path.",
                    ContributionDescription = "base_salary, bonus, ltip_units — full compensation data."
                },

                // ── Row → Table sensitivity bindings
                // The SSN row's data is served THROUGH the Customers table.
                // When the traversal reaches the SSN row, it bubbles up to Customers
                // via the parent_table attribute (Rule 2b), THEN from Customers to endpoints
                // via the ContentBindings above. This is the row → endpoint sensitivity chain.
                //
                // Additionally, we model the Table as a "consumer" of its own rows so that
                // Rule 6 (ContentBinding reverse) also works when starting traversal from the endpoint.
                new ContentBinding
                {
                    Id = Binding_Tbl_Customers_Row_SSN,
                    ConsumerResourceId = Id_Tbl_Customers,
                    ContentSourceId    = Id_Row_CustomerSSN,
                    AccessorResourceId = null,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "The Customers table aggregates (serves) this TopSecret SSN row. " +
                                         "Any grant or binding on the table transitively touches this row.",
                    ContributionDescription = "SSN row — TopSecret sensitivity flag propagates to the parent table."
                },
                new ContentBinding
                {
                    Id = Binding_Tbl_Salaries_Row_Salary,
                    ConsumerResourceId = Id_Tbl_Salaries,
                    ContentSourceId    = Id_Row_SalaryRecord,
                    AccessorResourceId = null,
                    AccessType         = ContentBindingAccessType.Read,
                    Role               = ContentBindingRole.PrimarySource,
                    IsActive           = true,
                    Description        = "The Salaries table aggregates this executive compensation row.",
                    ContributionDescription = "Executive LTIP row — TopSecret."
                }
            });

            // ═════════════════════════════════════════════════════════════════════
            // REPORTS
            // ═════════════════════════════════════════════════════════════════════

            // 1. Full graph — nodes, relationships, grants, bindings
            PrintGraphNeo4jStyle(ctx);

            // 2. Sensitivity traversal starting from the TopSecret SSN row
            //    Shows full chain: Row → Table → Endpoint → AD Group → Human User
            AnsiConsole.MarkupLine("\n[bold magenta]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[bold magenta]  TRAVERSAL 1: TopSecret Row → Endpoint → Users[/]");
            AnsiConsole.MarkupLine("[bold magenta]═══════════════════════════════════════════════════════════[/]");
            PrintCapabilityTraversal(Id_Row_CustomerSSN, ctx);

            // 3. Sensitivity traversal from the Salaries TopSecret row
            AnsiConsole.MarkupLine("\n[bold magenta]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[bold magenta]  TRAVERSAL 2: Salary Row → HR Endpoint → Executives[/]");
            AnsiConsole.MarkupLine("[bold magenta]═══════════════════════════════════════════════════════════[/]");
            PrintCapabilityTraversal(Id_Row_SalaryRecord, ctx);

            // 4. Sensitivity report (text summary) for the Customers table
            PrintSensitivityReport(Id_Tbl_Customers, ctx);

            // 5. Anomaly detection
            AnsiConsole.MarkupLine("\n[bold red]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[bold red]  CONTENT ANOMALY REPORT[/]");
            AnsiConsole.MarkupLine("[bold red]═══════════════════════════════════════════════════════════[/]");
            var anomalies = ContentAnomalyDetector.DetectAll(ctx);
            if (!anomalies.Any())
            {
                AnsiConsole.MarkupLine("[green]No anomalies detected.[/]");
            }
            else
            {
                foreach (var anomaly in anomalies)
                {
                    var resource = ctx.FindResource(anomaly.ResourceId);
                    AnsiConsole.MarkupLine($"\n  [[{anomaly.Type}]]");
                    AnsiConsole.MarkupLine($"  Resource     : {resource?.Name} ({resource?.Type})");
                    if (anomaly.ActualLevel.HasValue)
                        AnsiConsole.MarkupLine($"  Levels       : required={anomaly.RequiredLevel}  actual={anomaly.ActualLevel}");
                    AnsiConsole.MarkupLine($"  Description  : {anomaly.Description}");
                    AnsiConsole.MarkupLine($"  Recommend    : {anomaly.Recommendation}");
                }
                AnsiConsole.MarkupLine($"\n[red]Total anomalies: {anomalies.Count}[/]");
            }

            // 6. Request flow simulation — end-to-end access check for each endpoint
            AnsiConsole.MarkupLine("\n[bold cyan]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[bold cyan]  REQUEST FLOW SIMULATION[/]");
            AnsiConsole.MarkupLine("[bold cyan]═══════════════════════════════════════════════════════════[/]");
            PrintRequestFlowSimulation(Id_Alice, Id_Ep_GetCustomers, ctx, "alice trying GET /api/customers");
            PrintRequestFlowSimulation(Id_Bob, Id_Ep_GetCustomers, ctx, "bob trying GET /api/customers (also has DB bypass)");
            PrintRequestFlowSimulation(Id_Harry, Id_Ep_GetSalaries, ctx, "harry (executive) trying GET /api/hr/salaries");
            PrintRequestFlowSimulation(Id_Carol, Id_Ep_GetSalaries, ctx, "carol (HR user, not executive) trying GET /api/hr/salaries — EXPECT DENY");
            PrintRequestFlowSimulation(Id_Ivan, Id_Ep_PostOrder, ctx, "ivan (DevOps) trying POST /api/orders");
            PrintRequestFlowSimulation(Id_Judy, Id_Ep_GetEmployees, ctx, "judy (auditor) trying GET /api/hr/employees");

        }

        static Resource MakeAccount(Guid id, string name, string desc) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.Account,
            Description = desc,
            Status = "Active"
        };

        static Resource MakeGroup(Guid id, string name, string desc) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.Group,
            Description = desc,
            Status = "Active"
        };

        static Resource MakeDatabase(Guid id, string name, string desc, SensitivityClassification sensitivity) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.Database,
            Description = desc,
            ContentAccessModel = ContentAccessModel.AccessSurface,
            ContentNature = ContentNature.Dynamic,
            Sensitivity = sensitivity,
            Status = "Active"
        };

        static Resource MakeTable(Guid id, string name, string desc, SensitivityClassification sensitivity) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.DataStore,
            Description = desc,
            ContentAccessModel = ContentAccessModel.ResourceIsContent,
            ContentNature = ContentNature.Dynamic,
            Sensitivity = sensitivity,
            Status = "Active"
        };

        static Resource MakeRole(Guid id, string name, string desc) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.Role,
            Description = desc,
            Status = "Active"
        };

        static Resource MakeDbUser(Guid id, string name, string desc) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.ServiceAccount,
            Description = desc,
            Status = "Active"
        };

        static Resource MakeEndpoint(Guid id, string name, string desc) => new()
        {
            Id = id,
            Name = name,
            Type = ResourceType.ServiceEndpoint,
            Description = desc,
            ContentAccessModel = ContentAccessModel.AccessSurface,
            ContentNature = ContentNature.Dynamic,
            Status = "Active"
        };

        static ResourceCapability MakeCap(Guid id, Guid resourceId, CapabilityType type,
            CapabilityScope scope, string desc) => new()
            {
                Id = id,
                ResourceId = resourceId,
                Type = type,
                Scope = scope,
                Description = desc,
                IsEnabled = true,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AllOwners
            };

        static CapabilityGrant MakeGrant(Guid id, Guid capId, Guid subjectId,
            string justification, DateTime activatedAt) => new()
            {
                Id = id,
                ResourceCapabilityId = capId,
                SubjectResourceId = subjectId,
                Status = GrantStatus.Active,
                Justification = justification,
                ActivatedAt = activatedAt
            };

        static void AddAttrValue(AccessGraphContext ctx, Guid resourceId, Guid defId,
            string value, ResourceAttributeDefinition def)
        {
            ctx.AttributeValues.Add(new ResourceAttributeValue
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ResourceAttributeDefinitionId = defId,
                ValueString = value,
                AttributeDefinition = def
            });
        }

        static ResourceRelationship Rel(Guid parentId, Guid childId,
            RelationshipType type, string? notes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                ParentResourceId = parentId,
                ChildResourceId = childId,
                Type = type,
                Notes = notes
            };

        static BusinessAppMembership Membership(Guid groupId, Guid memberId,
            string role = "Member") =>
            new()
            {
                Id = Guid.NewGuid(),
                BusinessAppResourceId = groupId,
                MemberResourceId = memberId,
                Role = role,
                IsActive = true
            };
        
        // ═════════════════════════════════════════════════════════════════════════
        // PRINT HELPERS
        // ═════════════════════════════════════════════════════════════════════════

        static void PrintGraphNeo4jStyle(AccessGraphContext ctx)
        {
            // Nodes
            AnsiConsole.MarkupLine("\n[bold yellow]NODES[/]");
            var nodeTable = new Table().Border(TableBorder.Rounded);
            nodeTable.AddColumn("Id"); nodeTable.AddColumn("Type");
            nodeTable.AddColumn("Name"); nodeTable.AddColumn("Sensitivity");
            foreach (var r in ctx.Resources)
                nodeTable.AddRow(r.Id.ToString()[..8] + "…", r.Type.ToString(), r.Name, r.Sensitivity.ToString());
            AnsiConsole.Write(nodeTable);

            // Relationships
            AnsiConsole.MarkupLine("\n[bold yellow]RELATIONSHIPS[/]");
            var relTable = new Table().Border(TableBorder.Rounded);
            relTable.AddColumn("Parent"); relTable.AddColumn("Type"); relTable.AddColumn("Child"); relTable.AddColumn("Notes");
            foreach (var rel in ctx.Relationships)
            {
                var parent = ctx.FindResource(rel.ParentResourceId)?.Name ?? rel.ParentResourceId.ToString()[..8];
                var child = ctx.FindResource(rel.ChildResourceId)?.Name ?? rel.ChildResourceId.ToString()[..8];
                relTable.AddRow(parent, rel.Type.ToString(), child, rel.Notes ?? "");
            }
            AnsiConsole.Write(relTable);

            // Group Memberships
            AnsiConsole.MarkupLine("\n[bold yellow]MEMBERSHIPS[/]");
            var memTable = new Table().Border(TableBorder.Rounded);
            memTable.AddColumn("Group / Role"); memTable.AddColumn("Member"); memTable.AddColumn("Role");
            foreach (var m in ctx.Memberships)
            {
                var group = ctx.FindResource(m.BusinessAppResourceId)?.Name ?? m.BusinessAppResourceId.ToString()[..8];
                var member = ctx.FindResource(m.MemberResourceId)?.Name ?? m.MemberResourceId.ToString()[..8];
                memTable.AddRow(group, member, m.Role);
            }
            AnsiConsole.Write(memTable);

            // Capabilities
            AnsiConsole.MarkupLine("\n[bold yellow]CAPABILITIES[/]");
            var capTable = new Table().Border(TableBorder.Rounded);
            capTable.AddColumn("Resource"); capTable.AddColumn("Type"); capTable.AddColumn("Scope"); capTable.AddColumn("Description");
            foreach (var cap in ctx.Capabilities)
            {
                var resource = ctx.FindResource(cap.ResourceId)?.Name ?? cap.ResourceId.ToString()[..8];
                capTable.AddRow(resource, cap.Type.ToString(), cap.Scope.ToString(), cap.Description ?? "");
            }
            AnsiConsole.Write(capTable);

            // Grants
            AnsiConsole.MarkupLine("\n[bold yellow]GRANTS[/]");
            var grantTable = new Table().Border(TableBorder.Rounded);
            grantTable.AddColumn("Subject"); grantTable.AddColumn("Capability"); grantTable.AddColumn("Status"); grantTable.AddColumn("Justification");
            foreach (var g in ctx.Grants)
            {
                var subject = ctx.FindResource(g.SubjectResourceId)?.Name ?? g.SubjectResourceId.ToString()[..8];
                var cap = ctx.Capabilities.FirstOrDefault(c => c.Id == g.ResourceCapabilityId);
                var capDesc = cap != null ? $"{cap.Type} on {ctx.FindResource(cap.ResourceId)?.Name}" : g.ResourceCapabilityId.ToString()[..8];
                grantTable.AddRow(subject, capDesc, g.Status.ToString(), g.Justification?.Substring(0, Math.Min(60, g.Justification.Length)) ?? "");
            }
            AnsiConsole.Write(grantTable);

            // Content Bindings
            AnsiConsole.MarkupLine("\n[bold yellow]CONTENT BINDINGS[/]");
            var bindingTable = new Table().Border(TableBorder.Rounded);
            bindingTable.AddColumn("Source"); bindingTable.AddColumn("Consumer"); bindingTable.AddColumn("Accessor");
            bindingTable.AddColumn("AccessType"); bindingTable.AddColumn("Role"); bindingTable.AddColumn("Active");
            foreach (var b in ctx.ContentBindings)
            {
                var source = ctx.FindResource(b.ContentSourceId)?.Name ?? b.ContentSourceId.ToString()[..8];
                var consumer = ctx.FindResource(b.ConsumerResourceId)?.Name ?? b.ConsumerResourceId.ToString()[..8];
                var accessor = b.AccessorResourceId.HasValue
                    ? ctx.FindResource(b.AccessorResourceId.Value)?.Name ?? "" : "—";
                bindingTable.AddRow(source, consumer, accessor, b.AccessType.ToString(), b.Role.ToString(), b.IsActive ? "✓" : "✗");
            }
            AnsiConsole.Write(bindingTable);
        }

        static void PrintCapabilityTraversal(Guid sensitiveResourceId, AccessGraphContext ctx)
        {
            var touchpoints = AccessGraphResolver.FindTouchpoints(sensitiveResourceId, ctx);
            var source = ctx.FindResource(sensitiveResourceId)!;
            AnsiConsole.MarkupLine($"\n[grey]Source: [bold]{source.Name}[/] ({source.Type}) — Sensitivity: [red]{source.Sensitivity}[/][/]");

            if (touchpoints.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No touchpoints found.[/]");
                return;
            }

            var tree = new Tree($"[bold red]{source.Name}[/] [grey]({source.Type})[/] [yellow]⚡ {source.Sensitivity}[/]");
            var nodesByPath = new Dictionary<string, TreeNode>();
            var rootTp = touchpoints[0];
            var rootPathKey = string.Join(" -> ", rootTp.PathFromSource);
            nodesByPath[rootPathKey] = tree.AddNode($"[bold red]{rootTp.Resource.Name}[/] [grey]({rootTp.Resource.Type})[/]");

            foreach (var tp in touchpoints.Skip(1))
            {
                var pathKey = string.Join(" -> ", tp.PathFromSource);
                var parentPathKey = string.Join(" -> ", tp.PathFromSource.Take(tp.PathFromSource.Count - 1));
                if (!nodesByPath.TryGetValue(parentPathKey, out var parentNode))
                    parentNode = nodesByPath[rootPathKey];

                var sensitivityTag = tp.Resource.Sensitivity >= SensitivityClassification.Restricted
                    ? $"[red]⚠ {tp.Resource.Sensitivity}[/]" : $"[grey]{tp.Resource.Sensitivity}[/]";
                var typeColor = tp.Resource.Type switch
                {
                    ResourceType.Account or ResourceType.ServiceAccount => "cyan",
                    ResourceType.Group => "yellow",
                    ResourceType.ServiceEndpoint => "magenta",
                    ResourceType.DataStore or ResourceType.Database => "blue",
                    _ => "white"
                };
                var node = parentNode.AddNode(
                    $"[{typeColor}]{tp.Resource.Name}[/] [grey]({tp.Resource.Type}) ← {tp.EdgeLabel}[/] {sensitivityTag} [grey]depth:{tp.Depth}[/]");
                nodesByPath[pathKey] = node;
            }

            AnsiConsole.Write(tree);

            var humans = AccessGraphResolver.FindHumanAccessors(sensitiveResourceId, ctx);
            AnsiConsole.MarkupLine($"\n[bold]Unique human/service-account endpoints: {humans.Count}[/]");
            foreach (var h in humans)
                AnsiConsole.MarkupLine($"  [cyan]•[/] {h.Resource.Type}/{h.Resource.Name}  [grey](via {h.EdgeLabel})[/]");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // REQUEST FLOW SIMULATION
        // Walks the IAM graph to answer: "Can this user call this endpoint, and what
        // access checks happen all the way down to the database tables?"
        // ─────────────────────────────────────────────────────────────────────────
        static void PrintRequestFlowSimulation(Guid userId, Guid endpointId, AccessGraphContext ctx, string scenario)
        {
            var user = ctx.FindResource(userId)!;
            var endpoint = ctx.FindResource(endpointId)!;

            AnsiConsole.MarkupLine($"\n[bold white]┌─ SCENARIO: {scenario}[/]");
            AnsiConsole.MarkupLine($"[white]│  User: {user.Name} ({user.Type})[/]");
            AnsiConsole.MarkupLine($"[white]│  Target: {endpoint.Name} ({endpoint.Type})[/]");

            // CHECK 1: Does the user have (directly or via group membership) an active
            //          Execute grant on this endpoint?
            var endpointCaps = ctx.Capabilities
                .Where(c => c.ResourceId == endpointId && c.IsEnabled && c.Type == CapabilityType.Execute)
                .Select(c => c.Id)
                .ToHashSet();

            bool userCanExecute = false;
            string? accessPath = null;

            // Direct grant
            if (ctx.Grants.Any(g => g.Status == GrantStatus.Active
                                  && endpointCaps.Contains(g.ResourceCapabilityId)
                                  && g.SubjectResourceId == userId))
            {
                userCanExecute = true;
                accessPath = $"Direct grant on {user.Name}";
            }

            // Transitive: resolve all groups the user is a member of (including nested groups)
            if (!userCanExecute)
            {
                var userGroups = ResolveTransitiveGroupMemberships(userId, ctx);
                foreach (var grpId in userGroups)
                {
                    if (ctx.Grants.Any(g => g.Status == GrantStatus.Active
                                          && endpointCaps.Contains(g.ResourceCapabilityId)
                                          && g.SubjectResourceId == grpId))
                    {
                        userCanExecute = true;
                        var grpName = ctx.FindResource(grpId)?.Name ?? grpId.ToString();
                        accessPath = $"Via group {grpName}";
                        break;
                    }
                }
            }

            if (!userCanExecute)
            {
                AnsiConsole.MarkupLine("│  [bold red]✗ CHECK 1 FAILED — No Execute grant on endpoint. Request DENIED.[/]");
                AnsiConsole.MarkupLine("[white]└──────────────────────────────────────────────────────────[/]");
                return;
            }

            AnsiConsole.MarkupLine($"│  [green]✓ CHECK 1 PASS — Endpoint access: {accessPath}[/]");

            // CHECK 2: For each ContentBinding on this endpoint, verify the accessor has
            //          a sufficient grant on the source table.
            var bindings = ctx.ContentBindings
                .Where(b => b.IsActive && b.ConsumerResourceId == endpointId)
                .ToList();

            AnsiConsole.MarkupLine($"│  [white]CHECK 2 — Verifying {bindings.Count} content binding(s):[/]");
            bool allBindingsOk = true;

            foreach (var binding in bindings)
            {
                var source = ctx.FindResource(binding.ContentSourceId)!;
                var accessorName = binding.AccessorResourceId.HasValue
                    ? ctx.FindResource(binding.AccessorResourceId.Value)?.Name ?? "unknown"
                    : "none";

                // Find a capability of sufficient level on the source for this access type
                CapabilityType requiredCap = binding.AccessType switch
                {
                    ContentBindingAccessType.Write => CapabilityType.Write,
                    ContentBindingAccessType.ReadWrite => CapabilityType.Write,
                    ContentBindingAccessType.Execute => CapabilityType.Execute,
                    _ => CapabilityType.Read
                };

                var sourceCaps = ctx.Capabilities
                    .Where(c => c.ResourceId == binding.ContentSourceId
                             && c.IsEnabled
                             && c.Type == requiredCap)
                    .Select(c => c.Id)
                    .ToHashSet();

                bool accessorHasGrant = false;
                if (binding.AccessorResourceId.HasValue)
                {
                    var accessorId = binding.AccessorResourceId.Value;
                    // Direct grant on accessor
                    accessorHasGrant = ctx.Grants.Any(g =>
                        g.Status == GrantStatus.Active
                        && sourceCaps.Contains(g.ResourceCapabilityId)
                        && g.SubjectResourceId == accessorId);

                    // Via role membership
                    if (!accessorHasGrant)
                    {
                        var accessorGroups = ResolveTransitiveGroupMemberships(accessorId, ctx);
                        accessorHasGrant = accessorGroups.Any(gId =>
                            ctx.Grants.Any(g => g.Status == GrantStatus.Active
                                             && sourceCaps.Contains(g.ResourceCapabilityId)
                                             && g.SubjectResourceId == gId));
                    }
                }

                string icon = accessorHasGrant ? "[green]  ✓[/]" : "[red]  ✗[/]";
                string role = $"[bold yellow]{binding.Role}[/]";
                AnsiConsole.MarkupLine(
                    $"│  {icon} {role,22} {source.Name,-35} via {accessorName,-30} " +
                    $"({binding.AccessType}) Sensitivity:{source.Sensitivity}");

                if (!accessorHasGrant) allBindingsOk = false;
            }

            if (!allBindingsOk)
                AnsiConsole.MarkupLine("│  [bold red]✗ CHECK 2 PARTIAL FAIL — One or more accessor grants are missing.[/]");
            else
                AnsiConsole.MarkupLine("│  [green]✓ CHECK 2 PASS — All accessor grants verified.[/]");

            // CHECK 3: Determine effective sensitivity of the endpoint response
            var maxSensitivity = bindings
                .Select(b => ctx.FindResource(b.ContentSourceId)?.Sensitivity ?? SensitivityClassification.None)
                .DefaultIfEmpty(SensitivityClassification.None)
                .Max();

            AnsiConsole.MarkupLine($"│  [bold]CHECK 3 — Effective response sensitivity: [red]{maxSensitivity}[/][/]");

            // CHECK 4: Does the user's clearance level match the response sensitivity?
            // For demo purposes we model executive-only endpoints as requiring Restricted+
            bool sensitivityOk = maxSensitivity <= SensitivityClassification.Restricted
                || user.Name.Contains("harry") || user.Name.Contains("adm");
            if (!sensitivityOk)
                AnsiConsole.MarkupLine("│  [red]⚠ CHECK 4 WARN — Response sensitivity exceeds standard clearance for this user.[/]");
            else
                AnsiConsole.MarkupLine("│  [green]✓ CHECK 4 PASS — User clearance sufficient.[/]");

            AnsiConsole.MarkupLine("│");
            string finalVerdict = userCanExecute && allBindingsOk ? "[bold green]✓ REQUEST ALLOWED[/]" : "[bold red]✗ REQUEST WOULD FAIL AT RUNTIME[/]";
            AnsiConsole.MarkupLine($"│  {finalVerdict}");
            AnsiConsole.MarkupLine("[white]└──────────────────────────────────────────────────────────[/]");
        }

        /// <summary>
        /// Resolves all group IDs that the given principal belongs to, including
        /// transitive nested group memberships (group-of-groups).
        /// </summary>
        static HashSet<Guid> ResolveTransitiveGroupMemberships(Guid principalId, AccessGraphContext ctx)
        {
            var resolved = new HashSet<Guid>();
            var queue = new Queue<Guid>();
            queue.Enqueue(principalId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                // Find all groups/BusinessApps that have this principal as a direct member
                var parentGroups = ctx.Memberships
                    .Where(m => m.IsActive && m.MemberResourceId == current)
                    .Select(m => m.BusinessAppResourceId);

                foreach (var grpId in parentGroups)
                {
                    if (resolved.Add(grpId))
                        queue.Enqueue(grpId); // recurse into parent group to find its own groups
                }
            }

            return resolved;
        }

        public static void PrintSensitivityReport(
            Guid sensitiveResourceId,
            AccessGraphContext ctx)
        {
            var source = ctx.FindResource(sensitiveResourceId)!;

            // Header
            AnsiConsole.MarkupLine("\n[bold magenta]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine($"[bold magenta]  SENSITIVITY REPORT[/]");
            AnsiConsole.MarkupLine($"[bold]  Source:[/] [yellow]{source.Name}[/] [grey]({source.Type})[/]  [bold]Sensitivity:[/] [red]{source.Sensitivity}[/]");
            AnsiConsole.MarkupLine("[bold magenta]═══════════════════════════════════════════════════════════[/]");

            var touchpoints = AccessGraphResolver.FindTouchpoints(sensitiveResourceId, ctx);

            // Table for access touchpoints
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[grey]Depth[/]");
            table.AddColumn("[grey]Type[/]");
            table.AddColumn("[grey]Name[/]");
            table.AddColumn("[grey]Edge[/]");

            foreach (var tp in touchpoints.Skip(1)) // skip source itself
            {
                var typeColor = tp.Resource.Type switch
                {
                    ResourceType.Account or ResourceType.ServiceAccount => "cyan",
                    ResourceType.Group => "yellow",
                    ResourceType.ServiceEndpoint => "magenta",
                    ResourceType.DataStore or ResourceType.Database => "blue",
                    _ => "white"
                };
                var sensitivityTag = tp.Resource.Sensitivity >= IdentityMap.DataModel.Enums.SensitivityClassification.Restricted
                    ? $"[red]{tp.Resource.Sensitivity}[/]" : $"[grey]{tp.Resource.Sensitivity}[/]";
                table.AddRow(
                    $"[grey]{tp.Depth}[/]",
                    $"[{typeColor}]{tp.Resource.Type}[/]",
                    $"[{typeColor}]{tp.Resource.Name}[/] {sensitivityTag}",
                    $"[grey]{tp.EdgeLabel}[/]"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine($"\n[bold]Total access touchpoints:[/] [yellow]{touchpoints.Count - 1}[/]");

            var humans = AccessGraphResolver.FindHumanAccessors(sensitiveResourceId, ctx);
            AnsiConsole.MarkupLine($"[bold]Unique human/service-account endpoints:[/] [yellow]{humans.Count}[/]");
            foreach (var h in humans)
                AnsiConsole.MarkupLine($"  [cyan]•[/] {h.Resource.Type}/{h.Resource.Name}  [grey](via {h.EdgeLabel})[/]");
        }
    }
}
