using Domain.Entites.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class IdempotencyConfiguration : IEntityTypeConfiguration<Idempotency>
    {
        public void Configure(EntityTypeBuilder<Idempotency> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            builder.Property(x => x.IdempotencyKey)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.UserId)
                .HasMaxLength(450);

            builder.Property(x => x.Path)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ResponseBody)
                .IsRequired();

            builder.Property(x => x.StatusCode)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(x => x.HttpMethod)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.HttpMethod, x.Path, x.IdempotencyKey } )
            .IsUnique();
        }
    }
}