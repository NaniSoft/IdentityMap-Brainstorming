using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    /// <summary>
    /// Defines a formal dependency between two CapabilityTypes.
    ///
    /// Examples:
    ///   Write → implies → Read  (to write you need read)
    ///   Delete → implies → Read
    ///   Administer → implies → Write
    ///   Delegate → implies → Administer
    ///
    /// When IsImplied = true: any subject that holds a CapabilityGrant for
    /// DependentType automatically holds an effective grant for RequiredType
    /// on the same resource, without needing an explicit CapabilityGrant for it.
    ///
    /// When IsImplied = false: the dependent capability REQUIRES the required one
    /// to already exist, but does not automatically confer it. Used to enforce
    /// prerequisites (e.g. you cannot be granted Delete unless you already have Write).
    /// </summary>
    public class CapabilityImplication : BaseEntity
    {
        public CapabilityType DependentType { get; set; }   // the "higher" capability
        public CapabilityType RequiredType { get; set; }    // the "lower" prerequisite
        public bool IsImplied { get; set; } = true;         // does Dependent grant Required automatically?
        public string? Rationale { get; set; }
    }
}
