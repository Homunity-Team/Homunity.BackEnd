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
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessageEntity>
    {
        public void Configure(EntityTypeBuilder<ChatMessageEntity> b)
        {
            b.ToTable("ChatMessages");
            b.HasKey(x => x.MessageId);
            b.Property(x => x.Role).HasMaxLength(20).IsRequired();
            b.Property(x => x.Content).IsRequired();
        }
    }
}
