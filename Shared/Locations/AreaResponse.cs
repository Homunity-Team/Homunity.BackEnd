using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Locations
{
    public class AreaResponse
    {
        public int LocationId { get; set; }
        public string? Area { get; set; }
        public string? Street { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
