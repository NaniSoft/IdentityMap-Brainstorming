using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel
{
    internal class Program
    {
        // ── Well-known Ids ────────────────────────────────────────────────────
        // Fixed so the scenario is repeatable and readable in tests.

        // Active Directory resources
        public static readonly Guid Id_AD = new("10000000-0000-0000-0000-000000000001");
        public static readonly Guid Id_Alice = new("10000000-0000-0000-0000-000000000002");
        public static readonly Guid Id_Group_WebAppUsers = new("10000000-0000-0000-0000-000000000003");
        public static readonly Guid Id_Group_DbReaders = new("10000000-0000-0000-0000-000000000004");
        public static readonly Guid Id_SvcAcct_WebApp_AD = new("10000000-0000-0000-0000-000000000005");

        // SQL Server resources
        public static readonly Guid Id_SqlServer = new("20000000-0000-0000-0000-000000000001");
        public static readonly Guid Id_SqlDb_Customers = new("20000000-0000-0000-0000-000000000002");
        public static readonly Guid Id_SqlAcct_WebApp = new("20000000-0000-0000-0000-000000000003");
        public static readonly Guid Id_SqlRole_DbReaders = new("20000000-0000-0000-0000-000000000004");

        // Web App resources
        public static readonly Guid Id_WebApp = new("30000000-0000-0000-0000-000000000001");
        public static readonly Guid Id_Endpoint_Customers = new("30000000-0000-0000-0000-000000000002");

        // Attribute definition Ids
        static readonly Guid AttrDef_SvcAcct_LinkedAD = new("40000000-0000-0000-0000-000000000001");
        static readonly Guid AttrDef_Group_LinkedADGroup = new("40000000-0000-0000-0000-000000000002");

        // Capability Ids
        static readonly Guid Cap_Endpoint_Execute = new("50000000-0000-0000-0000-000000000001");
        static readonly Guid Cap_SqlDb_Read = new("50000000-0000-0000-0000-000000000002");
        static readonly Guid Cap_SqlDb_Administer = new("50000000-0000-0000-0000-000000000003");

        // Grant Ids
        static readonly Guid Grant_WebAppUsers_ExecEndpoint = new("60000000-0000-0000-0000-000000000001");
        static readonly Guid Grant_SqlAcct_ReadDb = new("60000000-0000-0000-0000-000000000002");
        static readonly Guid Grant_SqlRole_ReadDb = new("60000000-0000-0000-0000-000000000003");

        static void Main(string[] args)
        {
            Console.WriteLine("Creating Graph");

            var ctx = new AccessGraphContext();

            // =================================================================
            // 1. RESOURCES
            // =================================================================

            // ── Active Directory (BusinessApp = governance/identity system) ───
            //    AD is modelled as a BusinessApp resource because it acts as a
            //    group governor: it owns all child accounts and groups.
            var ad = new Resource
            {
                Id = Id_AD,
                Name = "Active Directory",
                Type = ResourceType.BusinessApp,
                Description = "Corporate identity provider. Parent of all AD accounts and groups.",
                Status = "Active"
            };

            // Alice — a human user Account whose identity lives in AD.
            // She is a member of two AD groups, giving her two paths to the DB.
            var alice = new Resource
            {
                Id = Id_Alice,
                Name = "alice@corp.com",
                Type = ResourceType.Account,
                Description = "Human user account in Active Directory.",
                Status = "Active"
            };

            // AD security group that gates access to the web endpoint.
            // Any member of this group can execute the endpoint.
            var grp_webAppUsers = new Resource
            {
                Id = Id_Group_WebAppUsers,
                Name = "AD\\WebApp-Users",
                Type = ResourceType.Group,
                Description = "AD security group. Members may invoke the customer data endpoint.",
                Status = "Active"
            };

            // AD security group that is also mapped to a SQL server role.
            // Members can read the database directly — a potential bypass path.
            var grp_dbReaders = new Resource
            {
                Id = Id_Group_DbReaders,
                Name = "AD\\DB-Readers",
                Type = ResourceType.Group,
                Description = "AD security group mapped to SQL role sqlRole_dbReaders via Windows Auth.",
                Status = "Active"
            };

            // Service account used by the web app runtime.
            // Its SQL-side twin is sqlAcct_webApp, linked via a ResourceReference attr.
            var svcAcct_AD = new Resource
            {
                Id = Id_SvcAcct_WebApp_AD,
                Name = "svc-webapp@corp.com",
                Type = ResourceType.ServiceAccount,
                Description = "AD service account — runtime identity for the web application. " +
                                  "Delegated to sqlAcct_webApp on the SQL Server.",
                ContentNature = ContentNature.Dynamic,
                Status = "Active"
            };

            // ── SQL Server (VirtualMachine — the host infrastructure) ─────────
            var sqlServer = new Resource
            {
                Id = Id_SqlServer,
                Name = "SQLSRV-PROD-01",
                Type = ResourceType.VirtualMachine,
                Description = "Primary SQL Server instance. AD-integrated via Windows Authentication.",
                Status = "Active"
            };

            // The actual database — its Sensitivity is Restricted.
            // This is the source node for sensitivity bubbling.
            // ContentAccessModel = AccessSurface because the rows are the real content;
            // the Database resource is the gateway to reach them.
            var sqlDb = new Resource
            {
                Id = Id_SqlDb_Customers,
                Name = "CustomerData",
                Type = ResourceType.Database,
                Description = "Customer PII database on SQLSRV-PROD-01. Restricted sensitivity.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                ContentSchemaDescription = "Normalised relational schema: Customers, Orders, Payments. " +
                                           "Contains PII (name, email, address, payment tokens).",
                Sensitivity = SensitivityClassification.Restricted,
                Status = "Active"
            };

            // SQL-side service account, delegated from adSvcAcct_webapp via AD auth.
            // Its 'linked_ad_identity' attribute (ResourceReference) points to svcAcct_AD.
            var sqlAcct = new Resource
            {
                Id = Id_SqlAcct_WebApp,
                Name = "SQL\\svc-webapp",
                Type = ResourceType.ServiceAccount,
                Description = "SQL Server login delegated from AD service account svc-webapp@corp.com. " +
                              "Has db_datareader on CustomerData.",
                Status = "Active"
            };

            // SQL server role mapped from adGroup_dbReaders via Windows Auth integration.
            // Its 'linked_ad_group' attribute (ResourceReference) points to grp_dbReaders.
            // Members are resolved transitively through the AD group.
            var sqlRole = new Resource
            {
                Id = Id_SqlRole_DbReaders,
                Name = "SQL\\db-readers-role",
                Type = ResourceType.Group,
                Description = "SQL Server role. Maps to AD\\DB-Readers via Windows Auth. " +
                              "Members inherit db_datareader on CustomerData.",
                Status = "Active"
            };

            // ── Web App (BusinessApp — the runtime host) ──────────────────────
            var webApp = new Resource
            {
                Id = Id_WebApp,
                Name = "CustomerPortal",
                Type = ResourceType.BusinessApp,
                Description = "Web application serving customer data endpoints. " +
                                       "Uses svc-webapp AD identity for all database calls.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                Status = "Active"
            };

            // The endpoint — protected by AD group auth (adGroup_webApp_users).
            // Its dynamic content is fetched from sqlDb_customers via the service account.
            // Note: endpoint_customers is itself content of webApp (child resource).
            // The same svcAcct_AD could be used by other endpoints in other web apps.
            var endpoint = new Resource
            {
                Id = Id_Endpoint_Customers,
                Name = "GET /api/customers",
                Type = ResourceType.ServiceEndpoint,
                Description = "Returns paginated customer records from CustomerData DB. " +
                                       "Protected by AD\\WebApp-Users role. Dynamic content.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                ContentSchemaDescription = "JSON: { customers: Customer[], total: int, page: int }",
                Status = "Active"
            };

            ctx.Resources.AddRange(new[]
            {
                ad, alice, grp_webAppUsers, grp_dbReaders, svcAcct_AD,
                sqlServer, sqlDb, sqlAcct, sqlRole,
                webApp, endpoint
            });

            // =================================================================
            // 2. STRUCTURAL RELATIONSHIPS
            // =================================================================

            // ── AD children ──────────────────────────────────────────────────
            // All AD accounts and groups are hosted within the AD resource.
            // This means AD's owners inherit governance over them if no explicit
            // owner is defined on the account/group itself.
            ctx.Relationships.AddRange(new[]
            {
                Rel(Id_AD, Id_Alice,             RelationshipType.HostedIn,
                    "Alice's account lives in Active Directory"),
                Rel(Id_AD, Id_Group_WebAppUsers, RelationshipType.HostedIn,
                    "WebApp-Users group lives in Active Directory"),
                Rel(Id_AD, Id_Group_DbReaders,   RelationshipType.HostedIn,
                    "DB-Readers group lives in Active Directory"),
                Rel(Id_AD, Id_SvcAcct_WebApp_AD, RelationshipType.HostedIn,
                    "svc-webapp service account lives in Active Directory"),

                // ── SQL Server children ───────────────────────────────────────
                Rel(Id_SqlServer, Id_SqlDb_Customers,   RelationshipType.HostedIn,
                    "CustomerData DB is hosted on SQLSRV-PROD-01"),
                Rel(Id_SqlServer, Id_SqlAcct_WebApp,    RelationshipType.HostedIn,
                    "SQL service account login lives on SQLSRV-PROD-01"),
                Rel(Id_SqlServer, Id_SqlRole_DbReaders, RelationshipType.HostedIn,
                    "SQL role db-readers-role is defined on SQLSRV-PROD-01"),

                // ── Web App children ──────────────────────────────────────────
                Rel(Id_WebApp, Id_Endpoint_Customers, RelationshipType.HostedIn,
                    "GET /api/customers endpoint is hosted in CustomerPortal"),

                // ── Web App UsesIdentity ──────────────────────────────────────
                // WebApp (Parent) uses AD service account (Child) as runtime identity.
                // This is the critical edge that connects endpoint access to DB access.
                // The same svcAcct_AD could appear as the child in additional
                // UsesIdentity relationships for other web apps or batch services.
                Rel(Id_WebApp, Id_SvcAcct_WebApp_AD, RelationshipType.UsesIdentity,
                    "CustomerPortal authenticates to SQL using svc-webapp AD identity")
            });

            // =================================================================
            // 3. AD GROUP MEMBERSHIPS
            // =================================================================
            // BusinessAppMembership is used for all Group and BusinessApp member
            // tracking — including AD groups.  This is how Alice is resolved as
            // a member of the AD groups that control access.

            ctx.Memberships.AddRange(new[]
            {
                // Alice is a member of WebApp-Users → she can Execute the endpoint
                Membership(Id_Group_WebAppUsers, Id_Alice, "Member"),

                // Alice is also a member of DB-Readers → she can Read the DB directly!
                // This is the bypass path surfaced by the sensitivity resolver.
                Membership(Id_Group_DbReaders, Id_Alice, "Member")
            });

            // =================================================================
            // 4. ATTRIBUTE DEFINITIONS
            // =================================================================
            // Only the two cross-system linkage attributes are defined here.
            // Full schemas (login_name, email, etc.) would be in ResourceTypeSchema
            // records for each ResourceType.

            var attrDef_linkedAdIdentity = new ResourceAttributeDefinition
            {
                Id = AttrDef_SvcAcct_LinkedAD,
                Key = "linked_ad_identity",
                Label = "Linked AD Identity",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.ServiceAccount,
                HelpText = "The AD ServiceAccount that this SQL account delegates from. " +
                                       "Set by DBA during Windows Auth integration.",
                IsRequired = false
            };

            var attrDef_linkedAdGroup = new ResourceAttributeDefinition
            {
                Id = AttrDef_Group_LinkedADGroup,
                Key = "linked_ad_group",
                Label = "Linked AD Group",
                DataType = AttributeDataType.ResourceReference,
                AllowedReferenceType = ResourceType.Group,
                HelpText = "The AD Group this SQL role is mapped to via Windows Authentication. " +
                                       "Members of the AD group inherit this role's permissions.",
                IsRequired = false
            };

            ctx.AttributeDefinitions.AddRange(new[]
            {
                attrDef_linkedAdIdentity,
                attrDef_linkedAdGroup
            });

            // =================================================================
            // 5. ATTRIBUTE VALUES  (AD ↔ SQL cross-system linkages)
            // =================================================================

            // sqlAcct_webApp.linked_ad_identity → adSvcAcct_webapp
            // "This SQL login IS the AD service account on the SQL side."
            ctx.AttributeValues.Add(new ResourceAttributeValue
            {
                Id = Guid.NewGuid(),
                ResourceId = Id_SqlAcct_WebApp,
                ResourceAttributeDefinitionId = AttrDef_SvcAcct_LinkedAD,
                ValueString = Id_SvcAcct_WebApp_AD.ToString(),
                AttributeDefinition = attrDef_linkedAdIdentity
            });

            // sqlRole_dbReaders.linked_ad_group → adGroup_dbReaders
            // "Membership in this SQL role = membership in the AD group DB-Readers."
            ctx.AttributeValues.Add(new ResourceAttributeValue
            {
                Id = Guid.NewGuid(),
                ResourceId = Id_SqlRole_DbReaders,
                ResourceAttributeDefinitionId = AttrDef_Group_LinkedADGroup,
                ValueString = Id_Group_DbReaders.ToString(),
                AttributeDefinition = attrDef_linkedAdGroup
            });

            // =================================================================
            // 6. CAPABILITIES
            // =================================================================

            // endpoint_customers: ContentAccess.Execute
            // The endpoint exposes its dynamic content (customer rows) to subjects
            // that have been granted Execute.  Approval requires the WebApp team.
            ctx.Capabilities.Add(new ResourceCapability
            {
                Id = Cap_Endpoint_Execute,
                ResourceId = Id_Endpoint_Customers,
                Type = CapabilityType.Execute,
                Scope = CapabilityScope.ContentAccess,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AllOwners,
                Description = "Invoke GET /api/customers and receive customer data rows.",
                IsEnabled = true
            });

            // sqlDb_customers: ContentAccess.Read
            // Direct read access to the database rows.  Granted to the SQL service
            // account AND to the SQL role (which maps to the AD group).
            ctx.Capabilities.Add(new ResourceCapability
            {
                Id = Cap_SqlDb_Read,
                ResourceId = Id_SqlDb_Customers,
                Type = CapabilityType.Read,
                Scope = CapabilityScope.ContentAccess,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AllOwners,
                Description = "Read rows from CustomerData tables. Requires DBA approval.",
                IsEnabled = true
            });

            // sqlDb_customers: SelfManagement.Administer
            // Schema changes, backups, index maintenance — DBA-only.
            ctx.Capabilities.Add(new ResourceCapability
            {
                Id = Cap_SqlDb_Administer,
                ResourceId = Id_SqlDb_Customers,
                Type = CapabilityType.Administer,
                Scope = CapabilityScope.SelfManagement,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AllOwners,
                Description = "DBA-level administration of the CustomerData database.",
                IsEnabled = true
            });

            // =================================================================
            // 7. CAPABILITY GRANTS  (active entitlements)
            // =================================================================

            // Grant 1: adGroup_webApp_users → endpoint_customers.Execute
            // The AD group IS the subject.  Any member of the group (Alice, others)
            // can call the endpoint.  This grant was approved by the WebApp team.
            ctx.Grants.Add(new CapabilityGrant
            {
                Id = Grant_WebAppUsers_ExecEndpoint,
                ResourceCapabilityId = Cap_Endpoint_Execute,
                SubjectResourceId = Id_Group_WebAppUsers,
                Status = GrantStatus.Active,
                Justification = "WebApp-Users AD group is the authorised consumer of the customer data endpoint.",
                ActivatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            });

            // Grant 2: sqlAcct_webApp → sqlDb_customers.Read
            // The SQL service account (delegated from the AD svc account) reads
            // customer data to serve web requests.
            ctx.Grants.Add(new CapabilityGrant
            {
                Id = Grant_SqlAcct_ReadDb,
                ResourceCapabilityId = Cap_SqlDb_Read,
                SubjectResourceId = Id_SqlAcct_WebApp,
                Status = GrantStatus.Active,
                Justification = "Web application service account requires read access to serve customer data.",
                ActivatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            });

            // Grant 3: sqlRole_dbReaders → sqlDb_customers.Read
            // The SQL role (mapped from AD group DB-Readers) gives direct DB read
            // access to all group members — including Alice.
            // This is a BYPASS path: Alice can query the DB directly, not just
            // through the endpoint.  The sensitivity report surfaces this.
            ctx.Grants.Add(new CapabilityGrant
            {
                Id = Grant_SqlRole_ReadDb,
                ResourceCapabilityId = Cap_SqlDb_Read,
                SubjectResourceId = Id_SqlRole_DbReaders,
                Status = GrantStatus.Active,
                Justification = "DB-Readers AD group mapped to this SQL role for legacy reporting access.",
                ActivatedAt = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            AccessGraphResolver.PrintSensitivityReport(Id_SqlDb_Customers, ctx);

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
    }
}
