using Domain.Entites.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100); 

            builder.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(c => c.VideoUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.PdfUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(c => c.StartAt)
                .IsRequired();
            
            builder.Property(c => c.EndAt)
                .IsRequired();

            builder.Property(c => c.DayOfWeek)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIMEOFFSET()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(c => c.DeletedAt)
                .IsRequired(false);
                    
            builder.HasQueryFilter(c => !c.IsDeleted);

            // Indexes
            builder.HasIndex(c => new { c.Name, c.CourseId })
                .IsUnique();
        }
    }
}