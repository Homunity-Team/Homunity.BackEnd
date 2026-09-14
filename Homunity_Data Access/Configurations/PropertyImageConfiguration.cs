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
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImageEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyImageEntity> b)
        {
            b.ToTable("PropertyImages");
            b.HasKey(x => x.ImageId);
            b.Property(x => x.ImagePath).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasOne(x => x.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
