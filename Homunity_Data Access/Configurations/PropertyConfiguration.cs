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
    public class PropertyConfiguration : IEntityTypeConfiguration<PropertyEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyEntity> b)
        {
            b.ToTable("Properties");
            b.HasKey(x => x.PropertyId);
            b.Property(x => x.Title).HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).IsRequired(); // nvarchar(max)
            b.Property(x => x.Price).HasColumnType("decimal(10,2)").IsRequired();
            b.Property(x => x.Rooms).IsRequired();
            b.Property(x => x.PropertyType).HasMaxLength(20).IsRequired();
            b.Property(x => x.RejectReason).HasMaxLength(300).IsRequired(false);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.FullAddress).HasMaxLength(300).IsRequired(false);

            // مفيش Cascade في الداتابيز الحالية — لازم نطابق نفس السلوك
            b.HasOne(x => x.Location)
                .WithMany(l => l.Properties)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.University)
                .WithMany()
                .HasForeignKey(x => x.UniversityId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);


          
            b.HasOne(x => x.Owner)
                 .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
