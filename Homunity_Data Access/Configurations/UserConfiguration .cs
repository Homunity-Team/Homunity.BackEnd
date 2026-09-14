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
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> b)
        {
            b.ToTable("Users");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasColumnName("UserId");
            b.Property(x => x.FirstName).HasColumnName("FirstName").HasMaxLength(20).IsRequired();
            b.Property(x => x.LastName).HasColumnName("LastName").HasMaxLength(20).IsRequired();
            b.Property(x => x.Phone).HasColumnName("Phone").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Phone).IsUnique();
            b.Property(x => x.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(300).IsRequired();
            b.Property(x => x.RoleId).HasColumnName("RoleId");
            b.Property(x => x.IsActive).HasColumnName("IsActive");

            b.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}