using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class PropertyEntity
    {
        public int PropertyId { get; set; }
        public int OwnerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }
        public int LocationId { get; set; }
        public int StatusId { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UniversityId { get; set; }
        public string? FullAddress { get; set; }

        public LocationEntity Location { get; set; }
        public UniversityEntity University { get; set; }
        public List<PropertyImageEntity> Images { get; set; } = new();
        public List<PropertyVideoEntity> Videos { get; set; } = new();
        public List<PropertyServiceEntity> PropertyServices { get; set; } = new();
    }
}
