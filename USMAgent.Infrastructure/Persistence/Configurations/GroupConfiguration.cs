using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities.Schedule;

namespace USMAgent.Infrastructure.Persistence.Configurations;
public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(32).IsRequired();
        builder.Property(g => g.Language).HasMaxLength(16).IsRequired();
        builder.HasIndex(g => g.Name).IsUnique();
    }
}

