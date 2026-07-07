using Domain.Entites.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(c => new { c.StudentId, c.CourseId });

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(c => c.DeletedAt)
                .IsRequired(false);
                            
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}