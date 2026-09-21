using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.AdminActions
{
    public class AdminPropertyDetailResponse
    {
        public int PropertyId { get; set; }
        public int OwnerId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public int StatusId { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public AdminPropertyLocationInfo Location { get; set; }
        public List<AdminPropertyImageInfo>? Images { get; set; }
        public AdminPropertyVideoInfo? Video { get; set; }
    }

    public class AdminPropertyImageInfo
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class AdminPropertyVideoInfo
    {
        public int VideoId { get; set; }
        public string VideoUrl { get; set; }
    }

}
