using Microsoft.EntityFrameworkCore;
using USMAgent.Domain.Entities;

namespace USMAgent.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ScheduleEntry> ScheduleEntries => Set<ScheduleEntry>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseAlias> CourseAliases => Set<CourseAlias>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
        public DbSet<AcademicPeriod> AcademicPeriods => Set<AcademicPeriod>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
