using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class PropertyServiceEntity
    {
        public int PropertyServicesId { get; set; }
        public int PropertyId { get; set; }
        public int ServiceId { get; set; }

        public PropertyEntity Property { get; set; }
        public ServiceEntity Service { get; set; }
    }
}
