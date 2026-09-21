using Homunity_Data_Access.Configurations;
using Homunity_Data_Access.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Data
{
        public class HomunityDbContext : DbContext
    {
        public HomunityDbContext(DbContextOptions<HomunityDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<PropertyEntity> Properties => Set<PropertyEntity>();
        public DbSet<LocationEntity> Locations => Set<LocationEntity>();
        public DbSet<UniversityEntity> Universities => Set<UniversityEntity>();
        public DbSet<ServiceEntity> Services => Set<ServiceEntity>();
        public DbSet<PropertyImageEntity> PropertyImages => Set<PropertyImageEntity>();
        public DbSet<PropertyVideoEntity> PropertyVideos => Set<PropertyVideoEntity>();
        public DbSet<PropertyServiceEntity> PropertyServices => Set<PropertyServiceEntity>();
        public DbSet<RoleEntity> Roles => Set<RoleEntity>();
        public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
        public DbSet<BookingStatusEntity> BookingStatuses => Set<BookingStatusEntity>();
        public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
        public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new UniversityConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyImageConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyVideoConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyServiceConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
            modelBuilder.ApplyConfiguration(new BookingStatusConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
        }
    }
 
}
