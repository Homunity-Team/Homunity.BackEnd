using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories.Models
{
    public class PropertyUpdateCommand
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; }
        public string FullAddress { get; set; }
        public int? UniversityId { get; set; }

        public bool UpdateLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationStreet { get; set; }

        public List<int> ImageIdsToDelete { get; set; } = new();
        public List<string> NewImagePaths { get; set; } = new();

        // null = متلمسش الخدمات إطلاقًا (نفس سلوك dto.Services == null في الأصل)
        public List<int> ServiceIds { get; set; }
    }
}
