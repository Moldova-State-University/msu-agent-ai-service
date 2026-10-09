namespace USMAgent.MigrationService.Extensions;

public static class HostEnvironmentExtensions
{
    public const string LocalEnvironment = "Local";

    public static bool IsLocal(this IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        return hostEnvironment.IsEnvironment(LocalEnvironment);
    }
}