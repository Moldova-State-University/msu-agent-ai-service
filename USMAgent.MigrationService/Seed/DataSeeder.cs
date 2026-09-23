using Microsoft.EntityFrameworkCore;
using USMAgent.Infrastructure.Persistence;

namespace USMAgent.MigrationService.Seed;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}

public class DataSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Seed only an empty database, so restarts do not duplicate data.
        if (await dbContext.TimeSlots.AnyAsync(cancellationToken))
        {
            return;
        }

        var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Seed", "seed.sql"), cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
