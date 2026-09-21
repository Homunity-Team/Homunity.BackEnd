using System;
using System.Collections.Generic;

namespace Homunity_Web_Api.Properties
{
    public class PropertySearchResponse
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PropertySearchLocationInfo Location { get; set; }
        public PropertySearchUniversityInfo University { get; set; }
        public List<PropertySearchImageInfo>? Images { get; set; }
        public PropertySearchVideoInfo? Video { get; set; }
        public List<PropertySearchServiceInfo>? Services { get; set; }
    }

    public class PropertySearchLocationInfo
    {
        public int LocationId { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Street { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class PropertySearchUniversityInfo
    {
        public int UniversityId { get; set; }
        public string? UniversityName { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double? DistanceKm { get; set; }
    }

    public class PropertySearchImageInfo
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class PropertySearchVideoInfo
    {
        public int VideoId { get; set; }
        public string VideoUrl { get; set; }
    }

    public class PropertySearchServiceInfo
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string? Icon { get; set; }
    }

}
