using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    /// <summary>
    /// Associates a resource with a compliance or regulatory framework.
    /// Multiple policies can be attached to a single resource
    /// (e.g. a Payments table might be both PCI-DSS and GDPR scope).
    /// </summary>
    public class ResourcePolicy : BaseEntity
    {
        public ComplianceFramework Framework { get; set; }

        /// <summary>
        /// Optional sub-classification within the framework.
        /// E.g. for PCI-DSS: "CDE", "Connected", "Out-of-Scope".
        /// For GDPR: "PersonalData", "SensitivePersonalData", "Anonymous".
        /// </summary>
        public string? SubClassification { get; set; }

        public string? Notes { get; set; }
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public Guid ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;
    }
}
