using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly HomunityDbContext _db;
        public ServiceRepository(HomunityDbContext db) => _db = db;

        public Task<List<ServiceEntity>> GetAllAsync() => _db.Services.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        public Task<ServiceEntity?> FindByIdAsync(int serviceId) => _db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == serviceId);
        public Task<bool> ExistsAsync(int serviceId) => _db.Services.AsNoTracking().AnyAsync(s => s.ServiceId == serviceId);
    }

}
