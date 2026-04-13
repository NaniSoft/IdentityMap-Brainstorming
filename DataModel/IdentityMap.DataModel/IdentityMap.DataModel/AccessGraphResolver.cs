using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel
{
    public static class AccessGraphResolver
    {
        /// <summary>
        /// Finds all access touchpoints starting from the given sensitive resource.
        /// Returns them in BFS order (shallowest first).
        /// </summary>
        public static IReadOnlyList<AccessTouchpoint> FindTouchpoints(
            Guid sensitiveResourceId,
            AccessGraphContext ctx,
            int maxDepth = 15)
        {
            var source = ctx.FindResource(sensitiveResourceId)
                ?? throw new ArgumentException($"Resource {sensitiveResourceId} not found.");

            var results = new List<AccessTouchpoint>();
            var visited = new HashSet<Guid>();

            // Queue item: (resource, edgeLabel, pathSoFar)
            var queue = new Queue<(Resource resource, string edge, List<string> path)>();

            void Enqueue(Resource r, string edge, List<string> path)
            {
                if (visited.Contains(r.Id)) return;
                visited.Add(r.Id);
                queue.Enqueue((r, edge, path));
            }

            visited.Add(source.Id);
            queue.Enqueue((source, "Source", new List<string> { source.Name }));

            while (queue.Count > 0)
            {
                var (resource, edge, path) = queue.Dequeue();
                int depth = path.Count - 1;

                results.Add(new AccessTouchpoint
                {
                    Resource = resource,
                    EdgeLabel = edge,
                    PathFromSource = path.AsReadOnly(),
                    Depth = depth,
                    EffectiveSensitivity = source.Sensitivity
                });

                if (depth >= maxDepth) continue;

                // ── Rule 1: Who has grants on this resource? ─────────────────
                var myCaps = ctx.Capabilities
                    .Where(c => c.ResourceId == resource.Id)
                    .Select(c => c.Id)
                    .ToHashSet();

                var grantSubjects = ctx.Grants
                    .Where(g => g.Status == GrantStatus.Active && myCaps.Contains(g.ResourceCapabilityId))
                    .Select(g => new
                    {
                        Subject = ctx.FindResource(g.SubjectResourceId),
                        CapType = ctx.Capabilities.First(c => c.Id == g.ResourceCapabilityId).Type
                    })
                    .Where(x => x.Subject != null);

                foreach (var gs in grantSubjects)
                {
                    var label = $"Has{gs.CapType}Grant";
                    Enqueue(gs.Subject!, label,
                        new List<string>(path) { $"[{label}] {gs.Subject!.Name}" });
                }

                // ── Rule 2a: Incoming ResourceReference attrs ─────────────────
                var resourceIdStr = resource.Id.ToString();
                var incomingRefs = ctx.AttributeValues
                    .Where(v => v.ValueString == resourceIdStr)
                    .Select(v => new
                    {
                        Referencing = ctx.FindResource(v.ResourceId),
                        AttrKey = ctx.AttributeDefinitions
                            .FirstOrDefault(d => d.Id == v.ResourceAttributeDefinitionId)?.Key
                            ?? "ref"
                    })
                    .Where(x => x.Referencing != null);

                foreach (var r in incomingRefs)
                {
                    var label = $"ReferencedBy:{r.AttrKey}";
                    Enqueue(r.Referencing!, label,
                        new List<string>(path) { $"[{label}] {r.Referencing!.Name}" });
                }

                // ── Rule 2b: Outgoing ResourceReference attrs ─────────────────
                var outgoingRefs = ctx.AttributeValues
                    .Where(v => v.ResourceId == resource.Id)
                    .Select(v => new
                    {
                        AttrDef = ctx.AttributeDefinitions.FirstOrDefault(d => d.Id == v.ResourceAttributeDefinitionId),
                        ValueStr = v.ValueString
                    })
                    .Where(x => x.AttrDef?.DataType == AttributeDataType.ResourceReference
                             && Guid.TryParse(x.ValueStr, out _));

                foreach (var r in outgoingRefs)
                {
                    var refId = Guid.Parse(r.ValueStr!);
                    var refRes = ctx.FindResource(refId);
                    if (refRes == null) continue;
                    var label = $"LinkedTo:{r.AttrDef!.Key}";
                    Enqueue(refRes, label,
                        new List<string>(path) { $"[{label}] {refRes.Name}" });
                }

                // ── Rule 3: Who uses this resource as identity? ───────────────
                var identityConsumers = ctx.Relationships
                    .Where(rel => rel.Type == RelationshipType.UsesIdentity
                               && rel.ChildResourceId == resource.Id)
                    .Select(rel => ctx.FindResource(rel.ParentResourceId))
                    .Where(r => r != null);

                foreach (var consumer in identityConsumers)
                {
                    var label = $"IdentityUsedBy:{consumer!.Name}";
                    Enqueue(consumer!, label,
                        new List<string>(path) { $"[{label}] {consumer!.Name}" });
                }

                // ── Rule 4: Children of this resource with active grants ───────
                var childResIds = ctx.Relationships
                    .Where(rel => rel.ParentResourceId == resource.Id
                               && (rel.Type == RelationshipType.HostedIn
                                || rel.Type == RelationshipType.BelongsTo))
                    .Select(rel => rel.ChildResourceId)
                    .ToHashSet();

                var childrenWithGrants = ctx.Capabilities
                    .Where(c => childResIds.Contains(c.ResourceId)
                             && c.Scope == CapabilityScope.ContentAccess)
                    .Select(c => c.ResourceId)
                    .Distinct()
                    .Where(cid => ctx.Grants.Any(g =>
                        g.Status == GrantStatus.Active &&
                        ctx.Capabilities.Any(cap => cap.Id == g.ResourceCapabilityId && cap.ResourceId == cid)));

                foreach (var childId in childrenWithGrants)
                {
                    var child = ctx.FindResource(childId);
                    if (child == null) continue;
                    var label = $"ContentOf:{resource.Name}";
                    Enqueue(child, label,
                        new List<string>(path) { $"[{label}] {child.Name}" });
                }

                // ── Rule 5: Group / BusinessApp members ───────────────────────
                if (resource.Type == ResourceType.Group
                 || resource.Type == ResourceType.BusinessApp)
                {
                    var members = ctx.Memberships
                        .Where(m => m.BusinessAppResourceId == resource.Id && m.IsActive)
                        .Select(m => ctx.FindResource(m.MemberResourceId))
                        .Where(m => m != null);

                    foreach (var member in members)
                    {
                        var label = $"MemberOf:{resource.Name}";
                        Enqueue(member!, label,
                            new List<string>(path) { $"[{label}] {member!.Name}" });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns only human Account touchpoints (the end-users who can
        /// ultimately reach the data), deduplicated by resource Id.
        /// </summary>
        public static IReadOnlyList<AccessTouchpoint> FindHumanAccessors(
            Guid sensitiveResourceId,
            AccessGraphContext ctx,
            int maxDepth = 15)
        {
            return FindTouchpoints(sensitiveResourceId, ctx, maxDepth)
                .Where(t => t.Resource.Type == ResourceType.Account
                         || t.Resource.Type == ResourceType.ServiceAccount)
                .GroupBy(t => t.Resource.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}