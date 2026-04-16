using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public record ContentAnomaly
    {
        public ContentAnomalyType Type { get; init; }

        /// <summary>The resource that has the inconsistency (typically the endpoint).</summary>
        public Guid ResourceId { get; init; }

        /// <summary>The ContentBinding involved, if applicable.</summary>
        public Guid? ContentBindingId { get; init; }

        /// <summary>
        /// The conflicting resource — the accessor with insufficient grant, or the
        /// content source with untraced sensitivity.
        /// </summary>
        public Guid? ConflictingResourceId { get; init; }

        /// <summary>
        /// The accessor's actual level vs the level the binding required.
        /// Null for anomaly types that don't involve a level comparison.
        /// </summary>
        public ContentAccessLevel? ActualLevel { get; init; }
        public ContentAccessLevel? RequiredLevel { get; init; }

        public string Description { get; init; } = string.Empty;
        public string Recommendation { get; init; } = string.Empty;
    }
}
