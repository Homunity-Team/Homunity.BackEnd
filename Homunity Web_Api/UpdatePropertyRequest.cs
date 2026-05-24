using System.ComponentModel.DataAnnotations;

namespace Homunity_Web_Api
{
    public class UpdatePropertyRequest
    {
        [Required]
        public int PropertyID { get; set; }

        [Required]
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
         public List<IFormFile>? NewImages { get; set; }
        public List<int>? ImageIdsToDelete { get; set; }
        public IFormFile? NewVideo { get; set; }
        public bool DeleteVideo { get; set; } = false;
        public List<int>? Services { get; set; }
    }
}