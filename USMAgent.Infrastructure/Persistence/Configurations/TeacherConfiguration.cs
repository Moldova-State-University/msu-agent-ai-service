using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities;

namespace USMAgent.Infrastructure.Persistence.Configurations;
public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FirstName).HasMaxLength(64).IsRequired();
        builder.Property(t => t.LastName).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ShortName).HasMaxLength(64).IsRequired();
        builder.HasIndex(t => t.LastName);
    }
}