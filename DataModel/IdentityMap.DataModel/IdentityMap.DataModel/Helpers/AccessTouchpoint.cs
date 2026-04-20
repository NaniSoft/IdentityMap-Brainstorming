using IdentityMap.DataModel.Entities;
using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Helpers
{
    public record AccessTouchpoint
    {
        /// <summary>The discovered resource.</summary>
        public Resource Resource { get; init; } = null!;

        /// <summary>
        /// How this resource was connected to the previous node.
        /// E.g. "HasReadGrant", "LinkedTo:linked_ad_identity", "MemberOf:adGroup_db_readers"
        /// </summary>
        public string EdgeLabel { get; init; } = string.Empty;

        /// <summary>Human-readable chain from the sensitive source to this node.</summary>
        public IReadOnlyList<string> PathFromSource { get; init; } = Array.Empty<string>();

        /// <summary>Number of hops from the source resource.</summary>
        public int Depth { get; init; }

        /// <summary>The highest sensitivity seen on this path.</summary>
        public SensitivityClassification EffectiveSensitivity { get; init; }

        public TraversalEdge? TraversalEdge { get; init; }

        public IReadOnlyList<string> PolicyConditions { get; init; } = Array.Empty<string>();
        public EdgeCategory? EdgeCategory => TraversalEdge?.Category;
        public bool IsReversedEdge => TraversalEdge?.IsReversed ?? false;

        public override string ToString() =>
            $"[depth={Depth}] {Resource.Type}/{Resource.Name}  ← {EdgeLabel}\n" +
            $"   path: {string.Join(" → ", PathFromSource)}" +
            (PolicyConditions.Any() ? $"\n policies: {string.Join(", ", PolicyConditions)}" : "");
    }
}