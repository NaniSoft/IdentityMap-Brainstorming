namespace IdentityMap.DataModel.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Audit ─────────────────────────────────────────────────────────────
        public Guid? CreatedByResourceId { get; set; }
        public Guid? UpdatedByResourceId { get; set; }   // NEW: who last mutated

        // ── Soft delete ───────────────────────────────────────────────────────
        // Use IsDeleted/DeletedAt instead of hard-deletes so the access graph
        // can reason about historical entitlements during certification reviews.
        public bool IsDeleted { get; set; } = false;     // NEW
        public DateTime? DeletedAt { get; set; }         // NEW
        public Guid? DeletedByResourceId { get; set; }   // NEW
    }
}