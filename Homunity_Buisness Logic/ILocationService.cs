using Homunity_Shared_DTOs.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public enum LocationUpdateStatus { Success, UniversityNotFound, PropertyNotFound, Forbidden, Failed }

    public class LocationUpdateResult
    {
        public LocationUpdateStatus Status { get; set; }
        public int LocationId { get; set; }
        public string? UniversityName { get; set; }
        public double DistanceKm { get; set; }
    }

    public interface ILocationService
    {
        Task<List<string>> GetCitiesAsync();
        Task<List<AreaResponse>> GetAreasByCityAsync(string city);   // كان: List<object>
        Task<LocationUpdateResult> SetPropertyLocationAsync(int propertyId, int universityId, string? address, double lat, double lng, int currentUserId, bool isAdmin);
        Task<LocationUpdateResult> UpdatePropertyLocationAsync(int propertyId, int universityId, string? address, double lat, double lng, int currentUserId, bool isAdmin);
    }


}
