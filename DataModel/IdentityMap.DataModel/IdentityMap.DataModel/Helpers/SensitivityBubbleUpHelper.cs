using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public static class SensitivityBubbleUpHelper
    {
        /// <summary>
        /// Recomputes EffectiveSensitivity for <paramref name="resource"/> by taking
        /// the MAX of:
        ///   • Its own declared Sensitivity
        ///   • The Sensitivity of every active ContentBinding source
        ///   • The AttributeSensitivity of all its attribute definitions
        ///   • The Sensitivity of all its children (HostedIn / BelongsTo)
        ///
        /// Returns the new EffectiveSensitivity and updates the resource in-place.
        /// </summary>
        public static SensitivityClassification Recompute(
            Resource resource,
            AccessGraphContext ctx)
        {
            var max = resource.Sensitivity;

            // From content bindings where this resource is the consumer
            foreach (var binding in ctx.ContentBindings
                .Where(b => b.IsActive && b.ConsumerResourceId == resource.Id))
            {
                var source = ctx.FindResource(binding.ContentSourceId);
                if (source != null)
                {
                    var src = source.EffectiveSensitivity > source.Sensitivity
                        ? source.EffectiveSensitivity : source.Sensitivity;
                    if (src > max) max = src;
                }
            }

            // From attribute definitions (e.g. an ssn_encrypted attr on any resource type)
            foreach (var attrVal in ctx.AttributeValues
                .Where(v => v.ResourceId == resource.Id))
            {
                var def = ctx.AttributeDefinitions
                    .FirstOrDefault(d => d.Id == attrVal.ResourceAttributeDefinitionId);
                if (def != null && def.AttributeSensitivity > max)
                    max = def.AttributeSensitivity;
            }

            // From hosted children (bubble up from rows to tables, tables to databases)
            var childIds = ctx.Relationships
                .Where(r => r.ParentResourceId == resource.Id
                         && (r.Type == RelationshipType.HostedIn
                          || r.Type == RelationshipType.BelongsTo))
                .Select(r => r.ChildResourceId);

            foreach (var childId in childIds)
            {
                var child = ctx.FindResource(childId);
                if (child != null && child.EffectiveSensitivity > max)
                    max = child.EffectiveSensitivity;
            }

            resource.EffectiveSensitivity = max;
            return max;
        }

        /// <summary>
        /// Re-runs Recompute for every resource in bottom-up order
        /// (leaves first, then their parents). Call after bulk import or
        /// after a sensitivity re-classification campaign.
        /// </summary>
        public static void RecomputeAll(AccessGraphContext ctx)
        {
            // Topological sort: process resources with no children first.
            var parentIds = ctx.Relationships
                .Where(r => r.Type is RelationshipType.HostedIn
                                   or RelationshipType.BelongsTo)
                .Select(r => r.ParentResourceId)
                .ToHashSet();

            var ordered = ctx.Resources
                .OrderBy(r => parentIds.Contains(r.Id) ? 1 : 0);

            foreach (var resource in ordered)
                Recompute(resource, ctx);
        }
    }
}
