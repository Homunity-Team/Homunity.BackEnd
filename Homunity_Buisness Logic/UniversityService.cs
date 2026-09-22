using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Homunity_Buisness_Logic
{
    public class UniversityService : IUniversityService
    {
        private readonly IUniversityRepository _repo;
        public UniversityService(IUniversityRepository repo) => _repo = repo;

        public Task<List<UniversityEntity>> GetAllAsync() => _repo.GetAllAsync();

        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(R * c, 2);
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        public async Task<List<PropertyWithDistanceDto>> SearchByUniversityAsync(int universityId, decimal? maxPrice, double? maxDistanceKm = null)
        {
            var university = await _repo.GetByIdAsync(universityId);
            if (university == null) return new List<PropertyWithDistanceDto>();

            var properties = await _repo.SearchByUniversityIdAsync(universityId, maxPrice);
            var list = new List<PropertyWithDistanceDto>();

            foreach (var property in properties)
            {
                double? distance = null;
                if (property.Location?.Latitude != null && property.Location?.Longitude != null)
                    distance = CalculateDistance(property.Location.Latitude.Value, property.Location.Longitude.Value, university.Latitude, university.Longitude);

                if (maxDistanceKm.HasValue && distance.HasValue && distance > maxDistanceKm.Value) continue;

                list.Add(new PropertyWithDistanceDto
                {
                    Property = property,
                    UniversityId = universityId,
                    UniversityName = university.Name,
                    UniversityLat = university.Latitude,
                    UniversityLon = university.Longitude,
                    DistanceKm = distance
                });
            }

            return list.OrderBy(x => x.DistanceKm ?? double.MaxValue).ToList();
        }

 



        public static PropertySearchResponse BuildResponse(PropertyWithDistanceDto r, string baseUrl)   // كان: static object
        {
            var property = r.Property;
            var video = property.Videos?.OrderByDescending(v => v.CreatedAt).FirstOrDefault();

            return new PropertySearchResponse
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Rooms = property.Rooms,
                PropertyType = property.PropertyType,
                StatusId = property.StatusId,
                CreatedAt = property.CreatedAt,
                Location = new PropertySearchLocationInfo
                {
                    LocationId = property.LocationId,
                    City = property.Location?.City,
                    Area = property.Location?.Area,
                    Street = property.Location?.Street,
                    Latitude = property.Location?.Latitude,
                    Longitude = property.Location?.Longitude
                },
                University = new PropertySearchUniversityInfo
                {
                    UniversityId = r.UniversityId,
                    UniversityName = r.UniversityName,
                    Lat = r.UniversityLat,
                    Lon = r.UniversityLon,
                    DistanceKm = r.DistanceKm
                },
                Images = property.Images?.Select(img => new PropertySearchImageInfo { ImageId = img.ImageId, ImageUrl = $"{baseUrl}/{img.ImagePath}" }).ToList(),
                Video = video == null ? null : new PropertySearchVideoInfo { VideoId = video.VideoId, VideoUrl = $"{baseUrl}/{video.VideoPath}" },
                Services = property.PropertyServices?.Select(ps => new PropertySearchServiceInfo { ServiceId = ps.Service.ServiceId, Name = ps.Service.Name, Icon = ps.Service.Icon }).ToList()
            };
        }



    }

}
