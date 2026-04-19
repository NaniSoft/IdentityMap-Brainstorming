using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    // ═══════════════════════════════════════════════════════════════════════════
    // GraphExportService
    //
    // Serialises an AccessGraphContext to a Cytoscape.js-compatible JSON file.
    //
    // Output structure
    // ────────────────
    // {
    //   "meta":     { schemaVersion, generatedAt, generator, stats }
    //   "legend":   { nodeTypes, edgeKinds, sensitivityScale, tagPrefixes }
    //   "elements": { "nodes": [...], "edges": [...] }
    // }
    //
    // Nodes
    // ─────
    // Each Resource becomes one Cytoscape node element.
    // The "data" object carries every field the frontend view-plane compiler
    // needs to assign parent containers, pick icons, and apply filters —
    // without the frontend needing to know about C# types.
    //
    // "container:*" tags are derived here from HostedIn relationships so the
    // frontend never has to traverse the edge list just to resolve grouping.
    //
    // Edges
    // ─────
    // Four edge categories, each with a distinct "edgeKind" discriminator:
    //
    //   Relationship   — structural (HostedIn, BelongsTo, DependsOn, …)
    //   Membership     — group / role membership (BusinessAppMembership)
    //   Grant          — capability entitlement (CapabilityGrant)
    //                    source = subject, target = resource the capability is on
    //   ContentBinding — data flow declaration (ContentBinding)
    //                    source = content source, target = consumer
    //
    // All IDs are lowercased GUIDs (no braces) to match typical JS UUID handling.
    // ═══════════════════════════════════════════════════════════════════════════
    public static class GraphExportService
    {
        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Serialise the context and write to <paramref name="filePath"/>.</summary>
        public static void ExportToFile(AccessGraphContext ctx, string filePath)
        {
            var json = Export(ctx);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>Serialise the context and return the JSON string.</summary>
        public static string Export(AccessGraphContext ctx)
        {
            // 1. Derive container tags from HostedIn relationships
            var containerTagMap = BuildContainerTagMap(ctx);

            // 2. Build node elements
            var nodes = ctx.Resources
                .Select(r => BuildNode(r, containerTagMap))
                .ToList();

            // 3. Build all edge elements
            var edges = new List<CyEdge>();
            edges.AddRange(ctx.Relationships.Select(BuildRelationshipEdge));
            edges.AddRange(ctx.Memberships.Select(BuildMembershipEdge));
            edges.AddRange(BuildGrantEdges(ctx));
            edges.AddRange(ctx.ContentBindings.Select(b => BuildContentBindingEdge(b, ctx)));

            // 4. Assemble the root document
            var doc = new CyDocument
            {
                Meta = new CyMeta
                {
                    SchemaVersion = "1.0",
                    GeneratedAt = DateTime.UtcNow.ToString("o"),
                    Generator = "IdentityMap.DataModel.GraphExportService",
                    Stats = new CyStats
                    {
                        Nodes = nodes.Count,
                        Edges = new CyEdgeStats
                        {
                            Relationships = ctx.Relationships.Count,
                            Memberships = ctx.Memberships.Count,
                            Grants = ctx.Grants.Count,
                            ContentBindings = ctx.ContentBindings.Count,
                            Total = ctx.Relationships.Count
                                  + ctx.Memberships.Count
                                  + ctx.Grants.Count
                                  + ctx.ContentBindings.Count
                        }
                    }
                },
                Legend = BuildLegend(ctx),
                Elements = new CyElements { Nodes = nodes, Edges = edges }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(doc, options);
        }

        // ── Node builder ──────────────────────────────────────────────────────

        private static CyNode BuildNode(
            Resource r,
            IReadOnlyDictionary<Guid, string> containerTagMap)
        {
            // Merge static tags (set in Program.cs) with derived tags
            var tags = new List<string>(r.Tags);

            // Derived: container tag
            if (containerTagMap.TryGetValue(r.Id, out var containerTag)
                && !tags.Contains(containerTag))
                tags.Add(containerTag);

            // Derived: sensitivity tag (convenient for CSS class binding in frontend)
            var sensitivityTag = $"sensitivity:{r.Sensitivity.ToString().ToLowerInvariant()}";
            if (!tags.Contains(sensitivityTag))
                tags.Add(sensitivityTag);

            // Derived: status tag
            var statusTag = $"status:{r.Status.ToLowerInvariant()}";
            if (!tags.Contains(statusTag))
                tags.Add(statusTag);

            return new CyNode
            {
                Data = new CyNodeData
                {
                    Id = Fmt(r.Id),
                    Label = r.Name,
                    Type = r.Type.ToString(),
                    Sensitivity = r.Sensitivity.ToString(),
                    SensitivityLevel = (int)r.Sensitivity,
                    Status = r.Status,
                    Description = r.Description,
                    ContentAccessModel = r.ContentAccessModel?.ToString(),
                    ContentNature = r.ContentNature?.ToString(),
                    ContentSchemaDescription = r.ContentSchemaDescription,
                    Tags = tags
                }
            };
        }

        // ── Edge builders ─────────────────────────────────────────────────────

        private static CyEdge BuildRelationshipEdge(ResourceRelationship rel) =>
            new CyEdge
            {
                Data = new CyEdgeData
                {
                    Id = Fmt(rel.Id),
                    Source = Fmt(rel.ParentResourceId),
                    Target = Fmt(rel.ChildResourceId),
                    EdgeKind = "Relationship",
                    Label = rel.Type.ToString(),
                    RelationshipType = rel.Type.ToString(),
                    Notes = rel.Notes
                }
            };

        private static CyEdge BuildMembershipEdge(BusinessAppMembership m) =>
            new CyEdge
            {
                Data = new CyEdgeData
                {
                    Id = Fmt(m.Id),
                    // Cytoscape convention: source = the container (group/role),
                    // target = the member, so arrows point inward to the container.
                    // The view-plane compiler reads MemberRole to decide direction.
                    Source = Fmt(m.BusinessAppResourceId),
                    Target = Fmt(m.MemberResourceId),
                    EdgeKind = "Membership",
                    Label = "MemberOf",
                    MemberRole = m.Role,
                    IsActive = m.IsActive
                }
            };

        private static IEnumerable<CyEdge> BuildGrantEdges(AccessGraphContext ctx)
        {
            // Build a lookup so we can embed capability details in the edge data
            // without the frontend needing to know about the ResourceCapability entity.
            var capLookup = ctx.Capabilities.ToDictionary(c => c.Id);

            foreach (var g in ctx.Grants)
            {
                if (!capLookup.TryGetValue(g.ResourceCapabilityId, out var cap))
                    continue; // orphaned grant — skip

                yield return new CyEdge
                {
                    Data = new CyEdgeData
                    {
                        Id = Fmt(g.Id),
                        // Grant: subject → resource that exposes the capability.
                        // "Subject has [CapabilityType] access to [Resource]."
                        Source = Fmt(g.SubjectResourceId),
                        Target = Fmt(cap.ResourceId),
                        EdgeKind = "Grant",
                        Label = cap.Type.ToString(),
                        CapabilityId = Fmt(cap.Id),
                        CapabilityType = cap.Type.ToString(),
                        CapabilityScope = cap.Scope.ToString(),
                        GrantStatus = g.Status.ToString(),
                        Justification = g.Justification,
                        ActivatedAt = g.ActivatedAt?.ToString("o"),
                        ExpiresAt = g.ExpiresAt?.ToString("o")
                    }
                };
            }
        }

        private static CyEdge BuildContentBindingEdge(ContentBinding b, AccessGraphContext ctx)
        {
            // Embed accessor name so the frontend can label the edge without a lookup.
            string? accessorLabel = b.AccessorResourceId.HasValue
                ? ctx.FindResource(b.AccessorResourceId.Value)?.Name
                : null;

            return new CyEdge
            {
                Data = new CyEdgeData
                {
                    Id = Fmt(b.Id),
                    // ContentBinding: source = data source, target = consumer endpoint/app.
                    // "Source serves data TO consumer."
                    Source = Fmt(b.ContentSourceId),
                    Target = Fmt(b.ConsumerResourceId),
                    EdgeKind = "ContentBinding",
                    Label = $"{b.Role}:{b.AccessType}",
                    BindingRole = b.Role.ToString(),
                    AccessType = b.AccessType.ToString(),
                    AccessorId = b.AccessorResourceId.HasValue ? Fmt(b.AccessorResourceId.Value) : null,
                    AccessorLabel = accessorLabel,
                    IsActive = b.IsActive,
                    Notes = b.Description,
                    ContributionDescription = b.ContributionDescription
                }
            };
        }

        // ── Container tag derivation ──────────────────────────────────────────
        //
        // Walk the HostedIn relationship tree upward from each node to find the
        // nearest ancestor that is a top-level container (a BusinessApp or
        // VirtualMachine with no HostedIn parent of its own).
        // That ancestor's slugified name becomes "container:<slug>".
        //
        // Example chains:
        //   alice → HostedIn → corp.AD               → container:corp-ad
        //   dbo.Customers → HostedIn → CustomerData
        //                 → HostedIn → SQLSRV-PROD-01 → container:sqlsrv-prod-01
        //   GET /api/customers → HostedIn → CustomerPortal → container:customerportal

        private static IReadOnlyDictionary<Guid, string> BuildContainerTagMap(
            AccessGraphContext ctx)
        {
            // child → parent mapping (HostedIn or BelongsTo only)
            var parentMap = ctx.Relationships
                .Where(r => r.Type is RelationshipType.HostedIn
                                   or RelationshipType.BelongsTo)
                .GroupBy(r => r.ChildResourceId)
                .ToDictionary(g => g.Key, g => g.First().ParentResourceId);

            // resourceId → Resource lookup
            var resourceMap = ctx.Resources.ToDictionary(r => r.Id);

            // Determine which resource IDs are "root containers":
            // BusinessApp or VirtualMachine that are not children of anything else.
            var childIds = parentMap.Keys.ToHashSet();
            var rootContainers = ctx.Resources
                .Where(r => !childIds.Contains(r.Id)
                         && r.Type is ResourceType.BusinessApp
                                   or ResourceType.VirtualMachine)
                .ToDictionary(r => r.Id, r => Slugify(r.Name));

            // For each resource, walk up the parent chain to find the root container.
            var result = new Dictionary<Guid, string>();
            foreach (var resource in ctx.Resources)
            {
                var rootSlug = FindRootContainerSlug(
                    resource.Id, parentMap, rootContainers, resourceMap);
                if (rootSlug != null)
                    result[resource.Id] = $"container:{rootSlug}";
            }

            return result;
        }

        private static string? FindRootContainerSlug(
            Guid resourceId,
            Dictionary<Guid, Guid> parentMap,
            Dictionary<Guid, string> rootContainers,
            Dictionary<Guid, Resource> resourceMap,
            int maxDepth = 20)
        {
            // If this resource IS a root container, return its own slug.
            if (rootContainers.TryGetValue(resourceId, out var ownSlug))
                return ownSlug;

            var current = resourceId;
            for (int i = 0; i < maxDepth; i++)
            {
                if (!parentMap.TryGetValue(current, out var parentId))
                    break; // no parent — not under any root container

                if (rootContainers.TryGetValue(parentId, out var slug))
                    return slug; // found the root container

                current = parentId;
            }

            return null; // orphan node — no container tag
        }

        // ── Legend builder ────────────────────────────────────────────────────

        private static CyLegend BuildLegend(AccessGraphContext ctx) =>
            new CyLegend
            {
                NodeTypes = ctx.Resources
                    .Select(r => r.Type.ToString())
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList(),

                EdgeKinds = new List<string>
                {
                    "Relationship", "Membership", "Grant", "ContentBinding"
                },

                RelationshipTypes = ctx.Relationships
                    .Select(r => r.Type.ToString())
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList(),

                SensitivityScale = new Dictionary<string, int>
                {
                    ["None"] = 0,
                    ["Internal"] = 1,
                    ["Confidential"] = 2,
                    ["Restricted"] = 3,
                    ["TopSecret"] = 4
                },

                TagPrefixes = new List<string>
                {
                    "tier:identity", "tier:database", "tier:web",
                    "kind:human", "kind:svc-account", "kind:ad-group",
                    "kind:sql-server", "kind:database", "kind:table",
                    "kind:row", "kind:server-role", "kind:server-login",
                    "kind:db-role", "kind:db-user", "kind:webapp",
                    "kind:endpoint", "kind:idp",
                    "container:*  (derived from HostedIn relationships)",
                    "sensitivity:* (derived from Sensitivity field)",
                    "status:*      (derived from Status field)"
                },

                CapabilityTypes = ctx.Capabilities
                    .Select(c => c.Type.ToString())
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList()
            };

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Format a Guid as a lowercase string (no braces).</summary>
        private static string Fmt(Guid id) => id.ToString("D").ToLowerInvariant();

        /// <summary>
        /// Convert a resource name to a URL-safe slug for container tags.
        /// "SQLSRV-PROD-01" → "sqlsrv-prod-01"
        /// "corp.AD"        → "corp-ad"
        /// "CustomerPortal" → "customerportal"
        /// </summary>
        private static string Slugify(string name) =>
            new string(name
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray())
            .Trim('-');
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DTOs — plain records serialised directly to JSON.
    // Named with the "Cy" prefix to avoid namespace conflicts with the domain.
    // ═══════════════════════════════════════════════════════════════════════════

    internal record CyDocument
    {
        public CyMeta Meta { get; init; } = new();
        public CyLegend Legend { get; init; } = new();
        public CyElements Elements { get; init; } = new();
    }

    internal record CyMeta
    {
        public string SchemaVersion { get; init; } = "1.0";
        public string GeneratedAt { get; init; } = string.Empty;
        public string Generator { get; init; } = string.Empty;
        public CyStats Stats { get; init; } = new();
    }

    internal record CyStats
    {
        public int Nodes { get; init; }
        public CyEdgeStats Edges { get; init; } = new();
    }

    internal record CyEdgeStats
    {
        public int Relationships { get; init; }
        public int Memberships { get; init; }
        public int Grants { get; init; }
        public int ContentBindings { get; init; }
        public int Total { get; init; }
    }

    internal record CyLegend
    {
        public List<string> NodeTypes { get; init; } = new();
        public List<string> EdgeKinds { get; init; } = new();
        public List<string> RelationshipTypes { get; init; } = new();
        public Dictionary<string, int> SensitivityScale { get; init; } = new();
        public List<string> TagPrefixes { get; init; } = new();
        public List<string> CapabilityTypes { get; init; } = new();
    }

    internal record CyElements
    {
        public List<CyNode> Nodes { get; init; } = new();
        public List<CyEdge> Edges { get; init; } = new();
    }

    internal record CyNode
    {
        public CyNodeData Data { get; init; } = new();
    }

    internal record CyNodeData
    {
        public string Id { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Sensitivity { get; init; } = string.Empty;
        public int SensitivityLevel { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ContentAccessModel { get; init; }
        public string? ContentNature { get; init; }
        public string? ContentSchemaDescription { get; init; }
        public List<string> Tags { get; init; } = new();
    }

    internal record CyEdge
    {
        public CyEdgeData Data { get; init; } = new();
    }

    internal record CyEdgeData
    {
        // ── Common ────────────────────────────────────────────────────────────
        public string Id { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
        public string EdgeKind { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;

        // ── Relationship-specific ─────────────────────────────────────────────
        public string? RelationshipType { get; init; }
        public string? Notes { get; init; }

        // ── Membership-specific ───────────────────────────────────────────────
        public string? MemberRole { get; init; }
        public bool? IsActive { get; init; }

        // ── Grant-specific ────────────────────────────────────────────────────
        public string? CapabilityId { get; init; }
        public string? CapabilityType { get; init; }
        public string? CapabilityScope { get; init; }
        public string? GrantStatus { get; init; }
        public string? Justification { get; init; }
        public string? ActivatedAt { get; init; }
        public string? ExpiresAt { get; init; }

        // ── ContentBinding-specific ───────────────────────────────────────────
        public string? BindingRole { get; init; }
        public string? AccessType { get; init; }
        public string? AccessorId { get; init; }
        public string? AccessorLabel { get; init; }
        public string? ContributionDescription { get; init; }
    }
}
