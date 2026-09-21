using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IUniversityRepository
    {
        Task<List<UniversityEntity>> GetAllAsync();
        Task<UniversityEntity?> GetByIdAsync(int universityId);
        Task<List<PropertyEntity>> SearchByUniversityIdAsync(int universityId, decimal? maxPrice);
    }

}
