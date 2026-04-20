using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Helpers
{
    public record TraversalEdge
    {
        public Guid EdgeId { get; init; }
        public Guid SourceId{ get; init; }
        public Guid TargetId { get; init; }
        public EdgeCategory Category { get; init; }
        public string EdgeType { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public bool IsReversed { get; init; }
        public string? Notes { get; init; }
        public GrantStatus? GrantStatus { get; init; }
        public CapabilityScope? CapabilityScope { get; init; }
        public CapabilityType? CapabilityType { get; init; }
        public ContentBindingAccessType? BindingAccessType { get; init; }
        public ContentBindingRole? BindingRole { get; init; }
        public Guid? AccessorResourceId { get; init; }
        public IReadOnlyList<string> PolicyConditions { get; init; } = Array.Empty<string>();

        public Guid GetNextResourceId(Guid currentResourceId)
        {
            if (currentResourceId == SourceId) return TargetId;
            if (currentResourceId == TargetId) return SourceId;
            throw new InvalidOperationException($"Current resource ID {currentResourceId} does not match either end of the edge.");
        }

        public TraversalEdge Reverse() => this with
        {
            SourceId = TargetId,
            TargetId = SourceId,
            IsReversed = !IsReversed,
            Label = $"[REV] {Label}"
        };
    }
}