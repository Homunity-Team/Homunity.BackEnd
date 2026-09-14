using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class PropertyImageEntity
    {
        public int ImageId { get; set; }
        public int PropertyId { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }

        public PropertyEntity Property { get; set; }
    }
}
