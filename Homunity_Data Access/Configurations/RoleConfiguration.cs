using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Homunity_Data_Access.Entities;


namespace Homunity_Data_Access.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> b)
        {
            b.ToTable("Roles");
            b.HasKey(x => x.RoleId);
            b.Property(x => x.Name).HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        }
    }
}
