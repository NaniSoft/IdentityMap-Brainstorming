using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum ContentBindingRole
    {
        /// <summary>
        /// The main data source driving the response body.
        /// E.g. the Customers table for GET /api/customers.
        /// </summary>
        PrimarySource,

        /// <summary>
        /// Supplementary data joined or merged into the response.
        /// E.g. an Orders table enriching a Customer DTO with recent order count.
        /// </summary>
        SecondarySource,

        /// <summary>
        /// Reference / lookup data used to resolve IDs to display values.
        /// E.g. a Countries table resolving country_code → country_name.
        /// </summary>
        LookupSource,

        /// <summary>
        /// The consumer writes data TO this source (POST/PUT/PATCH scenarios).
        /// AccessType on the binding should be Write or ReadWrite.
        /// </summary>
        WriteTarget,

        /// <summary>
        /// A cache or materialised view layer sitting in front of the real source.
        /// Read from the cache; write-through hits the underlying PrimarySource too.
        /// </summary>
        CacheLayer
    }
}
