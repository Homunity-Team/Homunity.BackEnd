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
    public class RoleRepository : IRoleRepository
    {
        private readonly HomunityDbContext _db;
        public RoleRepository(HomunityDbContext db) => _db = db;
        public Task<List<RoleEntity>> GetAllAsync() => _db.Roles.AsNoTracking().ToListAsync();
    }
}
