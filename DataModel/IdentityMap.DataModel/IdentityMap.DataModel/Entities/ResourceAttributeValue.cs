namespace IdentityMap.DataModel.Entities
{
    public class ResourceAttributeValue
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ResourceId { get; set; }
        public Guid AttributeDefinitionId { get; set; }

        public string? Value { get; set; }
    }

}
