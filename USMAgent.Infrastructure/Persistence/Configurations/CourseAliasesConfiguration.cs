using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities.Schedule;

namespace USMAgent.Infrastructure.Persistence.Configurations;

public class CourseAliasConfiguration : IEntityTypeConfiguration<CourseAlias>
{
    public void Configure(EntityTypeBuilder<CourseAlias> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Alias).HasMaxLength(256).IsRequired();

        builder.HasOne(a => a.Course)
            .WithMany(c => c.Aliases)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Alias);
    }
}