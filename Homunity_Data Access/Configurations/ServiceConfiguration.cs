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
    public class ServiceConfiguration : IEntityTypeConfiguration<ServiceEntity>
    {
        public void Configure(EntityTypeBuilder<ServiceEntity> b)
        {
            b.ToTable("Services");
            b.HasKey(x => x.ServiceId);
            b.Property(x => x.Name).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Icon).HasMaxLength(100).IsRequired(false);
        }
    }
}
