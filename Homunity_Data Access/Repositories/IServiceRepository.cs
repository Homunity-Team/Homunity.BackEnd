using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IServiceRepository
    {
        Task<List<ServiceEntity>> GetAllAsync();
        Task<ServiceEntity?> FindByIdAsync(int serviceId);
        Task<bool> ExistsAsync(int serviceId);
    }

}
