using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum ContentAnomalyType
    {
        /// <summary>
        /// An AccessSurface resource declares a ContentAccess capability (e.g. Write)
        /// but none of its active ContentBindings have an AccessType that supports that
        /// level.  The capability is declared but structurally unachievable.
        ///
        /// Example: endpoint has ContentAccess.Write capability, but all its
        /// ContentBindings have AccessType = Read.  No path exists for a write.
        /// </summary>
        CapabilityExceedsBindings,

        /// <summary>
        /// A ContentBinding declares an AccessType (e.g. ReadWrite) but the accessor
        /// resource does not hold an active CapabilityGrant at a sufficient level on
        /// the ContentSource.  The binding requires more than the accessor can provide.
        ///
        /// Example: binding has AccessType = Write (consumer wants to write to the DB)
        /// but SQL\svc-webapp only has a Read grant on CustomerData.  The write would
        /// fail at runtime — and the governance model should flag it before deployment.
        /// </summary>
        AccessorGrantInsufficientForBinding,

        /// <summary>
        /// An AccessSurface resource with ContentNature = Dynamic has no active
        /// ContentBindings.  Its content is untraced — sensitivity cannot be bubbled
        /// up, and the access graph is incomplete for this resource.
        /// </summary>
        UntracedDynamicContent,

        /// <summary>
        /// A ContentBinding declares Role = WriteTarget but the ResourceCapability
        /// on the consumer is scoped as ContentAccess.Read only.  The data flow says
        /// "write" but the governance model says "read" — an intent inconsistency.
        /// </summary>
        WriteTargetWithReadOnlyCapability
    }

}
