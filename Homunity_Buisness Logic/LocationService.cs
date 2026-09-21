using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepo;
        private readonly IUniversityRepository _universityRepo;
        private readonly IPropertyRepository _propertyRepo;

        public LocationService(ILocationRepository locationRepo, IUniversityRepository universityRepo, IPropertyRepository propertyRepo)
        {
            _locationRepo = locationRepo;
            _universityRepo = universityRepo;
            _propertyRepo = propertyRepo;
        }

        public Task<List<string>> GetCitiesAsync() => _locationRepo.GetAllCitiesAsync();

        public async Task<List<AreaResponse>> GetAreasByCityAsync(string city)
        {
            var areas = await _locationRepo.GetAreasByCityAsync(city);
            return areas.Select(a => new AreaResponse
            {
                LocationId = a.LocationId,
                Area = a.Area,
                Street = a.Street,
                Latitude = a.Latitude,
                Longitude = a.Longitude
            }).ToList();
        }

        private async Task<LocationUpdateResult> ChangeLocationAsync(int propertyId, int universityId, string? address, double lat, double lng, int currentUserId, bool isAdmin)
        {
            var university = await _universityRepo.GetByIdAsync(universityId);
            if (university == null) return new LocationUpdateResult { Status = LocationUpdateStatus.UniversityNotFound };

            var ownership = await _propertyRepo.GetOwnershipAsync(propertyId);
            if (ownership == null) return new LocationUpdateResult { Status = LocationUpdateStatus.PropertyNotFound };

            if (ownership.Value.OwnerId != currentUserId && !isAdmin)
                return new LocationUpdateResult { Status = LocationUpdateStatus.Forbidden };

            double distance = UniversityService.CalculateDistance(lat, lng, university.Latitude, university.Longitude);

            if (!await _locationRepo.UpdateAsync(ownership.Value.LocationId, address, lat, lng))
                return new LocationUpdateResult { Status = LocationUpdateStatus.Failed };

            if (!await _locationRepo.UpdatePropertyUniversityAsync(propertyId, universityId))
                return new LocationUpdateResult { Status = LocationUpdateStatus.Failed };

            return new LocationUpdateResult
            {
                Status = LocationUpdateStatus.Success,
                LocationId = ownership.Value.LocationId,
                UniversityName = university.Name,
                DistanceKm = distance
            };
        }

        public Task<LocationUpdateResult> SetPropertyLocationAsync(int propertyId, int universityId, string? address, double lat, double lng, int currentUserId, bool isAdmin) =>
            ChangeLocationAsync(propertyId, universityId, address, lat, lng, currentUserId, isAdmin);

        public Task<LocationUpdateResult> UpdatePropertyLocationAsync(int propertyId, int universityId, string? address, double lat, double lng, int currentUserId, bool isAdmin) =>
            ChangeLocationAsync(propertyId, universityId, address, lat, lng, currentUserId, isAdmin);
    }

}
