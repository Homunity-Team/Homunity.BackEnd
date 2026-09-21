using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories.Models
{
    public class PropertyImageProjection
    {
        public int ImageId { get; set; }
        public int PropertyId { get; set; }
        public string ImagePath { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class PropertyVideoProjection
    {
        public int VideoId { get; set; }
        public int PropertyId { get; set; }
        public string VideoPath { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
