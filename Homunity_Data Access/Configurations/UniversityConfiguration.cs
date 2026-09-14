using Homunity_Data_Access.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Configurations
{
    public class UniversityConfiguration : IEntityTypeConfiguration<UniversityEntity>
    {
        public void Configure(EntityTypeBuilder<UniversityEntity> b)
        {
            b.ToTable("Universities");
            b.HasKey(x => x.UniversityId);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Latitude).IsRequired();
            b.Property(x => x.Longitude).IsRequired();
        }
    }
}
