using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Helpers
{
    public static class ContentAccessLevelHelper
    {
        /// <summary>
        /// Maps a ContentBindingAccessType (what the consumer does) to the minimum
        /// ContentAccessLevel the accessor must have on the source.
        /// </summary>
        public static ContentAccessLevel ToAccessLevel(ContentBindingAccessType accessType) =>
            accessType switch
            {
                ContentBindingAccessType.Read => ContentAccessLevel.Read,
                ContentBindingAccessType.Write => ContentAccessLevel.Write,
                ContentBindingAccessType.ReadWrite => ContentAccessLevel.Write,   // Write ≥ Read
                ContentBindingAccessType.Execute => ContentAccessLevel.Execute,
                ContentBindingAccessType.Subscribe => ContentAccessLevel.Read,
                _ => ContentAccessLevel.None
            };

        /// <summary>
        /// Maps a CapabilityType to the ContentAccessLevel it requires on the
        /// underlying content source for that capability to be fulfillable.
        /// </summary>
        public static ContentAccessLevel ToAccessLevel(CapabilityType capType) =>
            capType switch
            {
                CapabilityType.Read => ContentAccessLevel.Read,
                CapabilityType.Write => ContentAccessLevel.Write,
                CapabilityType.ModifyResource => ContentAccessLevel.Write,
                CapabilityType.ModifyContent => ContentAccessLevel.Write,
                CapabilityType.Delete => ContentAccessLevel.Delete,
                CapabilityType.DeleteContent => ContentAccessLevel.Delete,
                CapabilityType.Execute => ContentAccessLevel.Execute,
                CapabilityType.Administer => ContentAccessLevel.Admin,
                CapabilityType.Delegate => ContentAccessLevel.Admin,
                _ => ContentAccessLevel.None
            };

        /// <summary>
        /// Returns true if <paramref name="available"/> satisfies
        /// <paramref name="required"/>, applying the implication rules.
        ///
        /// Admin satisfies everything.
        /// Write satisfies Write and Read.
        /// Delete satisfies Delete and Read.
        /// Execute satisfies Execute only (does NOT imply Read).
        /// </summary>
        public static bool Implies(ContentAccessLevel available, ContentAccessLevel required)
        {
            if (available == ContentAccessLevel.Admin) return true;
            if (available == required) return true;
            if (required == ContentAccessLevel.None) return true;

            return (available, required) switch
            {
                (ContentAccessLevel.Write, ContentAccessLevel.Read) => true,
                (ContentAccessLevel.Delete, ContentAccessLevel.Read) => true,
                (ContentAccessLevel.Delete, ContentAccessLevel.Write) => false, // Delete ≠ Write
                _ => false
            };
        }

        /// <summary>
        /// Returns the highest access level in a collection — useful for computing
        /// the effective access level of a set of ContentBindings on an endpoint.
        /// </summary>
        public static ContentAccessLevel Aggregate(IEnumerable<ContentAccessLevel> levels) =>
            levels.DefaultIfEmpty(ContentAccessLevel.None).Max();
    }
}
