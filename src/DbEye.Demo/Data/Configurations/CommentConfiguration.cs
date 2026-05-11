using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbEye.Demo.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("comments");

            builder.HasKey(x => x.Id);

            builder.Property(pi => pi.PostId)
                .IsRequired();

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(ca => ca.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");
        }
    }
}