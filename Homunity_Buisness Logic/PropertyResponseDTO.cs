using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // DTO للـ Read فقط - مش بيتبعت في الـ Request
    public class PropertyResponseDTO
    {
        public int PropertyID { get; set; }
        public int OwnerID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public int PropertyStatusID { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public object? Location { get; set; }
        public object? Images { get; set; }
        public object? Video { get; set; }
        public object? Services { get; set; }
        public string? FullAddress { get; set; } // ✅ NEW


        public static PropertyResponseDTO FromProperty(
            clsProperties property,
            string baseUrl,
            List<clsPropertyImages> images,
            clsPropertyVideo video,
            List<clsPropertyServices> services)
        {
            return new PropertyResponseDTO
            {
                PropertyID = property.PropertyID,
                OwnerID = property.OwnerID,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Rooms = property.Rooms,
                PropertyType = property.PropertyType,
                PropertyStatusID = property.PropertyStatusID,
                RejectReason = property.RejectReason,
                CreatedAt = property.CreatedAt,

                Location = new
                {
                    locationId = property.LocationID,
                    city = property.City,
                    area = property.Area,
                    street = property.Street, //  جديد
                    latitude = property.Latitude,   // ✅
                    longitude = property.Longitude

                },

                Images = images == null || images.Count == 0
                    ? null
                    : (object)images.Select(img => new
                    {
                        imageId = img.ImageId,
                        imageUrl = $"{baseUrl}/{img.ImagePath}"
                    }).ToList(),

                Video = video == null
                    ? null
                    : (object)new
                    {
                        videoId = video.VideoId,
                        videoUrl = $"{baseUrl}/{video.VideoPath}"
                    },

                // ✅ Services
                Services = services == null || services.Count == 0
                    ? null
                    : (object)services.Select(s => new
                    {
                        serviceId = s.ServiceId,
                        name = s.Name,
                        icon = s.Icon
                    }).ToList()

                
            };
        }



        public static PropertyResponseDTO FromPropertyV2(clsProperties property, string baseUrl,
              List<clsPropertyImages> images, clsPropertyVideo video, List<clsPropertyServices> services)
        {
            // ✅ بناء العنوان الكامل
            string fullAddress = property.Address;
            if (string.IsNullOrWhiteSpace(fullAddress))
            {
                var parts = new[] { property.City, property.Area, property.Street };
                fullAddress = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            return new PropertyResponseDTO
            {
                PropertyID = property.PropertyID,
                OwnerID = property.OwnerID,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Rooms = property.Rooms,
                PropertyType = property.PropertyType,
                PropertyStatusID = property.PropertyStatusID,
                RejectReason = property.RejectReason,
                CreatedAt = property.CreatedAt,
                FullAddress = fullAddress, // ✅ NEW
                Location = new
                {
                    locationId = property.LocationID,
                    city = property.City,
                    area = property.Area,
                    street = property.Street,
                    address = property.Address, // ✅ include full address in location object too
                    latitude = property.Latitude,
                    longitude = property.Longitude,
                    university = property.UniversityId == null ? null : new
                    {
                        universityId = property.UniversityId,
                        name = property.UniversityName,
                        distance_km = property.DistanceKm
                    }
                },
                Images = images?.Select(img => new { imageId = img.ImageId, imageUrl = $"{baseUrl}/{img.ImagePath}" }).ToList(),
                Video = video == null ? null : new { videoId = video.VideoId, videoUrl = $"{baseUrl}/{video.VideoPath}" },
                Services = services?.Select(s => new { serviceId = s.ServiceId, name = s.Name, icon = s.Icon }).ToList()
            };
        }
    }
}
