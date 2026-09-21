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
    public class UniversityRepository : IUniversityRepository
    {
        private readonly HomunityDbContext _db;
        public UniversityRepository(HomunityDbContext db) => _db = db;

        public Task<List<UniversityEntity>> GetAllAsync() => _db.Universities.AsNoTracking().OrderBy(u => u.Name).ToListAsync();

        public Task<UniversityEntity?> GetByIdAsync(int universityId) =>
            _db.Universities.AsNoTracking().FirstOrDefaultAsync(u => u.UniversityId == universityId);

        public async Task<List<PropertyEntity>> SearchByUniversityIdAsync(int universityId, decimal? maxPrice)
        {
            var query = _db.Properties.AsNoTracking()
                .Include(p => p.Location)
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .Include(p => p.PropertyServices).ThenInclude(ps => ps.Service)
                .Where(p => p.StatusId == 2 && p.UniversityId == universityId)
                .Where(p => !_db.Bookings.Any(b => b.PropertyId == p.PropertyId && b.StatusId == 3));

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.ToListAsync();
        }
    }

}
