using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories.Models
{
    public class BookingDetails
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int PropertyOwnerId { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? PropertyTitle { get; set; }
        public decimal PropertyPrice { get; set; }
        public string? PropertyAddress { get; set; }
        public string? PropertyImagePath { get; set; }
    }

    public class BookingListItem
    {
        public int BookingId { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public int PropertyId { get; set; }
        public string? PropertyTitle { get; set; }
        public decimal PropertyPrice { get; set; }
        public string? PropertyAddress { get; set; }
        public string? ImagePath { get; set; }
        public string? StudentName { get; set; }
    }

    public class BookingByPropertyItem
    {
        public int BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? StudentName { get; set; }
        public string? StatusName { get; set; }
    }

}
