using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Homunity_Web_Api.Contracts
{
    public class CreatePropertyRequest
    {
        [Required, StringLength(150, MinimumLength = 3)]
        public string Title { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required, Range(1, 50)]
        public int Rooms { get; set; }

        [Required]
        [RegularExpression("^(Apartment|Room)$", ErrorMessage = "PropertyType must be 'Apartment' or 'Room'.")]
        public string PropertyType { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public int UniversityId { get; set; }

        public List<IFormFile> Images { get; set; }

        public IFormFile? Video { get; set; }

        public List<int> Services { get; set; }
    }
}