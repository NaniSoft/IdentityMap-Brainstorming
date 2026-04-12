using IdentityMap.DataModel.Enums;

namespace IdentityMap.DataModel.Entities
{
    public class Resource
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }

        public ResourceType Type { get; set; }

        public List<ResourceAttributeValue> Attributes { get; set; } = new();
    }

}
