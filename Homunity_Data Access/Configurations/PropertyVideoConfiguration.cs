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
    public class PropertyVideoConfiguration : IEntityTypeConfiguration<PropertyVideoEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyVideoEntity> b)
        {
            b.ToTable("PropertyVideo");
            b.HasKey(x => x.VideoId);
            b.Property(x => x.VideoPath).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasOne(x => x.Property)
                .WithMany(p => p.Videos)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
