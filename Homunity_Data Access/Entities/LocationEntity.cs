using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class LocationEntity
    {
        public int LocationId { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Street { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<PropertyEntity> Properties { get; set; } = new();
    }
}
