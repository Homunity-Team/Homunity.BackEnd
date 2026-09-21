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
    public class BookingStatusRepository : IBookingStatusRepository
    {
        private readonly HomunityDbContext _db;
        public BookingStatusRepository(HomunityDbContext db) => _db = db;

        public Task<List<BookingStatusEntity>> GetAllAsync() =>
            _db.BookingStatuses.AsNoTracking().OrderBy(s => s.BookingStatusId).ToListAsync();

        public Task<BookingStatusEntity?> FindAsync(int id) =>
            _db.BookingStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.BookingStatusId == id);
    }

}
