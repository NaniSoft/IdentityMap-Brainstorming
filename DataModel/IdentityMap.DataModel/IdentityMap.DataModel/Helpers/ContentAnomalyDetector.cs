using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public static class ContentAnomalyDetector
    {
        public static IReadOnlyList<ContentAnomaly> DetectAll(AccessGraphContext ctx)
        {
            var anomalies = new List<ContentAnomaly>();
            anomalies.AddRange(CheckCapabilityExceedsBindings(ctx));
            anomalies.AddRange(CheckAccessorGrantInsufficient(ctx));
            anomalies.AddRange(CheckUntracedDynamicContent(ctx));
            anomalies.AddRange(CheckWriteTargetWithReadOnlyCapability(ctx));
            return anomalies;
        }

        // ── Check 1: Capability claims a level no binding can support ──────────
        static IEnumerable<ContentAnomaly> CheckCapabilityExceedsBindings(
            AccessGraphContext ctx)
        {
            // Only AccessSurface resources need this check.
            var accessSurfaces = ctx.Resources
                .Where(r => r.ContentAccessModel == ContentAccessModel.AccessSurface);

            foreach (var resource in accessSurfaces)
            {
                var contentCaps = ctx.Capabilities
                    .Where(c => c.ResourceId == resource.Id
                             && c.Scope == CapabilityScope.ContentAccess
                             && c.IsEnabled);

                var bindingLevels = ctx.ContentBindings
                    .Where(b => b.IsActive && b.ConsumerResourceId == resource.Id)
                    .Select(b => ContentAccessLevelHelper.ToAccessLevel(b.AccessType))
                    .ToList();

                var maxBindingLevel = ContentAccessLevelHelper.Aggregate(bindingLevels);

                foreach (var cap in contentCaps)
                {
                    var required = ContentAccessLevelHelper.ToAccessLevel(cap.Type);
                    if (!ContentAccessLevelHelper.Implies(maxBindingLevel, required))
                    {
                        yield return new ContentAnomaly
                        {
                            Type = ContentAnomalyType.CapabilityExceedsBindings,
                            ResourceId = resource.Id,
                            ActualLevel = maxBindingLevel,
                            RequiredLevel = required,
                            Description = $"{resource.Name} declares {cap.Scope}.{cap.Type} " +
                                                $"(requires {required}) but its ContentBindings " +
                                                $"provide at most {maxBindingLevel}.",
                            Recommendation = $"Either add a ContentBinding with AccessType >= {required} " +
                                                $"on {resource.Name}, or remove the {cap.Type} capability " +
                                                $"if the endpoint cannot actually perform writes."
                        };
                    }
                }
            }
        }

        // ── Check 2: Accessor's grant is below what the binding needs ──────────
        static IEnumerable<ContentAnomaly> CheckAccessorGrantInsufficient(
            AccessGraphContext ctx)
        {
            foreach (var binding in ctx.ContentBindings.Where(b => b.IsActive
                                                                 && b.AccessorResourceId.HasValue))
            {
                var requiredLevel = ContentAccessLevelHelper.ToAccessLevel(binding.AccessType);

                // Find the highest ContentAccess grant level the accessor holds on the source.
                var accessorCaps = ctx.Capabilities
                    .Where(c => c.ResourceId == binding.ContentSourceId
                             && c.Scope == CapabilityScope.ContentAccess)
                    .Select(c => c.Id)
                    .ToHashSet();

                var accessorGrantLevels = ctx.Grants
                    .Where(g => g.Status == GrantStatus.Active
                             && g.SubjectResourceId == binding.AccessorResourceId!.Value
                             && accessorCaps.Contains(g.ResourceCapabilityId))
                    .Select(g =>
                    {
                        var cap = ctx.Capabilities.First(c => c.Id == g.ResourceCapabilityId);
                        return ContentAccessLevelHelper.ToAccessLevel(cap.Type);
                    })
                    .ToList();

                var accessorLevel = ContentAccessLevelHelper.Aggregate(accessorGrantLevels);

                if (!ContentAccessLevelHelper.Implies(accessorLevel, requiredLevel))
                {
                    var source = ctx.FindResource(binding.ContentSourceId);
                    var accessor = ctx.FindResource(binding.AccessorResourceId!.Value);
                    var consumer = ctx.FindResource(binding.ConsumerResourceId);
                    yield return new ContentAnomaly
                    {
                        Type = ContentAnomalyType.AccessorGrantInsufficientForBinding,
                        ResourceId = binding.ConsumerResourceId,
                        ContentBindingId = binding.Id,
                        ConflictingResourceId = binding.AccessorResourceId,
                        ActualLevel = accessorLevel,
                        RequiredLevel = requiredLevel,
                        Description = $"ContentBinding '{binding.Description}': " +
                                               $"{consumer?.Name} → {source?.Name} " +
                                               $"requires {binding.AccessType} (level {requiredLevel}), " +
                                               $"but accessor {accessor?.Name} only has level {accessorLevel}.",
                        Recommendation = $"Grant {accessor?.Name} a {requiredLevel}-level capability " +
                                               $"on {source?.Name}, or downgrade the binding's AccessType " +
                                               $"to match what the accessor can actually do."
                    };
                }
            }
        }

        // ── Check 3: AccessSurface with Dynamic content but no bindings ────────
        static IEnumerable<ContentAnomaly> CheckUntracedDynamicContent(
            AccessGraphContext ctx)
        {
            var dynamicSurfaces = ctx.Resources
                .Where(r => r.ContentAccessModel == ContentAccessModel.AccessSurface
                         && r.ContentNature == ContentNature.Dynamic);

            foreach (var resource in dynamicSurfaces)
            {
                var hasBindings = ctx.ContentBindings
                    .Any(b => b.IsActive && b.ConsumerResourceId == resource.Id);

                if (!hasBindings)
                    yield return new ContentAnomaly
                    {
                        Type = ContentAnomalyType.UntracedDynamicContent,
                        ResourceId = resource.Id,
                        Description = $"{resource.Name} is an AccessSurface with Dynamic content " +
                                         $"but has no ContentBindings. Its data sources are invisible " +
                                         $"to the access graph — sensitivity cannot be traced.",
                        Recommendation = "Add at least one ContentBinding declaring where this " +
                                         "resource's dynamic content comes from."
                    };
            }
        }

        // ── Check 4: WriteTarget binding but consumer has no Write capability ───
        static IEnumerable<ContentAnomaly> CheckWriteTargetWithReadOnlyCapability(
            AccessGraphContext ctx)
        {
            var writeTargetBindings = ctx.ContentBindings
                .Where(b => b.IsActive && b.Role == ContentBindingRole.WriteTarget);

            foreach (var binding in writeTargetBindings)
            {
                var consumerHasWriteCap = ctx.Capabilities
                    .Any(c => c.ResourceId == binding.ConsumerResourceId
                           && c.Scope == CapabilityScope.ContentAccess
                           && c.IsEnabled
                           && ContentAccessLevelHelper.Implies(
                               ContentAccessLevelHelper.ToAccessLevel(c.Type),
                               ContentAccessLevel.Write));

                if (!consumerHasWriteCap)
                {
                    var consumer = ctx.FindResource(binding.ConsumerResourceId);
                    var source = ctx.FindResource(binding.ContentSourceId);
                    yield return new ContentAnomaly
                    {
                        Type = ContentAnomalyType.WriteTargetWithReadOnlyCapability,
                        ResourceId = binding.ConsumerResourceId,
                        ContentBindingId = binding.Id,
                        Description = $"ContentBinding marks {source?.Name} as a WriteTarget " +
                                            $"for {consumer?.Name}, but {consumer?.Name} has no " +
                                            $"ContentAccess.Write capability declared.",
                        Recommendation = "Add a ContentAccess.Write capability to " +
                                            $"{consumer?.Name} to match its WriteTarget binding, " +
                                            "or change the binding Role to SecondarySource/ReadOnly."
                    };
                }
            }
        }
    }
}
