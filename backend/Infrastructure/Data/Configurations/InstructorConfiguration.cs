using Domain.Entites.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasKey(c => c.Id);

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

            builder.HasOne(i => i.User)
                .WithOne()
                .HasForeignKey<Instructor>(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}