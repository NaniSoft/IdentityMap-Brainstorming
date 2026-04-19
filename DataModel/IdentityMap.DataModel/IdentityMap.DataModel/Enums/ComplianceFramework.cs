using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Enums
{
    public enum ComplianceFramework
    {
        // Data privacy
        GDPR,               // EU General Data Protection Regulation
        CCPA,               // California Consumer Privacy Act
        PDPA,               // Personal Data Protection Act (India / Singapore variants)

        // Financial
        PCIDSS,             // Payment Card Industry Data Security Standard
        SOX,                // Sarbanes-Oxley Act
        GLBA,               // Gramm-Leach-Bliley Act

        // Healthcare
        HIPAA,              // Health Insurance Portability and Accountability Act

        // Operational security
        ISO27001,           // Information security management
        NIST800_53,         // NIST Special Publication 800-53
        SOC2,               // System and Organisation Controls 2

        // Custom / internal
        Internal            // organisation-defined policy (use SubClassification for name)
    }
}
