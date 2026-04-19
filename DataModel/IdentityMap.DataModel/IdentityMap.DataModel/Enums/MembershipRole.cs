using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum MembershipRole
    {
        Member,             // regular member — inherits group grants, no management rights
        Owner,              // can approve grant requests for this group/app
        DataSteward,        // classifies and manages content policy for this scope
        TechnicalCustodian, // administers the resource; can update membership
        Delegate,           // temporary delegate for an absent owner
        Auditor,            // read-only observer — cannot approve, cannot delegate
        ServiceIdentity     // a service account added to a group for runtime access
    }
}
