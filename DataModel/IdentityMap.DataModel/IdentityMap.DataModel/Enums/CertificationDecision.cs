using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum CertificationDecision
    {
        Pending,            // reviewer has not yet acted
        Certified,          // access is confirmed as appropriate
        Revoked,            // reviewer has determined access should be removed
        Escalated,          // reviewer has escalated to another approver
        Abstained           // reviewer could not certify (conflict of interest)
    }
}
