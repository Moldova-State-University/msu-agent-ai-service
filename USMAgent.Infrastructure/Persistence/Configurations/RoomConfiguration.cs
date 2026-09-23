using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities.Schedule;

namespace USMAgent.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.Name).HasMaxLength(64).IsRequired();
    }
}


