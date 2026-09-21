using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.AdminActions
{
    public class AdminPendingPropertyResponse
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }
        public string? OwnerName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public AdminPropertyLocationInfo Location { get; set; }
        public string? Thumbnail { get; set; }
    }

}
