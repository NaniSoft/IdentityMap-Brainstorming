using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using IdentityMap.DataModel.Helpers;
using Spectre.Console;

namespace IdentityMap.DataModel
{
    internal class Program
    {
        // ═══════════════════════════════════════════════════════════════════════
        // WELL-KNOWN IDs  — fixed so the scenario is repeatable and readable.
        // ═══════════════════════════════════════════════════════════════════════

        // ── Identity Provider
        static readonly Guid Id_AD = new("10000000-0000-0000-0000-000000000001");

        // ── Human User Accounts (10 users)
        static readonly Guid Id_Alice = new("10000000-0000-0000-0000-000000000010");
        static readonly Guid Id_Bob = new("10000000-0000-0000-0000-000000000011");
        static readonly Guid Id_Carol = new("10000000-0000-0000-0000-000000000012");
        static readonly Guid Id_Dave = new("10000000-0000-0000-0000-000000000013");
        static readonly Guid Id_Eve = new("10000000-0000-0000-0000-000000000014");
        static readonly Guid Id_Frank = new("10000000-0000-0000-0000-000000000015");
        static readonly Guid Id_Grace = new("10000000-0000-0000-0000-000000000016");
        static readonly Guid Id_Harry = new("10000000-0000-0000-0000-000000000017");
        static readonly Guid Id_Ivan = new("10000000-0000-0000-0000-000000000018");
        static readonly Guid Id_Judy = new("10000000-0000-0000-0000-000000000019");

        // ── Elevated / Secondary AD Accounts
        static readonly Guid Id_Alice_Adm = new("10000000-0000-0000-0000-000000000020");
        static readonly Guid Id_Carol_Adm = new("10000000-0000-0000-0000-000000000021");
        static readonly Guid Id_Dave_Adm = new("10000000-0000-0000-0000-000000000022");

        // ── AD Service Accounts
        static readonly Guid Id_SvcAcct_WebApp = new("10000000-0000-0000-0000-000000000030");
        static readonly Guid Id_SvcAcct_Reporting = new("10000000-0000-0000-0000-000000000031");
        static readonly Guid Id_SvcAcct_HrApp = new("10000000-0000-0000-0000-000000000032");
        static readonly Guid Id_SvcAcct_Orders = new("10000000-0000-0000-0000-000000000033");

        // ── AD Groups
        static readonly Guid Id_Grp_WebAppUsers = new("10000000-0000-0000-0000-000000000040");
        static readonly Guid Id_Grp_DbReaders = new("10000000-0000-0000-0000-000000000041");
        static readonly Guid Id_Grp_DbAdmins = new("10000000-0000-0000-0000-000000000042");
        static readonly Guid Id_Grp_HrPortalUsers = new("10000000-0000-0000-0000-000000000043");
        static readonly Guid Id_Grp_Auditors = new("10000000-0000-0000-0000-000000000044");
        static readonly Guid Id_Grp_DataAccess = new("10000000-0000-0000-0000-000000000045");
        static readonly Guid Id_Grp_Executives = new("10000000-0000-0000-0000-000000000046");
        static readonly Guid Id_Grp_DevOps = new("10000000-0000-0000-0000-000000000047");

        // ── SQL Server Instances
        static readonly Guid Id_SqlSrv_Prod01 = new("20000000-0000-0000-0000-000000000001");
        static readonly Guid Id_SqlSrv_Prod02 = new("20000000-0000-0000-0000-000000000002");

        // ── Databases (including master)
        static readonly Guid Id_Db_Master_Prod01 = new("20000000-0000-0000-0000-000000000010");
        static readonly Guid Id_Db_Master_Prod02 = new("20000000-0000-0000-0000-000000000011");
        static readonly Guid Id_Db_CustomerData = new("20000000-0000-0000-0000-000000000012");
        static readonly Guid Id_Db_Orders = new("20000000-0000-0000-0000-000000000013");
        static readonly Guid Id_Db_CountryRef = new("20000000-0000-0000-0000-000000000014");
        static readonly Guid Id_Db_HRData = new("20000000-0000-0000-0000-000000000015");
        static readonly Guid Id_Db_AuditLog = new("20000000-0000-0000-0000-000000000016");

        // ── Tables  (ResourceType.Table)
        static readonly Guid Id_Tbl_Customers = new("20000000-0000-0000-0000-000000000020");
        static readonly Guid Id_Tbl_Payments = new("20000000-0000-0000-0000-000000000021");
        static readonly Guid Id_Tbl_Addresses = new("20000000-0000-0000-0000-000000000022");
        static readonly Guid Id_Tbl_Orders = new("20000000-0000-0000-0000-000000000023");
        static readonly Guid Id_Tbl_OrderItems = new("20000000-0000-0000-0000-000000000024");
        static readonly Guid Id_Tbl_Countries = new("20000000-0000-0000-0000-000000000025");
        static readonly Guid Id_Tbl_Employees = new("20000000-0000-0000-0000-000000000026");
        static readonly Guid Id_Tbl_Salaries = new("20000000-0000-0000-0000-000000000027");
        static readonly Guid Id_Tbl_AccessLog = new("20000000-0000-0000-0000-000000000028");

        // ── Sensitive Rows  (ResourceType.File)
        static readonly Guid Id_Row_CustomerSSN = new("20000000-0000-0000-0000-000000000030");
        static readonly Guid Id_Row_SalaryRecord = new("20000000-0000-0000-0000-000000000031");

        // ── Server Roles  (hosted in master DB)
        static readonly Guid Id_SrvRole_Sysadmin = new("20000000-0000-0000-0000-000000000040");
        static readonly Guid Id_SrvRole_DbCreator = new("20000000-0000-0000-0000-000000000041");
        static readonly Guid Id_SrvRole_SecurityAdmin = new("20000000-0000-0000-0000-000000000042");

        // ── Server Logins  (hosted in master DB)
        static readonly Guid Id_Login_SA = new("20000000-0000-0000-0000-000000000050");
        static readonly Guid Id_Login_WebApp = new("20000000-0000-0000-0000-000000000051");
        static readonly Guid Id_Login_Reporting = new("20000000-0000-0000-0000-000000000052");
        static readonly Guid Id_Login_HrApp = new("20000000-0000-0000-0000-000000000053");
        static readonly Guid Id_Login_CountryRef = new("20000000-0000-0000-0000-000000000054");
        static readonly Guid Id_Login_CarolDba = new("20000000-0000-0000-0000-000000000055");
        static readonly Guid Id_Login_Orders = new("20000000-0000-0000-0000-000000000056");

        // ── Database Roles
        static readonly Guid Id_DbRole_CustDataReader = new("20000000-0000-0000-0000-000000000060");
        static readonly Guid Id_DbRole_CustDataWriter = new("20000000-0000-0000-0000-000000000061");
        static readonly Guid Id_DbRole_OrdersReadWrite = new("20000000-0000-0000-0000-000000000062");
        static readonly Guid Id_DbRole_HrReadOnly = new("20000000-0000-0000-0000-000000000063");
        static readonly Guid Id_DbRole_HrAdmin = new("20000000-0000-0000-0000-000000000064");

        // ── Database Users
        static readonly Guid Id_DbUser_WebApp = new("20000000-0000-0000-0000-000000000070");
        static readonly Guid Id_DbUser_Reporting = new("20000000-0000-0000-0000-000000000071");
        static readonly Guid Id_DbUser_HrApp = new("20000000-0000-0000-0000-000000000072");
        static readonly Guid Id_DbUser_CountryRef = new("20000000-0000-0000-0000-000000000073");
        static readonly Guid Id_DbUser_Orders = new("20000000-0000-0000-0000-000000000074");
        static readonly Guid Id_DbUser_AuditUser = new("20000000-0000-0000-0000-000000000075");

        // ── Web Applications
        static readonly Guid Id_WebApp_CustomerPortal = new("30000000-0000-0000-0000-000000000001");
        static readonly Guid Id_WebApp_HrPortal = new("30000000-0000-0000-0000-000000000002");

        // ── Service Endpoints
        static readonly Guid Id_Ep_GetCustomers = new("30000000-0000-0000-0000-000000000010");
        static readonly Guid Id_Ep_GetCustomerById = new("30000000-0000-0000-0000-000000000011");
        static readonly Guid Id_Ep_PostOrder = new("30000000-0000-0000-0000-000000000012");
        static readonly Guid Id_Ep_GetEmployees = new("30000000-0000-0000-0000-000000000013");
        static readonly Guid Id_Ep_GetSalaries = new("30000000-0000-0000-0000-000000000014");

        // ── Attribute Definition IDs
        static readonly Guid AttrDef_LinkedAdIdentity = new("40000000-0000-0000-0000-000000000001");
        static readonly Guid AttrDef_LinkedAdGroup = new("40000000-0000-0000-0000-000000000002");
        static readonly Guid AttrDef_LinkedDbUser = new("40000000-0000-0000-0000-000000000003");
        static readonly Guid AttrDef_ParentTable = new("40000000-0000-0000-0000-000000000004");

        // ── Capability IDs — Endpoint
        static readonly Guid Cap_Ep_GetCustomers_Exec = new("50000000-0000-0000-0000-000000000001");
        static readonly Guid Cap_Ep_GetCustomerById_Exec = new("50000000-0000-0000-0000-000000000002");
        static readonly Guid Cap_Ep_PostOrder_Exec = new("50000000-0000-0000-0000-000000000003");
        static readonly Guid Cap_Ep_PostOrder_Write = new("50000000-0000-0000-0000-000000000004");
        static readonly Guid Cap_Ep_GetEmployees_Exec = new("50000000-0000-0000-0000-000000000005");
        static readonly Guid Cap_Ep_GetSalaries_Exec = new("50000000-0000-0000-0000-000000000006");

        // ── Capability IDs — Table level
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

        // ── Capability IDs — DB / Server admin
        static readonly Guid Cap_Db_CustomerData_Administer = new("50000000-0000-0000-0000-000000000030");
        static readonly Guid Cap_Db_HRData_Administer = new("50000000-0000-0000-0000-000000000031");
        static readonly Guid Cap_SrvRole_Sysadmin_Administer = new("50000000-0000-0000-0000-000000000035");

        // ── Grant IDs — Endpoint
        static readonly Guid Grant_WebAppUsers_GetCustomers = new("60000000-0000-0000-0000-000000000001");
        static readonly Guid Grant_WebAppUsers_GetCustomerById = new("60000000-0000-0000-0000-000000000002");
        static readonly Guid Grant_DevOps_PostOrder = new("60000000-0000-0000-0000-000000000003");
        static readonly Guid Grant_HrPortalUsers_GetEmployees = new("60000000-0000-0000-0000-000000000004");
        static readonly Guid Grant_Auditors_GetEmployees = new("60000000-0000-0000-0000-000000000005");
        static readonly Guid Grant_Executives_GetSalaries = new("60000000-0000-0000-0000-000000000006");

        // ── Grant IDs — Table level
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
        static readonly Guid Grant_Grp_DbReaders_Cust_Read = new("60000000-0000-0000-0000-000000000025");
        static readonly Guid Grant_Carol_Administer_CustData = new("60000000-0000-0000-0000-000000000026");
        static readonly Guid Grant_Carol_Administer_HRData = new("60000000-0000-0000-0000-000000000027");
        static readonly Guid Grant_Login_SA_Sysadmin = new("60000000-0000-0000-0000-000000000030");

        // ── Content Binding IDs
        static readonly Guid Binding_GetCustomers_Customers = new("70000000-0000-0000-0000-000000000001");
        static readonly Guid Binding_GetCustomers_Addresses = new("70000000-0000-0000-0000-000000000002");
        static readonly Guid Binding_GetCustomers_Countries = new("70000000-0000-0000-0000-000000000003");
        static readonly Guid Binding_GetCustomerById_Customers = new("70000000-0000-0000-0000-000000000004");
        static readonly Guid Binding_GetCustomerById_Payments = new("70000000-0000-0000-0000-000000000005");
        static readonly Guid Binding_PostOrder_Orders = new("70000000-0000-0000-0000-000000000006");
        static readonly Guid Binding_PostOrder_OrderItems = new("70000000-0000-0000-0000-000000000007");
        static readonly Guid Binding_GetEmployees_Employees = new("70000000-0000-0000-0000-000000000008");
        static readonly Guid Binding_GetSalaries_Salaries = new("70000000-0000-0000-0000-000000000009");
        static readonly Guid Binding_Tbl_Customers_Row_SSN = new("70000000-0000-0000-0000-000000000010");
        static readonly Guid Binding_Tbl_Salaries_Row_Salary = new("70000000-0000-0000-0000-000000000011");

        static void Main(string[] args)
        {
            AnsiConsole.MarkupLine("[bold cyan]╔══════════════════════════════════════════════════════════╗[/]");
            AnsiConsole.MarkupLine("[bold cyan]║  IdentityMap — Enterprise IAM Governance Demo           ║[/]");
            AnsiConsole.MarkupLine("[bold cyan]╚══════════════════════════════════════════════════════════╝[/]");

            var ctx = new AccessGraphContext();

            // ─────────────────────────────────────────────────────────────────
            // SECTION 1 — ACTIVE DIRECTORY
            // ─────────────────────────────────────────────────────────────────

            var ad = new Resource
            {
                Id = Id_AD,
                Name = "corp.AD",
                Type = ResourceType.BusinessApp,
                Description = "Corporate Active Directory. Authoritative identity provider for all " +
                              "human and service accounts and security groups.",
                Status = "Active",
                Tags = T("tier:identity", "kind:idp")
            };

            // ── 10 Human Accounts ─────────────────────────────────────────────
            var alice = MakeAccount(Id_Alice, "alice@corp.com", "Senior Developer — CustomerPortal team.");
            var bob = MakeAccount(Id_Bob, "bob@corp.com", "Data Analyst — reads customer reports.");
            var carol = MakeAccount(Id_Carol, "carol@corp.com", "DBA — day-to-day login.");
            var dave = MakeAccount(Id_Dave, "dave@corp.com", "App Admin — manages CustomerPortal deployments.");
            var eve = MakeAccount(Id_Eve, "eve@corp.com", "Security Officer — reviews audit logs and policy.");
            var frank = MakeAccount(Id_Frank, "frank@corp.com", "Developer — CustomerPortal feature work.");
            var grace = MakeAccount(Id_Grace, "grace@corp.com", "Business Analyst — reads customer data for reporting.");
            var harry = MakeAccount(Id_Harry, "harry@corp.com", "IT Manager / Executive — requires salary visibility.");
            var ivan = MakeAccount(Id_Ivan, "ivan@corp.com", "DevOps — manages order-processing pipeline.");
            var judy = MakeAccount(Id_Judy, "judy@corp.com", "Internal Auditor — read-only access to HR and audit.");

            // ── Elevated / Secondary AD Accounts
            var alice_adm = new Resource
            {
                Id = Id_Alice_Adm,
                Name = "alice-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Alice's privileged account for release-approval workflows.",
                Status = "Active",
                Tags = T("tier:identity", "kind:human", "elevated-account")
            };
            var carol_adm = new Resource
            {
                Id = Id_Carol_Adm,
                Name = "carol-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Carol's DBA admin account. Member of AD\\DB-Admins. " +
                              "Never used for browsing — only for DBA operations.",
                Status = "Active",
                Tags = T("tier:identity", "kind:human", "elevated-account")
            };
            var dave_adm = new Resource
            {
                Id = Id_Dave_Adm,
                Name = "dave-adm@corp.com",
                Type = ResourceType.Account,
                Description = "Dave's admin account. Member of AD\\DB-Admins for deployment scripts.",
                Status = "Active",
                Tags = T("tier:identity", "kind:human", "elevated-account")
            };

            // ── AD Service Accounts
            var svc_webapp = new Resource
            {
                Id = Id_SvcAcct_WebApp,
                Name = "svc-webapp@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for CustomerPortal runtime. " +
                                 "Delegated to SQL login CORP\\svc-webapp on SQLSRV-PROD-01.",
                ContentNature = ContentNature.Dynamic,
                Status = "Active",
                Tags = T("tier:identity", "kind:svc-account")
            };
            var svc_reporting = new Resource
            {
                Id = Id_SvcAcct_Reporting,
                Name = "svc-reporting@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for the reporting service.",
                Status = "Active",
                Tags = T("tier:identity", "kind:svc-account")
            };
            var svc_hrapp = new Resource
            {
                Id = Id_SvcAcct_HrApp,
                Name = "svc-hrapp@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for HRPortal. " +
                              "Delegated to SQL login CORP\\svc-hrapp on SQLSRV-PROD-02.",
                Status = "Active",
                Tags = T("tier:identity", "kind:svc-account")
            };
            var svc_orders = new Resource
            {
                Id = Id_SvcAcct_Orders,
                Name = "svc-orders@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account for the order-processing microservice.",
                Status = "Active",
                Tags = T("tier:identity", "kind:svc-account")
            };

            // ── AD Security Groups
            var grp_webAppUsers = MakeGroup(Id_Grp_WebAppUsers, "AD\\WebApp-Users",
                "Members may call CustomerPortal endpoints.");
            var grp_dbReaders = MakeGroup(Id_Grp_DbReaders, "AD\\DB-Readers",
                "Direct DB read group. Mapped to SQL db-readers-role. BYPASS PATH.",
                "bypass-risk");
            var grp_dbAdmins = MakeGroup(Id_Grp_DbAdmins, "AD\\DB-Admins",
                "DBA administrators.");
            var grp_hrPortalUsers = MakeGroup(Id_Grp_HrPortalUsers, "AD\\HRPortal-Users",
                "Members may call HRPortal employee endpoints.");
            var grp_auditors = MakeGroup(Id_Grp_Auditors, "AD\\Auditors",
                "Internal auditors. Read-only on HR and audit logs.");
            var grp_dataAccess = MakeGroup(Id_Grp_DataAccess, "AD\\Data-Access",
                "GROUP-OF-GROUPS: wraps AD\\DB-Readers transitively.",
                "group-of-groups");
            var grp_executives = MakeGroup(Id_Grp_Executives, "AD\\Executives",
                "Executive leadership. Can view salary data.");
            var grp_devops = MakeGroup(Id_Grp_DevOps, "AD\\DevOps",
                "DevOps engineers. May post orders.");

            // ─────────────────────────────────────────────────────────────────
            // SECTION 2 — SQL INFRASTRUCTURE
            // ─────────────────────────────────────────────────────────────────

            var sqlSrv_prod01 = new Resource
            {
                Id = Id_SqlSrv_Prod01,
                Name = "SQLSRV-PROD-01",
                Type = ResourceType.VirtualMachine,
                Description = "Primary SQL Server. Hosts CustomerData, Orders, CountryRef.",
                Status = "Active",
                Tags = T("tier:database", "kind:sql-server", "env:prod")
            };
            var sqlSrv_prod02 = new Resource
            {
                Id = Id_SqlSrv_Prod02,
                Name = "SQLSRV-PROD-02",
                Type = ResourceType.VirtualMachine,
                Description = "Secondary SQL Server. Hosts HRData and AuditLog. Stricter network rules.",
                Status = "Active",
                Tags = T("tier:database", "kind:sql-server", "env:prod")
            };

            // Master databases — server-level logins and roles live here
            var db_master_prod01 = MakeDatabase(Id_Db_Master_Prod01, "master (PROD-01)",
                "System database on SQLSRV-PROD-01. Server logins and server roles.",
                SensitivityClassification.Confidential,
                "kind:database", "system-database");
            var db_master_prod02 = MakeDatabase(Id_Db_Master_Prod02, "master (PROD-02)",
                "System database on SQLSRV-PROD-02. Server logins and server roles.",
                SensitivityClassification.Confidential,
                "kind:database", "system-database");

            // User databases
            var db_customerData = MakeDatabase(Id_Db_CustomerData, "CustomerData",
                "Customer PII database. Customers, Payments, Addresses.",
                SensitivityClassification.Restricted);
            var db_orders = MakeDatabase(Id_Db_Orders, "OrdersDB",
                "Transactional order processing database.",
                SensitivityClassification.Confidential);
            var db_countryRef = MakeDatabase(Id_Db_CountryRef, "CountryRef",
                "ISO country lookup database. Public reference data.",
                SensitivityClassification.None);
            var db_hrData = MakeDatabase(Id_Db_HRData, "HRData",
                "Human resources database on SQLSRV-PROD-02. Employee PII and salary data.",
                SensitivityClassification.TopSecret);
            var db_auditLog = MakeDatabase(Id_Db_AuditLog, "AuditLog",
                "Centralised audit logging database. Tamper-evident.",
                SensitivityClassification.Restricted);

            // ── Tables  — ResourceType.Table ──────────────────────────────────
            var tbl_customers = MakeTable(Id_Tbl_Customers, "dbo.Customers",
                "Core customer table: id, name, email, phone, country_code. PII.",
                SensitivityClassification.Restricted);
            var tbl_payments = MakeTable(Id_Tbl_Payments, "dbo.Payments",
                "Payment instruments: card_last4, card_token, billing_zip. PCI-DSS.",
                SensitivityClassification.TopSecret);
            var tbl_addresses = MakeTable(Id_Tbl_Addresses, "dbo.Addresses",
                "Customer shipping addresses.",
                SensitivityClassification.Confidential);
            var tbl_orders = MakeTable(Id_Tbl_Orders, "dbo.Orders",
                "Order header: id, customer_id, status, total_amount.",
                SensitivityClassification.Confidential);
            var tbl_orderItems = MakeTable(Id_Tbl_OrderItems, "dbo.OrderItems",
                "Order line items: product_id, qty, unit_price.",
                SensitivityClassification.Internal);
            var tbl_countries = MakeTable(Id_Tbl_Countries, "dbo.Countries",
                "ISO 3166 country lookup. Public data.",
                SensitivityClassification.None);
            var tbl_employees = MakeTable(Id_Tbl_Employees, "hr.Employees",
                "Employee master: id, name, email, job_title, department, hire_date.",
                SensitivityClassification.TopSecret);
            var tbl_salaries = MakeTable(Id_Tbl_Salaries, "hr.Salaries",
                "Salary records: base_salary, bonus, effective_date.",
                SensitivityClassification.TopSecret);
            var tbl_accessLog = MakeTable(Id_Tbl_AccessLog, "audit.AccessLog",
                "Security access events: principal, resource, action, outcome.",
                SensitivityClassification.Restricted);

            // ── Sensitive Rows  (individual records with elevated sensitivity)
            var row_customerSSN = new Resource
            {
                Id = Id_Row_CustomerSSN,
                Name = "dbo.Customers#Row:SSN_Field",
                Type = ResourceType.File,
                Description = "A customer row where the SSN column is populated (KYC cohort). " +
                              "TopSecret — higher than the parent table (Restricted).",
                ContentAccessModel = ContentAccessModel.ResourceIsContent,
                ContentNature = ContentNature.Static,
                ContentSchemaDescription = "{ customer_id, ssn_encrypted, ssn_last4, kyc_date }",
                Sensitivity = SensitivityClassification.TopSecret,
                Status = "Active",
                Tags = T("tier:database", "kind:row"),
            };
            var row_salaryRecord = new Resource
            {
                Id = Id_Row_SalaryRecord,
                Name = "hr.Salaries#Row:Executive",
                Type = ResourceType.File,
                Description = "Executive compensation row. Salary + LTIP + bonus breakdown.",
                ContentAccessModel = ContentAccessModel.ResourceIsContent,
                ContentNature = ContentNature.Static,
                ContentSchemaDescription = "{ employee_id, base_salary, bonus, ltip_units, effective_date }",
                Sensitivity = SensitivityClassification.TopSecret,
                Status = "Active",
                Tags = T("tier:database", "kind:row", "executive-compensation")
            };

            // ─────────────────────────────────────────────────────────────────
            // SECTION 3 — SQL IAM LAYER
            // ─────────────────────────────────────────────────────────────────

            // Server Roles
            var srvRole_sysadmin = MakeRole(Id_SrvRole_Sysadmin, "SQL\\sysadmin",
                "Fixed server role. Full instance control.",
                "tier:database", "kind:server-role");
            var srvRole_dbCreator = MakeRole(Id_SrvRole_DbCreator, "SQL\\dbcreator",
                "Fixed server role. Can create/drop databases.",
                "tier:database", "kind:server-role");
            var srvRole_securityAdmin = MakeRole(Id_SrvRole_SecurityAdmin, "SQL\\securityadmin",
                "Fixed server role. Can manage server logins.",
                "tier:database", "kind:server-role");

            // Server Logins
            var login_sa = new Resource
            {
                Id = Id_Login_SA,
                Name = "SQL\\sa",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL Server admin login. Should be disabled in production.",
                Status = "Disabled",
                Tags = T("tier:database", "kind:server-login", "native-sql-login", "high-privilege")
            };
            var login_webapp = new Resource
            {
                Id = Id_Login_WebApp,
                Name = "CORP\\svc-webapp",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login. Delegates from svc-webapp@corp.com.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "ad-login")
            };
            var login_reporting = new Resource
            {
                Id = Id_Login_Reporting,
                Name = "CORP\\DB-Readers",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth group login mapped from AD\\DB-Readers. " +
                              "All group members authenticate under this login. BYPASS PATH.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "ad-group-login", "bypass-risk")
            };
            var login_hrapp = new Resource
            {
                Id = Id_Login_HrApp,
                Name = "CORP\\svc-hrapp",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth SQL login for HRPortal on SQLSRV-PROD-02.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "ad-login")
            };
            var login_countryRef = new Resource
            {
                Id = Id_Login_CountryRef,
                Name = "SQL\\svc-countryref-ro",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL login. Read-only on CountryRef only. Vault-rotated.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "native-sql-login", "read-only")
            };
            var login_carolDba = new Resource
            {
                Id = Id_Login_CarolDba,
                Name = "CORP\\carol-adm",
                Type = ResourceType.ServiceAccount,
                Description = "Windows-auth DBA login for carol-adm@corp.com. " +
                              "SecurityAdmin + DbCreator server roles.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "ad-login", "high-privilege")
            };
            var login_orders = new Resource
            {
                Id = Id_Login_Orders,
                Name = "SQL\\svc-orders",
                Type = ResourceType.ServiceAccount,
                Description = "Native SQL login for order-processing service. OrdersDB only.",
                Status = "Active",
                Tags = T("tier:database", "kind:server-login", "native-sql-login")
            };

            // Database Roles
            var dbRole_custDataReader = MakeRole(Id_DbRole_CustDataReader,
                "CustomerData\\db-readers-role",
                "Custom DB role. SELECT on dbo.Customers + dbo.Addresses. " +
                "Mapped from AD\\DB-Readers via group login.",
                "tier:database", "kind:db-role");
            var dbRole_custDataWriter = MakeRole(Id_DbRole_CustDataWriter,
                "CustomerData\\db-writers-role",
                "Custom DB role. INSERT/UPDATE on dbo.Customers. No active members.",
                "tier:database", "kind:db-role");
            var dbRole_ordersReadWrite = MakeRole(Id_DbRole_OrdersReadWrite,
                "OrdersDB\\orders-rw-role",
                "Custom DB role. READ and WRITE on Orders and OrderItems.",
                "tier:database", "kind:db-role");
            var dbRole_hrReadOnly = MakeRole(Id_DbRole_HrReadOnly,
                "HRData\\hr-readonly-role",
                "Custom DB role. SELECT on hr.Employees only.",
                "tier:database", "kind:db-role");
            var dbRole_hrAdmin = MakeRole(Id_DbRole_HrAdmin,
                "HRData\\hr-admin-role",
                "Custom DB role. Full READ/WRITE on Employees and Salaries.",
                "tier:database", "kind:db-role");

            // Database Users
            var dbUser_webapp = MakeDbUser(Id_DbUser_WebApp,
                "CustomerData\\db_user:svc-webapp",
                "DB user in CustomerData for CORP\\svc-webapp. " +
                "Explicit GRANT on Customers, Addresses, Payments (SELECT).");
            var dbUser_reporting = MakeDbUser(Id_DbUser_Reporting,
                "CustomerData\\db_user:DB-Readers",
                "DB user for the AD group login CORP\\DB-Readers. Member of db-readers-role.");
            var dbUser_hrapp = MakeDbUser(Id_DbUser_HrApp,
                "HRData\\db_user:svc-hrapp",
                "DB user in HRData for CORP\\svc-hrapp. Member of hr-admin-role.");
            var dbUser_countryRef = MakeDbUser(Id_DbUser_CountryRef,
                "CountryRef\\db_user:svc-countryref",
                "DB user in CountryRef. Single explicit GRANT: Countries(SELECT).");
            var dbUser_orders = MakeDbUser(Id_DbUser_Orders,
                "OrdersDB\\db_user:svc-orders",
                "DB user in OrdersDB for SQL\\svc-orders. Member of orders-rw-role.");
            var dbUser_auditUser = MakeDbUser(Id_DbUser_AuditUser,
                "AuditLog\\db_user:auditors",
                "DB user in AuditLog. SELECT on audit.AccessLog only.");

            // ─────────────────────────────────────────────────────────────────
            // SECTION 4 — WEB APPLICATIONS AND ENDPOINTS
            // ─────────────────────────────────────────────────────────────────

            var webApp_customerPortal = new Resource
            {
                Id = Id_WebApp_CustomerPortal,
                Name = "CustomerPortal",
                Type = ResourceType.BusinessApp,
                Description = "Customer-facing web application. Uses svc-webapp for DB calls.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active",
                Tags = T("tier:web", "kind:webapp", "env:prod")
            };
            var webApp_hrPortal = new Resource
            {
                Id = Id_WebApp_HrPortal,
                Name = "HRPortal",
                Type = ResourceType.BusinessApp,
                Description = "Internal HR portal. Corporate network only.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active",
                Tags = T("tier:web", "kind:webapp", "env:prod", "internal-only")
            };

            var ep_getCustomers = MakeEndpoint(Id_Ep_GetCustomers, "GET /api/customers",
                "Returns paginated customer list with country enrichment.");
            var ep_getCustomerById = MakeEndpoint(Id_Ep_GetCustomerById, "GET /api/customers/{id}",
                "Returns single customer including payment summary. Exposes card_last4.");
            var ep_postOrder = MakeEndpoint(Id_Ep_PostOrder, "POST /api/orders",
                "Creates an order. Inserts into Orders and OrderItems.",
                "write-endpoint");
            var ep_getEmployees = MakeEndpoint(Id_Ep_GetEmployees, "GET /api/hr/employees",
                "Employee directory — no salary data.");
            var ep_getSalaries = MakeEndpoint(Id_Ep_GetSalaries, "GET /api/hr/salaries",
                "Salary report. TopSecret — executives only.",
                "restricted-endpoint");

            // ── Register all resources ─────────────────────────────────────────
            ctx.Resources.AddRange(new[]
            {
                // Identity
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

            // ─────────────────────────────────────────────────────────────────
            // SECTION 5 — STRUCTURAL RELATIONSHIPS
            // ─────────────────────────────────────────────────────────────────

            ctx.Relationships.AddRange(new[]
            {
                // AD hosts all identities
                Rel(Id_AD, Id_Alice,             RelationshipType.HostedIn),
                Rel(Id_AD, Id_Bob,               RelationshipType.HostedIn),
                Rel(Id_AD, Id_Carol,             RelationshipType.HostedIn),
                Rel(Id_AD, Id_Dave,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Eve,               RelationshipType.HostedIn),
                Rel(Id_AD, Id_Frank,             RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grace,             RelationshipType.HostedIn),
                Rel(Id_AD, Id_Harry,             RelationshipType.HostedIn),
                Rel(Id_AD, Id_Ivan,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Judy,              RelationshipType.HostedIn),
                Rel(Id_AD, Id_Alice_Adm,         RelationshipType.HostedIn,
                    "alice-adm@ secondary account for alice@"),
                Rel(Id_AD, Id_Carol_Adm,         RelationshipType.HostedIn,
                    "carol-adm@ elevated account for carol@"),
                Rel(Id_AD, Id_Dave_Adm,          RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_WebApp,    RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_Reporting, RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_HrApp,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_SvcAcct_Orders,    RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_WebAppUsers,   RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DbReaders,     RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DbAdmins,      RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_HrPortalUsers, RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_Auditors,      RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DataAccess,    RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_Executives,    RelationshipType.HostedIn),
                Rel(Id_AD, Id_Grp_DevOps,        RelationshipType.HostedIn),
 
                // SQLSRV-PROD-01 → master DB → logins, server roles
                Rel(Id_SqlSrv_Prod01, Id_Db_Master_Prod01, RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod01, Id_Db_CustomerData,  RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod01, Id_Db_Orders,        RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod01, Id_Db_CountryRef,    RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_SrvRole_Sysadmin,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_SrvRole_DbCreator,     RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_SrvRole_SecurityAdmin, RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_SA,              RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_WebApp,          RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_Reporting,       RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_CountryRef,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_CarolDba,        RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod01, Id_Login_Orders,          RelationshipType.HostedIn),
 
                // SQLSRV-PROD-02 → master DB → hrapp login
                Rel(Id_SqlSrv_Prod02, Id_Db_Master_Prod02, RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod02, Id_Db_HRData,        RelationshipType.HostedIn),
                Rel(Id_SqlSrv_Prod02, Id_Db_AuditLog,      RelationshipType.HostedIn),
                Rel(Id_Db_Master_Prod02, Id_Login_HrApp,   RelationshipType.HostedIn),
 
                // Tables inside their databases
                Rel(Id_Db_CustomerData, Id_Tbl_Customers,  RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_Tbl_Payments,   RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_Tbl_Addresses,  RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_Tbl_Orders,     RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_Tbl_OrderItems, RelationshipType.HostedIn),
                Rel(Id_Db_CountryRef,   Id_Tbl_Countries,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_Tbl_Employees,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_Tbl_Salaries,   RelationshipType.HostedIn),
                Rel(Id_Db_AuditLog,     Id_Tbl_AccessLog,  RelationshipType.HostedIn),
 
                // Database roles inside their database
                Rel(Id_Db_CustomerData, Id_DbRole_CustDataReader,  RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_DbRole_CustDataWriter,  RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_DbRole_OrdersReadWrite, RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbRole_HrReadOnly,      RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbRole_HrAdmin,         RelationshipType.HostedIn),
 
                // Database users inside their database
                Rel(Id_Db_CustomerData, Id_DbUser_WebApp,     RelationshipType.HostedIn),
                Rel(Id_Db_CustomerData, Id_DbUser_Reporting,  RelationshipType.HostedIn),
                Rel(Id_Db_HRData,       Id_DbUser_HrApp,      RelationshipType.HostedIn),
                Rel(Id_Db_CountryRef,   Id_DbUser_CountryRef, RelationshipType.HostedIn),
                Rel(Id_Db_Orders,       Id_DbUser_Orders,     RelationshipType.HostedIn),
                Rel(Id_Db_AuditLog,     Id_DbUser_AuditUser,  RelationshipType.HostedIn),
 
                // DB users depend on their server login
                Rel(Id_DbUser_WebApp,     Id_Login_WebApp,     RelationshipType.DependsOn,
                    "CustomerData\\svc-webapp user ← CORP\\svc-webapp login"),
                Rel(Id_DbUser_Reporting,  Id_Login_Reporting,  RelationshipType.DependsOn,
                    "CustomerData\\DB-Readers user ← CORP\\DB-Readers group login"),
                Rel(Id_DbUser_HrApp,      Id_Login_HrApp,      RelationshipType.DependsOn),
                Rel(Id_DbUser_CountryRef, Id_Login_CountryRef, RelationshipType.DependsOn),
                Rel(Id_DbUser_Orders,     Id_Login_Orders,     RelationshipType.DependsOn),
 
                // Endpoints inside their web app
                Rel(Id_WebApp_CustomerPortal, Id_Ep_GetCustomers,    RelationshipType.HostedIn),
                Rel(Id_WebApp_CustomerPortal, Id_Ep_GetCustomerById, RelationshipType.HostedIn),
                Rel(Id_WebApp_CustomerPortal, Id_Ep_PostOrder,       RelationshipType.HostedIn),
                Rel(Id_WebApp_HrPortal,       Id_Ep_GetEmployees,    RelationshipType.HostedIn),
                Rel(Id_WebApp_HrPortal,       Id_Ep_GetSalaries,     RelationshipType.HostedIn),
 
                // Web apps use AD service accounts
                Rel(Id_WebApp_CustomerPortal, Id_SvcAcct_WebApp, RelationshipType.UsesIdentity,
                    "CustomerPortal authenticates to SQL as svc-webapp@corp.com"),
                Rel(Id_WebApp_HrPortal,       Id_SvcAcct_HrApp,  RelationshipType.UsesIdentity,
                    "HRPortal authenticates to SQL as svc-hrapp@corp.com"),
 
                // SQL logins use AD identities
                Rel(Id_Login_WebApp,    Id_SvcAcct_WebApp,  RelationshipType.UsesIdentity,
                    "CORP\\svc-webapp login delegates from svc-webapp@corp.com"),
                Rel(Id_Login_Reporting, Id_Grp_DbReaders,   RelationshipType.UsesIdentity,
                    "CORP\\DB-Readers group login delegates from AD\\DB-Readers"),
                Rel(Id_Login_HrApp,     Id_SvcAcct_HrApp,   RelationshipType.UsesIdentity),
                Rel(Id_Login_CarolDba,  Id_Carol_Adm,       RelationshipType.UsesIdentity,
                    "CORP\\carol-adm login delegates from carol-adm@corp.com")
            });

            // ─────────────────────────────────────────────────────────────────
            // SECTION 6 — GROUP MEMBERSHIPS
            // ─────────────────────────────────────────────────────────────────

            ctx.Memberships.AddRange(new[]
            {
                // AD\WebApp-Users
                Membership(Id_Grp_WebAppUsers, Id_Alice, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Frank, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Grace, "Member"),
                Membership(Id_Grp_WebAppUsers, Id_Bob,   "Member"),
 
                // AD\DB-Readers — BYPASS PATH
                Membership(Id_Grp_DbReaders, Id_Bob,   "Member"),
                Membership(Id_Grp_DbReaders, Id_Grace, "Member"),
                Membership(Id_Grp_DbReaders, Id_Judy,  "Member"),
 
                // AD\DB-Admins
                Membership(Id_Grp_DbAdmins, Id_Carol_Adm, "Member"),
                Membership(Id_Grp_DbAdmins, Id_Dave_Adm,  "Member"),
 
                // AD\HRPortal-Users
                Membership(Id_Grp_HrPortalUsers, Id_Carol, "Member"),
                Membership(Id_Grp_HrPortalUsers, Id_Dave,  "Member"),
 
                // AD\Auditors
                Membership(Id_Grp_Auditors, Id_Judy, "Member"),
                Membership(Id_Grp_Auditors, Id_Eve,  "Member"),
 
                // AD\Executives
                Membership(Id_Grp_Executives, Id_Harry, "Member"),
 
                // AD\DevOps
                Membership(Id_Grp_DevOps, Id_Ivan, "Member"),
 
                // GROUP-OF-GROUPS: Data-Access contains DB-Readers
                Membership(Id_Grp_DataAccess, Id_Grp_DbReaders, "Member"),
 
                // DB role memberships
                Membership(Id_DbRole_CustDataReader,  Id_DbUser_Reporting, "Member"),
                Membership(Id_DbRole_OrdersReadWrite, Id_DbUser_Orders,    "Member"),
                Membership(Id_DbRole_HrAdmin,         Id_DbUser_HrApp,     "Member"),
                Membership(Id_DbRole_HrReadOnly,      Id_DbUser_AuditUser, "Member"),
 
                // Server role memberships
                Membership(Id_SrvRole_Sysadmin,      Id_Login_SA,       "Member"),
                Membership(Id_SrvRole_SecurityAdmin, Id_Login_CarolDba, "Member"),
                Membership(Id_SrvRole_DbCreator,     Id_Login_CarolDba, "Member")
            });

            // ─────────────────────────────────────────────────────────────────
            // SECTION 7 — ATTRIBUTE DEFINITIONS AND VALUES
            // ─────────────────────────────────────────────────────────────────

            var attrDef_linkedAdIdentity = new ResourceAttributeDefinition
            {
                Id = AttrDef_LinkedAdIdentity,
                Key = "linked_ad_identity",
                Label = "Linked AD Identity",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.ServiceAccount,
                HelpText = "The AD ServiceAccount this SQL login delegates from.",
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
                HelpText = "The database-scoped user that corresponds to this server login.",
                IsRequired = false
            };
            var attrDef_parentTable = new ResourceAttributeDefinition
            {
                Id = AttrDef_ParentTable,
                Key = "parent_table",
                Label = "Parent Table",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.Table,
                HelpText = "For row resources: the Table that contains this row. " +
                             "Used by the traversal engine to bubble sensitivity upward.",
                IsRequired = false
            };

            ctx.AttributeDefinitions.AddRange(new[]
            {
                attrDef_linkedAdIdentity, attrDef_linkedAdGroup,
                attrDef_linkedDbUser, attrDef_parentTable
            });

            // Cross-system attribute values
            AddAttrValue(ctx, Id_Login_WebApp, AttrDef_LinkedAdIdentity, Id_SvcAcct_WebApp.ToString(), attrDef_linkedAdIdentity);
            AddAttrValue(ctx, Id_Login_HrApp, AttrDef_LinkedAdIdentity, Id_SvcAcct_HrApp.ToString(), attrDef_linkedAdIdentity);
            AddAttrValue(ctx, Id_Login_CarolDba, AttrDef_LinkedAdIdentity, Id_Carol_Adm.ToString(), attrDef_linkedAdIdentity);
            AddAttrValue(ctx, Id_Login_Reporting, AttrDef_LinkedAdGroup, Id_Grp_DbReaders.ToString(), attrDef_linkedAdGroup);
            AddAttrValue(ctx, Id_DbRole_CustDataReader, AttrDef_LinkedAdGroup, Id_Grp_DbReaders.ToString(), attrDef_linkedAdGroup);
            AddAttrValue(ctx, Id_Row_CustomerSSN, AttrDef_ParentTable, Id_Tbl_Customers.ToString(), attrDef_parentTable);
            AddAttrValue(ctx, Id_Row_SalaryRecord, AttrDef_ParentTable, Id_Tbl_Salaries.ToString(), attrDef_parentTable);

            // ─────────────────────────────────────────────────────────────────
            // SECTION 8 — CAPABILITIES
            // ─────────────────────────────────────────────────────────────────

            // Endpoint capabilities
            ctx.Capabilities.AddRange(new[]
            {
                MakeCap(Cap_Ep_GetCustomers_Exec,    Id_Ep_GetCustomers,    CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/customers."),
                MakeCap(Cap_Ep_GetCustomerById_Exec, Id_Ep_GetCustomerById, CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/customers/{id} — includes payment summary."),
                MakeCap(Cap_Ep_PostOrder_Exec,       Id_Ep_PostOrder,       CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke POST /api/orders."),
                MakeCap(Cap_Ep_PostOrder_Write,      Id_Ep_PostOrder,       CapabilityType.Write,   CapabilityScope.ContentAccess,
                    "POST /api/orders write path — inserts into Orders and OrderItems."),
                MakeCap(Cap_Ep_GetEmployees_Exec,    Id_Ep_GetEmployees,    CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/hr/employees."),
                MakeCap(Cap_Ep_GetSalaries_Exec,     Id_Ep_GetSalaries,     CapabilityType.Execute, CapabilityScope.ContentAccess,
                    "Invoke GET /api/hr/salaries. TopSecret — exec only.")
            });

            // Table-level capabilities
            ctx.Capabilities.AddRange(new[]
            {
                MakeCap(Cap_Tbl_Customers_Read,   Id_Tbl_Customers, CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.Customers."),
                MakeCap(Cap_Tbl_Customers_Write,  Id_Tbl_Customers, CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT/UPDATE on dbo.Customers."),
                MakeCap(Cap_Tbl_Customers_Delete, Id_Tbl_Customers, CapabilityType.Delete, CapabilityScope.ContentAccess, "DELETE on dbo.Customers. GDPR erasure only."),
                MakeCap(Cap_Tbl_Payments_Read,    Id_Tbl_Payments,  CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.Payments. PCI-DSS."),
                MakeCap(Cap_Tbl_Addresses_Read,   Id_Tbl_Addresses, CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.Addresses."),
                MakeCap(Cap_Tbl_Addresses_Write,  Id_Tbl_Addresses, CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT/UPDATE on dbo.Addresses."),
                MakeCap(Cap_Tbl_Orders_Read,      Id_Tbl_Orders,    CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.Orders."),
                MakeCap(Cap_Tbl_Orders_Write,     Id_Tbl_Orders,    CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT/UPDATE on dbo.Orders."),
                MakeCap(Cap_Tbl_OrderItems_Read,  Id_Tbl_OrderItems,CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.OrderItems."),
                MakeCap(Cap_Tbl_OrderItems_Write, Id_Tbl_OrderItems,CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT/UPDATE on dbo.OrderItems."),
                MakeCap(Cap_Tbl_Countries_Read,   Id_Tbl_Countries, CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on dbo.Countries. Public."),
                MakeCap(Cap_Tbl_Employees_Read,   Id_Tbl_Employees, CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on hr.Employees."),
                MakeCap(Cap_Tbl_Employees_Write,  Id_Tbl_Employees, CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT/UPDATE on hr.Employees."),
                MakeCap(Cap_Tbl_Salaries_Read,    Id_Tbl_Salaries,  CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on hr.Salaries. TopSecret."),
                MakeCap(Cap_Tbl_AccessLog_Read,   Id_Tbl_AccessLog, CapabilityType.Read,   CapabilityScope.ContentAccess, "SELECT on audit.AccessLog."),
                MakeCap(Cap_Tbl_AccessLog_Write,  Id_Tbl_AccessLog, CapabilityType.Write,  CapabilityScope.ContentAccess, "INSERT on audit.AccessLog. SIEM only.")
            });

            // Admin capabilities
            ctx.Capabilities.AddRange(new[]
            {
                MakeCap(Cap_Db_CustomerData_Administer,  Id_Db_CustomerData,   CapabilityType.Administer, CapabilityScope.SelfManagement, "DBA admin of CustomerData."),
                MakeCap(Cap_Db_HRData_Administer,        Id_Db_HRData,         CapabilityType.Administer, CapabilityScope.SelfManagement, "DBA admin of HRData."),
                MakeCap(Cap_SrvRole_Sysadmin_Administer, Id_SrvRole_Sysadmin,  CapabilityType.Administer, CapabilityScope.SelfManagement, "Full server admin via sysadmin role.")
            });

            // ─────────────────────────────────────────────────────────────────
            // SECTION 9 — CAPABILITY GRANTS
            // ─────────────────────────────────────────────────────────────────

            // Endpoint grants
            ctx.Grants.AddRange(new[]
            {
                MakeGrant(Grant_WebAppUsers_GetCustomers,    Cap_Ep_GetCustomers_Exec,    Id_Grp_WebAppUsers,
                    "WebApp-Users AD group — authorised consumer of the customer list endpoint.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_WebAppUsers_GetCustomerById, Cap_Ep_GetCustomerById_Exec, Id_Grp_WebAppUsers,
                    "WebApp-Users can retrieve individual customer detail.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DevOps_PostOrder,            Cap_Ep_PostOrder_Exec,       Id_Grp_DevOps,
                    "DevOps team manages order ingestion pipeline.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_HrPortalUsers_GetEmployees,  Cap_Ep_GetEmployees_Exec,    Id_Grp_HrPortalUsers,
                    "HR portal users access the employee directory.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_Auditors_GetEmployees,       Cap_Ep_GetEmployees_Exec,    Id_Grp_Auditors,
                    "Auditors need the employee list for compliance cross-checks.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_Executives_GetSalaries,      Cap_Ep_GetSalaries_Exec,     Id_Grp_Executives,
                    "Executive leadership requires salary visibility.",
                    new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc))
            });

            // Table-level grants
            ctx.Grants.AddRange(new[]
            {
                MakeGrant(Grant_DbUser_WebApp_Cust_Read,     Cap_Tbl_Customers_Read,  Id_DbUser_WebApp,
                    "svc-webapp requires SELECT on Customers for the customer list endpoint.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_WebApp_Addr_Read,     Cap_Tbl_Addresses_Read,  Id_DbUser_WebApp,
                    "svc-webapp requires SELECT on Addresses for customer detail.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_WebApp_Pay_Read,      Cap_Tbl_Payments_Read,   Id_DbUser_WebApp,
                    "svc-webapp requires SELECT on Payments for payment summary.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbRole_CustReader_Cust_Read, Cap_Tbl_Customers_Read,  Id_DbRole_CustDataReader,
                    "db-readers-role grants SELECT on Customers to role members (incl. AD\\DB-Readers).",
                    new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Reporting_Cust_Read,  Cap_Tbl_Customers_Read,  Id_DbUser_Reporting,
                    "Reporting user has explicit SELECT on Customers in addition to role.",
                    new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_CountryRef_Countries, Cap_Tbl_Countries_Read,  Id_DbUser_CountryRef,
                    "Read-only access to ISO country lookup data.",
                    new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Orders_Read,   Cap_Tbl_Orders_Read,      Id_DbUser_Orders,
                    "svc-orders SELECT on Orders for idempotency checks.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Orders_Write,  Cap_Tbl_Orders_Write,     Id_DbUser_Orders,
                    "svc-orders INSERT/UPDATE on Orders.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Items_Read,    Cap_Tbl_OrderItems_Read,  Id_DbUser_Orders,
                    "svc-orders SELECT on OrderItems.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Orders_Items_Write,   Cap_Tbl_OrderItems_Write, Id_DbUser_Orders,
                    "svc-orders INSERT on OrderItems.",
                    new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_HrApp_Emp_Read,       Cap_Tbl_Employees_Read,  Id_DbUser_HrApp,
                    "svc-hrapp SELECT on Employees.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_HrApp_Emp_Write,      Cap_Tbl_Employees_Write, Id_DbUser_HrApp,
                    "svc-hrapp INSERT/UPDATE on Employees.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_HrApp_Sal_Read,       Cap_Tbl_Salaries_Read,   Id_DbUser_HrApp,
                    "svc-hrapp SELECT on Salaries for the salary report endpoint.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Audit_Emp_Read,       Cap_Tbl_Employees_Read,  Id_DbUser_AuditUser,
                    "Auditors need employee reference data to correlate log entries.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_DbUser_Audit_Log_Read,       Cap_Tbl_AccessLog_Read,  Id_DbUser_AuditUser,
                    "Auditors have SELECT on the audit access log.",
                    new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
 
                // BYPASS PATH — AD\DB-Readers directly on Customers(SELECT)
                MakeGrant(Grant_Grp_DbReaders_Cust_Read,     Cap_Tbl_Customers_Read,   Id_Grp_DbReaders,
                    "LEGACY BYPASS: AD\\DB-Readers inherited SELECT on Customers via group login. " +
                    "bob, grace, judy can query CustomerData directly.",
                    new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)),

                MakeGrant(Grant_Carol_Administer_CustData,   Cap_Db_CustomerData_Administer, Id_Login_CarolDba,
                    "carol-adm DBA login administers CustomerData.",
                    new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_Carol_Administer_HRData,     Cap_Db_HRData_Administer,       Id_Login_CarolDba,
                    "carol-adm DBA login administers HRData.",
                    new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
                MakeGrant(Grant_Login_SA_Sysadmin,           Cap_SrvRole_Sysadmin_Administer, Id_Login_SA,
                    "SA login has sysadmin. Should be disabled in prod.",
                    new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            });

            // ─────────────────────────────────────────────────────────────────
            // SECTION 10 — CONTENT BINDINGS
            // ─────────────────────────────────────────────────────────────────

            ctx.ContentBindings.AddRange(new[]
            {
                // GET /api/customers — 3 sources
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Customers,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Customers,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Paginated customer rows from dbo.Customers via svc-webapp.",
                    ContributionDescription = "id, name, email, phone — PII. Sensitivity: Restricted."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Addresses,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Addresses,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.SecondarySource,
                    IsActive    = true,
                    Description = "Address details joined into the customer DTO.",
                    ContributionDescription = "line1, city, postcode, country_code."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomers_Countries,
                    ConsumerResourceId = Id_Ep_GetCustomers,
                    ContentSourceId    = Id_Tbl_Countries,
                    AccessorResourceId = Id_DbUser_CountryRef,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.LookupSource,
                    IsActive    = true,
                    Description = "Country name from CountryRef via svc-countryref-ro.",
                    ContributionDescription = "countryName — display label. Non-sensitive."
                },
 
                // GET /api/customers/{id} — Customers + Payments (TopSecret path)
                new ContentBinding
                {
                    Id = Binding_GetCustomerById_Customers,
                    ConsumerResourceId = Id_Ep_GetCustomerById,
                    ContentSourceId    = Id_Tbl_Customers,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Primary customer record.",
                    ContributionDescription = "Full customer fields."
                },
                new ContentBinding
                {
                    Id = Binding_GetCustomerById_Payments,
                    ConsumerResourceId = Id_Ep_GetCustomerById,
                    ContentSourceId    = Id_Tbl_Payments,
                    AccessorResourceId = Id_DbUser_WebApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.SecondarySource,
                    IsActive    = true,
                    Description = "Payment summary. Elevates endpoint effective sensitivity to TopSecret.",
                    ContributionDescription = "card_last4, billing_zip — PCI-DSS."
                },
 
                // POST /api/orders — WriteTargets
                new ContentBinding
                {
                    Id = Binding_PostOrder_Orders,
                    ConsumerResourceId = Id_Ep_PostOrder,
                    ContentSourceId    = Id_Tbl_Orders,
                    AccessorResourceId = Id_DbUser_Orders,
                    AccessType  = ContentBindingAccessType.Write,
                    Role        = ContentBindingRole.WriteTarget,
                    IsActive    = true,
                    Description = "POST /api/orders inserts the order header into dbo.Orders.",
                    ContributionDescription = "order_id, customer_id, status, total_amount."
                },
                new ContentBinding
                {
                    Id = Binding_PostOrder_OrderItems,
                    ConsumerResourceId = Id_Ep_PostOrder,
                    ContentSourceId    = Id_Tbl_OrderItems,
                    AccessorResourceId = Id_DbUser_Orders,
                    AccessType  = ContentBindingAccessType.Write,
                    Role        = ContentBindingRole.WriteTarget,
                    IsActive    = true,
                    Description = "POST /api/orders inserts line items into dbo.OrderItems.",
                    ContributionDescription = "product_id, qty, unit_price."
                },
 
                // GET /api/hr/employees
                new ContentBinding
                {
                    Id = Binding_GetEmployees_Employees,
                    ConsumerResourceId = Id_Ep_GetEmployees,
                    ContentSourceId    = Id_Tbl_Employees,
                    AccessorResourceId = Id_DbUser_HrApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Employee directory from hr.Employees via svc-hrapp.",
                    ContributionDescription = "name, title, department, manager_id."
                },
 
                // GET /api/hr/salaries
                new ContentBinding
                {
                    Id = Binding_GetSalaries_Salaries,
                    ConsumerResourceId = Id_Ep_GetSalaries,
                    ContentSourceId    = Id_Tbl_Salaries,
                    AccessorResourceId = Id_DbUser_HrApp,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Salary report from hr.Salaries via svc-hrapp. TopSecret.",
                    ContributionDescription = "base_salary, bonus, ltip_units."
                },
 
                // Row → Table containment bindings
                new ContentBinding
                {
                    Id = Binding_Tbl_Customers_Row_SSN,
                    ConsumerResourceId = Id_Tbl_Customers,
                    ContentSourceId    = Id_Row_CustomerSSN,
                    AccessorResourceId = null,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Customers table aggregates this TopSecret SSN row. " +
                                  "Sensitivity propagates upward to the table and all endpoints that read it.",
                    ContributionDescription = "SSN row — TopSecret flag propagates to parent table."
                },
                new ContentBinding
                {
                    Id = Binding_Tbl_Salaries_Row_Salary,
                    ConsumerResourceId = Id_Tbl_Salaries,
                    ContentSourceId    = Id_Row_SalaryRecord,
                    AccessorResourceId = null,
                    AccessType  = ContentBindingAccessType.Read,
                    Role        = ContentBindingRole.PrimarySource,
                    IsActive    = true,
                    Description = "Salaries table aggregates this executive compensation row.",
                    ContributionDescription = "Executive LTIP row — TopSecret."
                }
            });

            // And separately register policies:
            ctx.Policies.Add(new ResourcePolicy
            {
                ResourceId = Id_Tbl_Payments,
                Framework = ComplianceFramework.PCIDSS,
                SubClassification = "CDE",
                EffectiveDate = DateTime.UtcNow
            });
            ctx.Policies.Add(new ResourcePolicy
            {
                ResourceId = Id_Tbl_Customers,
                Framework = ComplianceFramework.GDPR,
                SubClassification = "PersonalData",
                EffectiveDate = DateTime.UtcNow
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

            // ═════════════════════════════════════════════════════════════════
            // JSON EXPORT FOR CYTOSCAPE.JS
            // ═════════════════════════════════════════════════════════════════

            var outputPath = Path.Combine(
                AppContext.BaseDirectory, "iam-graph.json");

            AnsiConsole.MarkupLine("\n[bold cyan]═══════════════════════════════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[bold cyan]  GRAPH EXPORT — Cytoscape.js JSON[/]");
            AnsiConsole.MarkupLine("[bold cyan]═══════════════════════════════════════════════════════════[/]");

            GraphExportService.ExportToFile(ctx, outputPath);

            var fileInfo = new FileInfo(outputPath);
            AnsiConsole.MarkupLine($"\n[green]✓ Exported successfully[/]");
            AnsiConsole.MarkupLine($"  Path  : {outputPath}");
            AnsiConsole.MarkupLine($"  Size  : {fileInfo.Length:N0} bytes");
            AnsiConsole.MarkupLine($"  Nodes : {ctx.Resources.Count}");
            AnsiConsole.MarkupLine($"  Edges : {ctx.Relationships.Count + ctx.Memberships.Count + ctx.Grants.Count + ctx.ContentBindings.Count}");
            AnsiConsole.MarkupLine($"         ├─ Relationships   : {ctx.Relationships.Count}");
            AnsiConsole.MarkupLine($"         ├─ Memberships     : {ctx.Memberships.Count}");
            AnsiConsole.MarkupLine($"         ├─ Grants          : {ctx.Grants.Count}");
            AnsiConsole.MarkupLine($"         └─ ContentBindings : {ctx.ContentBindings.Count}");
            AnsiConsole.MarkupLine($"\n  Load in Cytoscape.js:");
            AnsiConsole.MarkupLine($"  [grey]const graph = await fetch('./iam-graph.json').then(r => r.json());[/]");
            AnsiConsole.MarkupLine($"  [grey]cy.add(graph.elements.nodes);[/]");
            AnsiConsole.MarkupLine($"  [grey]cy.add(graph.elements.edges);[/]");
        }

        static List<string> T(params string[] tags) => new List<string>(tags);

        static Resource MakeAccount(Guid id, string name, string desc,
             params string[] extraTags)
        {
            var tags = new List<string> { "tier:identity", "kind:human" };
            tags.AddRange(extraTags);
            return new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.Account,
                Description = desc,
                Status = "Active",
                Tags = tags
            };
        }

        static Resource MakeGroup(Guid id, string name, string desc,
            params string[] extraTags)
        {
            var tags = new List<string> { "tier:identity", "kind:ad-group" };
            tags.AddRange(extraTags);
            return new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.Group,
                Description = desc,
                Status = "Active",
                Tags = tags
            };
        }

        static Resource MakeDatabase(Guid id, string name, string desc,
            SensitivityClassification sensitivity, params string[] extraTags)
        {
            var tags = new List<string> { "tier:database", "kind:database" };
            tags.AddRange(extraTags);
            return new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.Database,
                Description = desc,
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Sensitivity = sensitivity,
                Status = "Active",
                Tags = tags
            };
        }

        /// <summary>
        /// Creates a table resource. Uses ResourceType.Table (not DataStore).
        /// ContentAccessModel = ResourceIsContent — the rows ARE the content.
        /// </summary>
        static Resource MakeTable(Guid id, string name, string desc,
            SensitivityClassification sensitivity, params string[] extraTags)
        {
            var tags = new List<string> { "tier:database", "kind:table" };
            tags.AddRange(extraTags);
            return new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.Table,
                Description = desc,
                ContentAccessModel = ContentAccessModel.ResourceIsContent,
                ContentNature = ContentNature.Dynamic,
                Sensitivity = sensitivity,
                Status = "Active",
                Tags = tags
            };
        }

        static Resource MakeRole(Guid id, string name, string desc,
            params string[] tags) =>
            new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.Role,
                Description = desc,
                Status = "Active",
                Tags = new List<string>(tags)
            };

        static Resource MakeDbUser(Guid id, string name, string desc) =>
            new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.ServiceAccount,
                Description = desc,
                Status = "Active",
                Tags = T("tier:database", "kind:db-user")
            };

        static Resource MakeEndpoint(Guid id, string name, string desc,
            params string[] extraTags)
        {
            var tags = new List<string> { "tier:web", "kind:endpoint" };
            tags.AddRange(extraTags);
            return new Resource
            {
                Id = id,
                Name = name,
                Type = ResourceType.ServiceEndpoint,
                Description = desc,
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active",
                Tags = tags
            };
        }

        static ResourceCapability MakeCap(Guid id, Guid resourceId,
            CapabilityType type, CapabilityScope scope, string desc) =>
            new ResourceCapability
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
            string justification, DateTime activatedAt) =>
            new CapabilityGrant
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
            new ResourceRelationship
            {
                Id = Guid.NewGuid(),
                ParentResourceId = parentId,
                ChildResourceId = childId,
                Type = type,
                Notes = notes
            };

        static BusinessAppMembership Membership(Guid groupId, Guid memberId,
            string role = "Member") =>
            new BusinessAppMembership
            {
                Id = Guid.NewGuid(),
                BusinessAppResourceId = groupId,
                MemberResourceId = memberId,
                Role = TryConvertToEnum<MembershipRole>(role),
                IsActive = true
            };

        static T TryConvertToEnum<T>(string value) where T : struct, Enum
        {
            // TryParse with case-insensitive option
            Enum.TryParse(value, ignoreCase: true, out T result);
            return result;
        }

        // ═════════════════════════════════════════════════════════════════════════
        // PRINT HELPERS
        // ═════════════════════════════════════════════════════════════════════════

        static void PrintGraphNeo4jStyle(AccessGraphContext ctx)
        {
            AnsiConsole.MarkupLine("\n[bold yellow]NODES[/]");
            var nodeTable = new Table().Border(TableBorder.Rounded);
            nodeTable.AddColumn("Type"); nodeTable.AddColumn("Name");
            nodeTable.AddColumn("Sensitivity"); nodeTable.AddColumn("Tags");
            foreach (var r in ctx.Resources)
                nodeTable.AddRow(r.Type.ToString(), r.Name, r.Sensitivity.ToString(),
                    string.Join(", ", r.Tags.Take(3)));
            AnsiConsole.Write(nodeTable);

            AnsiConsole.MarkupLine("\n[bold yellow]RELATIONSHIPS[/]");
            var relTable = new Table().Border(TableBorder.Rounded);
            relTable.AddColumn("Parent"); relTable.AddColumn("Type"); relTable.AddColumn("Child");
            foreach (var rel in ctx.Relationships)
            {
                var parent = ctx.FindResource(rel.ParentResourceId)?.Name ?? rel.ParentResourceId.ToString()[..8];
                var child = ctx.FindResource(rel.ChildResourceId)?.Name ?? rel.ChildResourceId.ToString()[..8];
                relTable.AddRow(parent, rel.Type.ToString(), child);
            }
            AnsiConsole.Write(relTable);

            AnsiConsole.MarkupLine("\n[bold yellow]MEMBERSHIPS[/]");
            var memTable = new Table().Border(TableBorder.Rounded);
            memTable.AddColumn("Group / Role"); memTable.AddColumn("Member"); memTable.AddColumn("Role");
            foreach (var m in ctx.Memberships)
            {
                var group = ctx.FindResource(m.BusinessAppResourceId)?.Name ?? m.BusinessAppResourceId.ToString()[..8];
                var member = ctx.FindResource(m.MemberResourceId)?.Name ?? m.MemberResourceId.ToString()[..8];
                memTable.AddRow(group, member, m.Role.ToString());
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

            AnsiConsole.MarkupLine("\n[bold yellow]GRANTS[/]");
            var grantTable = new Table().Border(TableBorder.Rounded);
            grantTable.AddColumn("Subject"); grantTable.AddColumn("Capability"); grantTable.AddColumn("Status");
            foreach (var g in ctx.Grants)
            {
                var subject = ctx.FindResource(g.SubjectResourceId)?.Name ?? g.SubjectResourceId.ToString()[..8];
                var cap = ctx.Capabilities.FirstOrDefault(c => c.Id == g.ResourceCapabilityId);
                var capDesc = cap != null ? $"{cap.Type} on {ctx.FindResource(cap.ResourceId)?.Name}" : "?";
                grantTable.AddRow(subject, capDesc, g.Status.ToString());
            }
            AnsiConsole.Write(grantTable);

            AnsiConsole.MarkupLine("\n[bold yellow]CONTENT BINDINGS[/]");
            var bindTable = new Table().Border(TableBorder.Rounded);
            bindTable.AddColumn("Source"); bindTable.AddColumn("Consumer");
            bindTable.AddColumn("Accessor"); bindTable.AddColumn("Type"); bindTable.AddColumn("Role");
            foreach (var b in ctx.ContentBindings)
            {
                var source = ctx.FindResource(b.ContentSourceId)?.Name ?? "?";
                var consumer = ctx.FindResource(b.ConsumerResourceId)?.Name ?? "?";
                var accessor = b.AccessorResourceId.HasValue
                    ? ctx.FindResource(b.AccessorResourceId.Value)?.Name ?? "?" : "—";
                bindTable.AddRow(source, consumer, accessor, b.AccessType.ToString(), b.Role.ToString());
            }
            AnsiConsole.Write(bindTable);
        }

        static void PrintCapabilityTraversal(Guid sensitiveResourceId, AccessGraphContext ctx)
        {
            var touchpoints = AccessGraphResolver.FindTouchpoints(sensitiveResourceId, ctx);
            var source = ctx.FindResource(sensitiveResourceId)!;

            if (touchpoints.Count == 0) { AnsiConsole.MarkupLine("[red]No touchpoints.[/]"); return; }

            var tree = new Tree($"[bold red]{source.Name}[/] [grey]({source.Type}) ⚡ {source.Sensitivity}[/]");
            var nodesByPath = new Dictionary<string, TreeNode>();
            var rootKey = string.Join(" -> ", touchpoints[0].PathFromSource);
            nodesByPath[rootKey] = tree.AddNode($"[bold red]{source.Name}[/]");

            foreach (var tp in touchpoints.Skip(1))
            {
                var key = string.Join(" -> ", tp.PathFromSource);
                var parentKey = string.Join(" -> ", tp.PathFromSource.Take(tp.PathFromSource.Count - 1));
                if (!nodesByPath.TryGetValue(parentKey, out var parent))
                    parent = nodesByPath[rootKey];

                var typeColor = tp.Resource.Type switch
                {
                    ResourceType.Account or ResourceType.ServiceAccount => "cyan",
                    ResourceType.Group => "yellow",
                    ResourceType.ServiceEndpoint => "magenta",
                    ResourceType.Table or ResourceType.Database => "blue",
                    _ => "white"
                };
                var node = parent.AddNode(
                    $"[{typeColor}]{tp.Resource.Name}[/] [grey]({tp.Resource.Type}) ← {tp.EdgeLabel}[/]");
                nodesByPath[key] = node;
            }
            AnsiConsole.Write(tree);

            var humans = AccessGraphResolver.FindHumanAccessors(sensitiveResourceId, ctx);
            AnsiConsole.MarkupLine($"\n[bold]Human/service endpoints: {humans.Count}[/]");
            foreach (var h in humans)
                AnsiConsole.MarkupLine($"  [cyan]•[/] {h.Resource.Type}/{h.Resource.Name}");
        }

        static void PrintRequestFlowSimulation(Guid userId, Guid endpointId,
            AccessGraphContext ctx, string scenario)
        {
            var user = ctx.FindResource(userId)!;
            var endpoint = ctx.FindResource(endpointId)!;

            AnsiConsole.MarkupLine($"\n[bold white]┌─ {scenario}[/]");

            var endpointExecCaps = ctx.Capabilities
                .Where(c => c.ResourceId == endpointId && c.IsEnabled && c.Type == CapabilityType.Execute)
                .Select(c => c.Id).ToHashSet();

            bool canExec = ctx.Grants.Any(g =>
                g.Status == GrantStatus.Active
                && endpointExecCaps.Contains(g.ResourceCapabilityId)
                && g.SubjectResourceId == userId);

            if (!canExec)
            {
                var groups = ResolveTransitiveGroups(userId, ctx);
                canExec = groups.Any(gId => ctx.Grants.Any(g =>
                    g.Status == GrantStatus.Active
                    && endpointExecCaps.Contains(g.ResourceCapabilityId)
                    && g.SubjectResourceId == gId));
            }

            if (!canExec)
            {
                AnsiConsole.MarkupLine("│  [bold red]✗ DENIED — no Execute grant on endpoint[/]");
                AnsiConsole.MarkupLine("[white]└──────────────────────────────────────[/]");
                return;
            }
            AnsiConsole.MarkupLine("│  [green]✓ Endpoint Execute grant confirmed[/]");

            var bindings = ctx.ContentBindings
                .Where(b => b.IsActive && b.ConsumerResourceId == endpointId).ToList();

            bool allOk = true;
            foreach (var b in bindings)
            {
                var src = ctx.FindResource(b.ContentSourceId)!;
                var reqCap = b.AccessType == ContentBindingAccessType.Write
                    ? CapabilityType.Write : CapabilityType.Read;
                var srcCaps = ctx.Capabilities
                    .Where(c => c.ResourceId == b.ContentSourceId && c.IsEnabled && c.Type == reqCap)
                    .Select(c => c.Id).ToHashSet();

                bool ok = false;
                if (b.AccessorResourceId.HasValue)
                {
                    var aid = b.AccessorResourceId.Value;
                    ok = ctx.Grants.Any(g => g.Status == GrantStatus.Active
                                           && srcCaps.Contains(g.ResourceCapabilityId)
                                           && g.SubjectResourceId == aid);
                    if (!ok)
                        ok = ResolveTransitiveGroups(aid, ctx).Any(gId =>
                            ctx.Grants.Any(g => g.Status == GrantStatus.Active
                                             && srcCaps.Contains(g.ResourceCapabilityId)
                                             && g.SubjectResourceId == gId));
                }

                var icon = ok ? "[green]  ✓[/]" : "[red]  ✗[/]";
                AnsiConsole.MarkupLine($"│  {icon} {b.Role,-22} {src.Name,-35} via " +
                    $"{(b.AccessorResourceId.HasValue ? ctx.FindResource(b.AccessorResourceId.Value)?.Name : "—"),-28} " +
                    $"({b.AccessType}) {src.Sensitivity}");
                if (!ok) allOk = false;
            }

            var verdict = allOk ? "[bold green]✓ ALLOWED[/]" : "[bold red]✗ RUNTIME FAILURE — accessor grant missing[/]";
            AnsiConsole.MarkupLine($"│  {verdict}");
            AnsiConsole.MarkupLine("[white]└──────────────────────────────────────[/]");
        }

        static HashSet<Guid> ResolveTransitiveGroups(Guid principalId, AccessGraphContext ctx)
        {
            var resolved = new HashSet<Guid>();
            var queue = new Queue<Guid>();
            queue.Enqueue(principalId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var m in ctx.Memberships
                    .Where(m => m.IsActive && m.MemberResourceId == current))
                {
                    if (resolved.Add(m.BusinessAppResourceId))
                        queue.Enqueue(m.BusinessAppResourceId);
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
