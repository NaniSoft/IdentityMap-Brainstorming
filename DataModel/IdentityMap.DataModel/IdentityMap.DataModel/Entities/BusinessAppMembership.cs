namespace IdentityMap.DataModel.Entities
{
    public class BusinessAppMembership : BaseEntity
    {
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;

        public Guid BusinessAppResourceId { get; set; }
        public Resource BusinessAppResource { get; set; } = null!;

        public Guid MemberResourceId { get; set; }
        public Resource MemberResource { get; set; } = null!;
    }
}