using USMAgent.Infrastructure.Persistence;
using USMAgent.MigrationService;
using USMAgent.MigrationService.Seed;

var builder = Host.CreateApplicationBuilder(args);

builder.AddNpgsqlDbContext<ApplicationDbContext>("USMAgentDb");

builder.Services.AddScoped<IDataSeeder, DataSeeder>();
builder.Services.AddHostedService<DbMigrator>();

builder.Build().Run();
