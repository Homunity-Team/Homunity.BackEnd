using Homunity_Data_Access.Entities;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<LocationEntity>
    {
        public void Configure(EntityTypeBuilder<LocationEntity> b)
        {
            b.ToTable("Location");
            b.HasKey(x => x.LocationId);
            b.Property(x => x.City).HasMaxLength(20).IsRequired();
            b.Property(x => x.Area).HasMaxLength(50).IsRequired();
            b.Property(x => x.Street).HasMaxLength(50).IsRequired(false); b.Property(x => x.Latitude);
            b.Property(x => x.Longitude);
        }
    }
}
