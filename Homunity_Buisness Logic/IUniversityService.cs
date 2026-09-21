using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class PropertyWithDistanceDto
    {
        public PropertyEntity Property { get; set; } = null!;
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = "";
        public double UniversityLat { get; set; }
        public double UniversityLon { get; set; }
        public double? DistanceKm { get; set; }
    }

    public interface IUniversityService
    {
        Task<List<UniversityEntity>> GetAllAsync();
        Task<List<PropertyWithDistanceDto>> SearchByUniversityAsync(int universityId, decimal? maxPrice, double? maxDistanceKm = null);
    }

}
