namespace IdentityMap.DataModel.Enums
{
    public enum OwnerType
    {
        // Existing
        Human,              // a named individual (Account resource)
        BusinessApp,        // a BusinessApp container; owners are its Owner-role members
        Group,              // an AD/LDAP group; owner is resolved via membership

        // NEW ↓
        DataSteward,        // accountable for data quality and classification decisions
        TechnicalOwner,     // the team/person who built and operates the resource
        Custodian           // holds day-to-day stewardship without full ownership rights
    }
}