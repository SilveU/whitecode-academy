using Domain.Entites.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired()
                .HasMaxLength(450);   // matches ASP.NET Identity key length

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);    // "Create", "Update", "Delete"

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityId)
                .IsRequired();

            builder.Property(a => a.OldValues)
                .IsRequired(false);   // null for Create actions

            builder.Property(a => a.NewValues)
                .IsRequired(false);   // null for Delete actions

            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.IpAddress)
                .IsRequired(false)
                .HasMaxLength(45);    // max IPv6 length

            // Indexes for common query patterns
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => new { a.EntityName, a.EntityId });
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}
