using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class CreatePropertyRequest
{
    [Required]
    public int OwnerID { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public int Rooms { get; set; }

    public string PropertyType { get; set; }

    public int LocationID { get; set; }

    // ✅ NEW: Location details from map picker
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }
    public int UniversityId { get; set; }

    public List<IFormFile> Images { get; set; }

    public IFormFile? Video { get; set; }

    public List<int> Services { get; set; }
}