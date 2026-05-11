using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbEye.Demo.Data.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("posts");

            builder.HasKey(x => x.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ca => ca.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.HasMany(c => c.Comments)
                .WithOne()
                .HasForeignKey(fk => fk.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}