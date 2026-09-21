using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories.Models
{
    public class DashboardStatsResult
    {
        public int TotalProperties { get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
    }

    public class AdminPropertyProjection
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = "";
        public string? OwnerName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; } = "";
        public int StatusId { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LocationId { get; set; }
        public string City { get; set; } = "";
        public string Area { get; set; } = "";
        public string? Thumbnail { get; set; }
    }

    public class AdminPropertyDetail
    {
        public int PropertyId { get; set; }
        public int OwnerId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string PropertyType { get; set; } = "";
        public int StatusId { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LocationId { get; set; }
        public string City { get; set; } = "";
        public string Area { get; set; } = "";
        public List<(int ImageId, string ImagePath)> Images { get; set; } = new();
        public (int VideoId, string VideoPath)? Video { get; set; }
    }

}
