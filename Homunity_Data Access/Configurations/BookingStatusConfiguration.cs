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
    public class BookingStatusConfiguration : IEntityTypeConfiguration<BookingStatusEntity>
    {
        public void Configure(EntityTypeBuilder<BookingStatusEntity> b)
        {
            b.ToTable("BookingStatus");
            b.HasKey(x => x.BookingStatusId);
            b.Property(x => x.StatusName).HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.StatusName).IsUnique();
        }
    }
}
