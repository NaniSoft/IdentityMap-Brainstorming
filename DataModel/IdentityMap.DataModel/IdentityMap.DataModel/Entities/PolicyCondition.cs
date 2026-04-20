using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    public class PolicyCondition : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ConditionExpression { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEvaluatable { get; set; } = false;
        public PolicyAttributeSource AttributeSource { get; set; } = PolicyAttributeSource.Subject;

        public Guid? ResourceCapabilityId { get; set; }
        public ResourceCapability? ResourceCapability { get; set; }
        public Guid? CapabilityGrantId { get; set; }
        public CapabilityGrant? CapabilityGrant { get; set; }
        public int EvaluationOrder { get; set; } = 0;   // order of evaluation when multiple conditions apply
        public bool IsRequired { get; set; } = true;           // if true, this condition must be satisfied for the grant to be effective

    }
}
