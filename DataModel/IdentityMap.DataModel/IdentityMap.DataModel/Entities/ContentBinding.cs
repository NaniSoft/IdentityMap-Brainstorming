using IdentityMap.DataModel.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMap.DataModel.Entities
{
    public class ContentBinding : BaseEntity
    {
        public ContentBindingAccessType AccessType { get; set; }
            = ContentBindingAccessType.Read;

        public ContentBindingRole Role { get; set; }
            = ContentBindingRole.PrimarySource;

        public string? ContributionDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }

        public Guid ConsumerResourceId { get; set; }
        public Resource ConsumerResource { get; set; } = null!;
        public Guid ContentSourceId { get; set; }
        public Resource ContentSource { get; set; } = null!;
        public Guid? AccessorResourceId { get; set; }
        public Resource? AccessorResource { get; set; }
    }
}
