using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities;

namespace USMAgent.Infrastructure.Persistence.Configurations;

public class ScheduleEntryConfiguration : IEntityTypeConfiguration<ScheduleEntry>
{
    public void Configure(EntityTypeBuilder<ScheduleEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Parity).HasMaxLength(16);
        builder.Property(e => e.LessonType).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Subgroup).HasMaxLength(16);
        builder.Property(e => e.Specialization).HasMaxLength(128);
        builder.Property(e => e.Alternative).HasMaxLength(128);

        builder.HasOne(e => e.AcademicPeriod)
            .WithMany(p => p.ScheduleEntries)
            .HasForeignKey(e => e.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TimeSlot)
            .WithMany(t => t.ScheduleEntries)
            .HasForeignKey(e => e.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.ScheduleEntries)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Room)
            .WithMany(r => r.ScheduleEntries)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Teachers)
            .WithMany(t => t.ScheduleEntries);

        builder.HasMany(e => e.Groups)
            .WithMany(g => g.ScheduleEntries);

            builder.HasIndex(e => new { e.AcademicPeriodId, e.DayOfWeek, e.TimeSlotId });
    }
}