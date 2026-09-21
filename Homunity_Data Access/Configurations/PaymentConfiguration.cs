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
    public class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentEntity> b)
        {
            b.ToTable("Payments");
            b.HasKey(x => x.PaymentId);
            b.Property(x => x.Amount).HasColumnType("decimal(10,2)").IsRequired();
            b.Property(x => x.MockOrderId).HasMaxLength(120).IsRequired();
            b.Property(x => x.Status).HasMaxLength(50).IsRequired().HasDefaultValue("Pending");
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasOne(x => x.Booking)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<Entities.UserEntity>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Entities.UserEntity>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Entities.PropertyEntity>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
