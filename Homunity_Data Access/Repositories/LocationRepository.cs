using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Homunity_Data_Access.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly HomunityDbContext _db;
        public LocationRepository(HomunityDbContext db) => _db = db;

        public Task<bool> ExistsAsync(int locationId) => _db.Locations.AsNoTracking().AnyAsync(l => l.LocationId == locationId);

        public Task<List<string>> GetAllCitiesAsync() =>
            _db.Locations.AsNoTracking().Select(l => l.City!).Distinct().OrderBy(c => c).ToListAsync();

        public Task<List<LocationEntity>> GetAreasByCityAsync(string city) =>
            _db.Locations.AsNoTracking().Where(l => l.City == city).OrderBy(l => l.Area).ToListAsync();

        public async Task<int> AddAsync(string city, string area, string? street, double? latitude, double? longitude)
        {
            var location = new LocationEntity { City = city, Area = area, Street = street, Latitude = latitude, Longitude = longitude };
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();
            return location.LocationId;
        }

        public async Task<bool> UpdateAsync(int locationId, string? address, double latitude, double longitude)
        {
            var location = await _db.Locations.FirstOrDefaultAsync(l => l.LocationId == locationId);
            if (location == null) return false;
            location.Street = address;
            location.Latitude = latitude;
            location.Longitude = longitude;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePropertyUniversityAsync(int propertyId, int universityId)
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            if (property == null) return false;
            property.UniversityId = universityId;
            return await _db.SaveChangesAsync() > 0;
        }
    }

}
