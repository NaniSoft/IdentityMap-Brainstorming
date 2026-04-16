using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using IdentityMap.DataModel.Helpers;
using Spectre.Console;

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

        // GET /api/customers enriches each customer row with their country name sourced from a separate CountryReference database.
        public static readonly Guid Id_SqlDb_CountryRef = new("20000000-0000-0000-0000-000000000005");
        // Read-only SQL account for the CountryReference DB (separate from svc-webapp).
        public static readonly Guid Id_SqlAcct_CountryRef = new("20000000-0000-0000-0000-000000000006");

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
        static readonly Guid Cap_SqlDb_CountryRef_Read = new("50000000-0000-0000-0000-000000000004");
        // This capability is intentionally Write — we'll use it to demonstrate the
        // CapabilityExceedsBindings anomaly (all bindings are Read-only).
        static readonly Guid Cap_Endpoint_Write = new("50000000-0000-0000-0000-000000000005");

        // Grant Ids
        static readonly Guid Grant_WebAppUsers_ExecEndpoint = new("60000000-0000-0000-0000-000000000001");
        static readonly Guid Grant_SqlAcct_ReadDb = new("60000000-0000-0000-0000-000000000002");
        static readonly Guid Grant_SqlRole_ReadDb = new("60000000-0000-0000-0000-000000000003");
        static readonly Guid Grant_CountryRef_Read = new("60000000-0000-0000-0000-000000000004");

        // ContentBinding Ids
        static readonly Guid Binding_Endpoint_CustomerData = new("70000000-0000-0000-0000-000000000001");
        // Second binding: endpoint also pulls from CountryReference DB (lookup).
        static readonly Guid Binding_Endpoint_CountryRef = new("70000000-0000-0000-0000-000000000002");

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
            // COMPOSITE DYNAMIC CONTENT: the response DTO is assembled from TWO databases:
            //   1. CustomerData DB  — primary customer rows (name, email, address)
            //   2. CountryReference DB — lookup to resolve country_code → country_name
            // This is the AccessSurface + Dynamic content model: the endpoint does not
            // IS the content; it is a gateway that assembles content from multiple sources.
            // ContentAccessModel = AccessSurface (gateway, not the data itself)
            // ContentNature      = Dynamic (response changes per request)
            // ContentSchemaDescription describes the composite output DTO.
            var endpoint = new Resource
            {
                Id = Id_Endpoint_Customers,
                Name = "GET /api/customers",
                Type = ResourceType.ServiceEndpoint,
                Description = "Returns paginated customer records. DTO is assembled from " +
                                           "CustomerData (primary) and CountryReference (lookup).",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Dynamic,
                ContentSchemaDescription = "JSON: { customers: CustomerDto[], total: int, page: int }\n" +
                                           "CustomerDto: { id, name, email, address, countryName }\n" +
                                           "  — id, name, email, address: from dbo.Customers (CustomerData)\n" +
                                           "  — countryName: resolved from dbo.Countries (CountryReference)",
                Status = "Active"
            };

            // CountryReference DB — a separate lookup database on the same SQL Server.
            // Not sensitive on its own, but it IS part of the endpoint's composite content.
            // The anomaly detector will surface it as a content source for the endpoint.
            var sqlDb_CountryRef = new Resource
            {
                Id = Id_SqlDb_CountryRef,
                Name = "CountryReference",
                Type = ResourceType.Database,
                Description = "Reference database: ISO country codes → display names. " +
                                           "Used by customer endpoints for country name resolution.",
                ContentAccessModel = ContentAccessModel.AccessSurface,
                ContentNature = ContentNature.Static,
                ContentSchemaDescription = "dbo.Countries: { country_code CHAR(2), country_name NVARCHAR(100) }",
                Sensitivity = SensitivityClassification.Internal,
                Status = "Active"
            };

            // Dedicated read-only SQL account for CountryReference.
            // Separate from svc-webapp deliberately: least-privilege — this account
            // has no access to CustomerData. The endpoint uses two different accessors.
            // This account has only a Read grant on CountryReference.
            var sqlAcct_CountryRef = new Resource
            {
                Id = Id_SqlAcct_CountryRef,
                Name = "SQL\\svc-countryref-ro",
                Type = ResourceType.ServiceAccount,
                Description = "Read-only SQL login for CountryReference DB. " +
                              "No access to CustomerData. Deliberately restricted.",
                Status = "Active"
            };

            ctx.Resources.AddRange(new[]
            {
                ad, alice, grp_webAppUsers, grp_dbReaders, svcAcct_AD,
                sqlServer, sqlDb, sqlAcct, sqlRole,
                sqlDb_CountryRef, sqlAcct_CountryRef,
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
 
                // CountryReference DB and its dedicated read-only account
                Rel(Id_SqlServer, Id_SqlDb_CountryRef,   RelationshipType.HostedIn,
                    "CountryReference DB is hosted on SQLSRV-PROD-01"),
                Rel(Id_SqlServer, Id_SqlAcct_CountryRef, RelationshipType.HostedIn,
                    "svc-countryref-ro SQL login lives on SQLSRV-PROD-01"),
 
                // ── Web App children ──────────────────────────────────────────
                Rel(Id_WebApp, Id_Endpoint_Customers, RelationshipType.HostedIn,
                    "GET /api/customers endpoint is hosted in CustomerPortal"),
 
                // ── Web App UsesIdentity ──────────────────────────────────────
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

            // CountryReference DB: ContentAccess.Read
            ctx.Capabilities.Add(new ResourceCapability
            {
                Id = Cap_SqlDb_CountryRef_Read,
                ResourceId = Id_SqlDb_CountryRef,
                Type = CapabilityType.Read,
                Scope = CapabilityScope.ContentAccess,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AnyOwner,
                Description = "Read ISO country lookup data.",
                IsEnabled = true
            });

            // ANOMALY DEMO: endpoint_customers: ContentAccess.Write
            // This capability is intentionally misconfigured to trigger the
            // CapabilityExceedsBindings anomaly: the endpoint declares it can
            // write data, but BOTH its ContentBindings have AccessType = Read.
            // No write path exists in the data flow — this is a governance gap.
            ctx.Capabilities.Add(new ResourceCapability
            {
                Id = Cap_Endpoint_Write,
                ResourceId = Id_Endpoint_Customers,
                Type = CapabilityType.Write,
                Scope = CapabilityScope.ContentAccess,
                DefaultApprovalPolicy = DefaultApprovalPolicy.AllOwners,
                Description = "Write/update customer data via the endpoint. " +
                                        "ANOMALY: endpoint has no ContentBinding with Write access.",
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

            // Grant 4: sqlAcct_CountryRef → CountryReference.Read
            // The dedicated lookup account has only Read on the reference DB.
            // It has NO grant on CustomerData — correctly least-privileged.
            ctx.Grants.Add(new CapabilityGrant
            {
                Id = Grant_CountryRef_Read,
                ResourceCapabilityId = Cap_SqlDb_CountryRef_Read,
                SubjectResourceId = Id_SqlAcct_CountryRef,
                Status = GrantStatus.Active,
                Justification = "Read-only access to ISO country reference data for display name resolution.",
                ActivatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            });

            // =================================================================
            // 8. CONTENT BINDINGS  (composite dynamic content model)
            // =================================================================
            //
            // GET /api/customers assembles its response DTO from TWO databases.
            // Each ContentBinding declares one source's contribution.
            // Together they describe the complete dynamic content surface.
            //
            // COMPOSITION MODEL:
            //   ResourceIsContent  → the resource IS the data (File, Record)
            //                        No ContentBindings needed; CapabilityGrants govern directly.
            //   AccessSurface      → the resource is a gateway to content behind it.
            //                        ContentBindings declare what's behind it.
            //                        The endpoint itself is not the data — it assembles the data.
            //
            // The endpoint's ContentSchemaDescription describes the output DTO.
            // Each ContentBinding's ContributionDescription describes what piece it contributes.
            // Together they form a complete picture of the endpoint's composite content.
            //
            // ANOMALY SEEDED:
            //   Cap_Endpoint_Write is declared on the endpoint (ContentAccess.Write).
            //   But both ContentBindings have AccessType = Read.
            //   ContentAnomalyDetector.Check1 will flag this as CapabilityExceedsBindings.

            // Binding 1: primary — customer rows from CustomerData DB
            ctx.ContentBindings.Add(new ContentBinding
            {
                Id = Binding_Endpoint_CustomerData,
                ConsumerResourceId = Id_Endpoint_Customers,
                ContentSourceId = Id_SqlDb_Customers,
                AccessorResourceId = Id_SqlAcct_WebApp,
                AccessType = ContentBindingAccessType.Read,
                Role = ContentBindingRole.PrimarySource,
                IsActive = true,
                Description = "GET /api/customers reads paginated customer rows from " +
                                          "CustomerData via SQL\\svc-webapp.",
                ContributionDescription = "id, name, email, address — from dbo.Customers. " +
                                          "This is the primary dataset for the response DTO."
            });

            // Binding 2: lookup — country names from CountryReference DB
            // This is the SECOND source that makes the content composite.
            // A different accessor account (svc-countryref-ro) with a separate,
            // scoped Read grant on the CountryReference DB — not CustomerData.
            ctx.ContentBindings.Add(new ContentBinding
            {
                Id = Binding_Endpoint_CountryRef,
                ConsumerResourceId = Id_Endpoint_Customers,
                ContentSourceId = Id_SqlDb_CountryRef,
                AccessorResourceId = Id_SqlAcct_CountryRef,
                AccessType = ContentBindingAccessType.Read,
                Role = ContentBindingRole.LookupSource,
                IsActive = true,
                Description = "GET /api/customers resolves country_code → country_name " +
                                          "from CountryReference via SQL\\svc-countryref-ro.",
                ContributionDescription = "countryName — resolved from dbo.Countries using customer's " +
                                          "country_code. Enrichment field; not primary data."
            });

            // 1. Sensitivity traversal from the Restricted database
            AccessGraphResolver.PrintSensitivityReport(Id_SqlDb_Customers, ctx);

            // 2. Composite content summary for the endpoint
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("  COMPOSITE CONTENT SUMMARY");
            Console.WriteLine("  Resource: GET /api/customers");
            Console.WriteLine(new string('=', 60));

            var bindings = ctx.ContentBindings
                .Where(b => b.IsActive && b.ConsumerResourceId == Id_Endpoint_Customers)
                .ToList();

            Console.WriteLine($"\nTotal content sources: {bindings.Count}");
            foreach (var b in bindings)
            {
                var src = ctx.FindResource(b.ContentSourceId);
                var accessor = b.AccessorResourceId.HasValue
                    ? ctx.FindResource(b.AccessorResourceId.Value)?.Name ?? "none"
                    : "none";
                Console.WriteLine(
                    $"\n  [{b.Role}]  {src?.Name} ({src?.Type})");
                Console.WriteLine(
                    $"    AccessType  : {b.AccessType}");
                Console.WriteLine(
                    $"    Accessor    : {accessor}");
                Console.WriteLine(
                    $"    Sensitivity : {src?.Sensitivity}");
                Console.WriteLine(
                    $"    Contributes : {b.ContributionDescription}");
            }

            // 3. Anomaly detection
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("  CONTENT ANOMALY REPORT");
            Console.WriteLine(new string('=', 60));

            var anomalies = ContentAnomalyDetector.DetectAll(ctx);
            if (!anomalies.Any())
            {
                Console.WriteLine("\nNo anomalies detected.");
                return;
            }

            foreach (var anomaly in anomalies)
            {
                var resource = ctx.FindResource(anomaly.ResourceId);
                Console.WriteLine($"\n  [{anomaly.Type}]");
                Console.WriteLine($"  Resource     : {resource?.Name} ({resource?.Type})");
                if (anomaly.ActualLevel.HasValue)
                    Console.WriteLine(
                        $"  Levels       : required={anomaly.RequiredLevel}  actual={anomaly.ActualLevel}");
                Console.WriteLine($"  Description  : {anomaly.Description}");
                Console.WriteLine($"  Recommend    : {anomaly.Recommendation}");
            }

            Console.WriteLine($"\nTotal anomalies: {anomalies.Count}");

            PrintGraphNeo4jStyle(ctx);
            PrintCapabilityTraversal(Id_SqlDb_Customers, ctx);
        }

        static void PrintGraphNeo4jStyle(AccessGraphContext ctx)
        {
            // Print all nodes
            AnsiConsole.MarkupLine("[bold yellow]NODES[/]");
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Id");
            table.AddColumn("Type");
            table.AddColumn("Name");
            table.AddColumn("Sensitivity");
            foreach (var r in ctx.Resources)
            {
                table.AddRow(
                    r.Id.ToString(),
                    r.Type.ToString(),
                    r.Name,
                    r.Sensitivity.ToString()
                );
            }
            AnsiConsole.Write(table);

            // Print all relationships
            AnsiConsole.MarkupLine("\n[bold yellow]RELATIONSHIPS[/]");
            var relTable = new Table().Border(TableBorder.Rounded);
            relTable.AddColumn("Parent");
            relTable.AddColumn("Type");
            relTable.AddColumn("Child");
            relTable.AddColumn("Notes");
            foreach (var rel in ctx.Relationships)
            {
                var parent = ctx.FindResource(rel.ParentResourceId)?.Name ?? rel.ParentResourceId.ToString();
                var child = ctx.FindResource(rel.ChildResourceId)?.Name ?? rel.ChildResourceId.ToString();
                relTable.AddRow(parent, rel.Type.ToString(), child, rel.Notes ?? "");
            }
            AnsiConsole.Write(relTable);

            // Print all memberships
            AnsiConsole.MarkupLine("\n[bold yellow]MEMBERSHIPS[/]");
            var memTable = new Table().Border(TableBorder.Rounded);
            memTable.AddColumn("Group");
            memTable.AddColumn("Member");
            memTable.AddColumn("Role");
            foreach (var m in ctx.Memberships)
            {
                var group = ctx.FindResource(m.BusinessAppResourceId)?.Name ?? m.BusinessAppResourceId.ToString();
                var member = ctx.FindResource(m.MemberResourceId)?.Name ?? m.MemberResourceId.ToString();
                memTable.AddRow(group, member, m.Role);
            }
            AnsiConsole.Write(memTable);

            // Print all capabilities
            AnsiConsole.MarkupLine("\n[bold yellow]CAPABILITIES[/]");
            var capTable = new Table().Border(TableBorder.Rounded);
            capTable.AddColumn("Resource");
            capTable.AddColumn("Type");
            capTable.AddColumn("Scope");
            capTable.AddColumn("Description");
            foreach (var cap in ctx.Capabilities)
            {
                var resource = ctx.FindResource(cap.ResourceId)?.Name ?? cap.ResourceId.ToString();
                capTable.AddRow(resource, cap.Type.ToString(), cap.Scope.ToString(), cap.Description ?? "");
            }
            AnsiConsole.Write(capTable);

            // Print all grants
            AnsiConsole.MarkupLine("\n[bold yellow]GRANTS[/]");
            var grantTable = new Table().Border(TableBorder.Rounded);
            grantTable.AddColumn("Subject");
            grantTable.AddColumn("Capability");
            grantTable.AddColumn("Status");
            grantTable.AddColumn("Justification");
            foreach (var g in ctx.Grants)
            {
                var subject = ctx.FindResource(g.SubjectResourceId)?.Name ?? g.SubjectResourceId.ToString();
                var cap = ctx.Capabilities.FirstOrDefault(c => c.Id == g.ResourceCapabilityId);
                var capDesc = cap != null ? $"{cap.Type} on {ctx.FindResource(cap.ResourceId)?.Name}" : g.ResourceCapabilityId.ToString();
                grantTable.AddRow(subject, capDesc, g.Status.ToString(), g.Justification ?? "");
            }
            AnsiConsole.Write(grantTable);

            // Print all content bindings
            AnsiConsole.MarkupLine("\n[bold yellow]CONTENT BINDINGS[/]");
            var bindingTable = new Table().Border(TableBorder.Rounded);
            bindingTable.AddColumn("Source");
            bindingTable.AddColumn("Consumer");
            bindingTable.AddColumn("Accessor");
            bindingTable.AddColumn("AccessType");
            bindingTable.AddColumn("IsActive");
            bindingTable.AddColumn("Description");
            foreach (var b in ctx.ContentBindings)
            {
                var source = ctx.FindResource(b.ContentSourceId)?.Name ?? b.ContentSourceId.ToString();
                var consumer = ctx.FindResource(b.ConsumerResourceId)?.Name ?? b.ConsumerResourceId.ToString();
                var accessor = b.AccessorResourceId.HasValue
                    ? ctx.FindResource(b.AccessorResourceId.Value)?.Name ?? b.AccessorResourceId.ToString()
                    : "";
                bindingTable.AddRow(
                    source,
                    consumer,
                    accessor,
                    b.AccessType.ToString(),
                    b.IsActive ? "Yes" : "No",
                    b.Description ?? ""
                );
            }
            AnsiConsole.Write(bindingTable);
        }

        static void PrintCapabilityTraversal(Guid sensitiveResourceId, AccessGraphContext ctx)
        {
            var touchpoints = AccessGraphResolver.FindTouchpoints(sensitiveResourceId, ctx);
            AnsiConsole.MarkupLine("\n[bold yellow]CAPABILITY TRAVERSAL FROM SENSITIVE RESOURCE[/]");

            if (touchpoints.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No touchpoints found.[/]");
                return;
            }

            // Create a tree and use a map keyed by the full path string so we can
            // deterministically attach nodes to their parent nodes.
            var tree = new Tree("[bold red]Access Traversal[/]");
            var nodesByPath = new Dictionary<string, TreeNode>();

            // Root touchpoint (the sensitive source)
            var rootTp = touchpoints[0];
            var rootPathKey = string.Join(" -> ", rootTp.PathFromSource);
            var rootNode = tree.AddNode($"[bold red]{rootTp.Resource.Name}[/] [grey]({rootTp.Resource.Type})[/] [yellow]Sensitivity: {rootTp.EffectiveSensitivity}[/]");
            nodesByPath[rootPathKey] = rootNode;

            // Add the rest of the discovered touchpoints, attaching each under the
            // node representing its parent path in the traversal.
            foreach (var tp in touchpoints.Skip(1))
            {
                var pathKey = string.Join(" -> ", tp.PathFromSource);
                var parentPathKey = string.Join(" -> ", tp.PathFromSource.Take(tp.PathFromSource.Count - 1));

                if (!nodesByPath.TryGetValue(parentPathKey, out var parentNode))
                {
                    // Fallback to root if parent isn't found (defensive).
                    parentNode = rootNode;
                }

                var node = parentNode.AddNode($"[white]{tp.Resource.Name}[/] [grey]({tp.Resource.Type})[/] [yellow]← {tp.EdgeLabel}[/] [red]Depth:{tp.Depth}[/]");
                nodesByPath[pathKey] = node;
            }

            AnsiConsole.Write(tree);
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
