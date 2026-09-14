using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Properties
{
    public class PropertyListItemDto
    {
        public int PropertyID { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MainImageUrl { get; set; }
        public int? UniversityId { get; set; }
        public string? UniversityName { get; set; }
    }
}
