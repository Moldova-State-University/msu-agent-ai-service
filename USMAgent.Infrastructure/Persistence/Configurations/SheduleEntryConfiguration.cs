using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using USMAgent.Domain.Entities.Schedule;

namespace USMAgent.Infrastructure.Persistence.Configurations;

public class ScheduleEntryConfiguration : IEntityTypeConfiguration<ScheduleEntry>
{
    public void Configure(EntityTypeBuilder<ScheduleEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Parity).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.LessonType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.Subgroup).HasMaxLength(16);
        builder.Property(e => e.Specialization).HasMaxLength(128);
        builder.Property(e => e.Alternative).HasMaxLength(128);

        builder.HasOne(e => e.AcademicPeriod)
            .WithMany(p => p.ScheduleEntries)
            .HasForeignKey("AcademicPeriodId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TimeSlot)
            .WithMany(t => t.ScheduleEntries)
            .HasForeignKey("TimeSlotId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.ScheduleEntries)
            .HasForeignKey("CourseId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Room)
            .WithMany(r => r.ScheduleEntries)
            .HasForeignKey("RoomId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Teachers)
            .WithMany(t => t.ScheduleEntries);

        builder.HasMany(e => e.Groups)
            .WithMany(g => g.ScheduleEntries);

        builder.HasIndex("AcademicPeriodId", nameof(ScheduleEntry.DayOfWeek), "TimeSlotId");
    }
}