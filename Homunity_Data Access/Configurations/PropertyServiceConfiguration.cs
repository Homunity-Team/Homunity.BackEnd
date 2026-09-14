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
    public class PropertyServiceConfiguration : IEntityTypeConfiguration<PropertyServiceEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyServiceEntity> b)
        {
            b.ToTable("PropertyServices");
            b.HasKey(x => x.PropertyServicesId);
            b.HasIndex(x => new { x.PropertyId, x.ServiceId }).IsUnique();

            b.HasOne(x => x.Property)
                .WithMany(p => p.PropertyServices)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Service)
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
