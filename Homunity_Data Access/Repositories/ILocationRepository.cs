using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface ILocationRepository
    {
        Task<bool> ExistsAsync(int locationId);
        Task<List<string>> GetAllCitiesAsync();
        Task<List<LocationEntity>> GetAreasByCityAsync(string city);
        Task<int> AddAsync(string city, string area, string? street, double? latitude, double? longitude);
        Task<bool> UpdateAsync(int locationId, string? address, double latitude, double longitude);
        Task<bool> UpdatePropertyUniversityAsync(int propertyId, int universityId);
    }

}
