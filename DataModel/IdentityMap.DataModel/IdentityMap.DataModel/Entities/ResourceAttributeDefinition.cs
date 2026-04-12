namespace IdentityMap.DataModel.Entities
{
    using IdentityMap.DataModel.Enums;

    public class ResourceAttributeDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }
        public AttributeDataType DataType { get; set; }
    }

}
