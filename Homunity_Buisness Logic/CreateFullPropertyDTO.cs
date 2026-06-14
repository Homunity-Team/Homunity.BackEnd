using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    /*
    public class CreateFullPropertyDTO
    {
        public int OwnerID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }

        // ✅ بيانات الموقع
        public string City { get; set; }
        public string Area { get; set; }
        public string Street { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public int UniversityId { get; set; }

        public List<string> Images { get; set; }
        public string VideoUrl { get; set; }
        public List<long> ImageSizes { get; set; }
        public long VideoSize { get; set; }
        public List<int> Services { get; set; }
    }
*/
    public class CreateFullPropertyDTO
    {
        public int OwnerID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }

        // Location
        public string City { get; set; }
        public string Area { get; set; }
        public string Street { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public int UniversityId { get; set; }

        // Media
        public List<string> Images { get; set; }
        public List<long> ImageSizes { get; set; }
        public string VideoUrl { get; set; }
        public long VideoSize { get; set; }

        // Services
        public List<int> Services { get; set; }
    }

}
