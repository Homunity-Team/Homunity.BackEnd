using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Entities
{
    public class PropertyVideoEntity
    {
        public int VideoId { get; set; }
        public int PropertyId { get; set; }
        public string VideoPath { get; set; }
        public DateTime CreatedAt { get; set; }

        public PropertyEntity Property { get; set; }
    }
}
