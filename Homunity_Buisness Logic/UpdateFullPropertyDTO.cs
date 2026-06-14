using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    /*
    public class UpdateFullPropertyDTO
    {
        public int PropertyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }

        // ✅ نفس الـ Add بالظبط
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public int UniversityId { get; set; }

        public List<string> NewImages { get; set; }
        public List<long> NewImageSizes { get; set; }
        public List<int> ImageIdsToDelete { get; set; }
        public string NewVideoUrl { get; set; }
        public long NewVideoSize { get; set; }
        public bool DeleteVideo { get; set; }
        public List<int> Services { get; set; }
    }
*/

    public class UpdateFullPropertyDTO
    {
        public int PropertyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }

        // Location
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public int UniversityId { get; set; }

        // Media
        public List<string> NewImages { get; set; }
        public List<long> NewImageSizes { get; set; }
        public List<int> ImageIdsToDelete { get; set; }
        public string NewVideoUrl { get; set; }
        public long NewVideoSize { get; set; }
        public bool DeleteVideo { get; set; }

        // Services
        public List<int> Services { get; set; }
    }

}
