using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    public class ContentBinding : BaseEntity
    {
        public ContentBindingAccessType AccessType { get; set; }
            = ContentBindingAccessType.Read;

        public ContentBindingRole Role { get; set; }
            = ContentBindingRole.PrimarySource;

        public string? ContributionDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }

        // NEW: Optional JSON or comma-delimited list of field/column names that
        // flow through this binding. Null means "all columns visible to accessor".
        // Example: "id,name,email" on a Customers→Endpoint binding that excludes ssn.
        // Enables column-level data lineage and fine-grained sensitivity propagation.
        public string? ColumnFilter { get; set; }

        // NEW: Cached effective sensitivity flowing from ContentSource to Consumer
        // via this binding. Set by SensitivityBubbleUpHelper when the source
        // sensitivity changes or the ColumnFilter is updated.
        public SensitivityClassification PropagatedSensitivity { get; set; }
            = SensitivityClassification.None;

        public Guid ConsumerResourceId { get; set; }
        public Resource ConsumerResource { get; set; } = null!;
        public Guid ContentSourceId { get; set; }
        public Resource ContentSource { get; set; } = null!;
        public Guid? AccessorResourceId { get; set; }
        public Resource? AccessorResource { get; set; }
    }
}
