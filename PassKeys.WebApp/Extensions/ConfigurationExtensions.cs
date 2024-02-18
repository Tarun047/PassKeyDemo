using Npgsql;

namespace PassKeys.WebApp.Extensions;

public static class ConfigurationExtensions
{
    const string DockerDev = "DockerDev";
    
    public static string GetDbConnectionString(this IConfiguration configuration)
    {
        var connectionBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["Database:Host"],
            Port = int.Parse(configuration["Database:Port"]!),
            Database = configuration["Database:Name"],
            Username = configuration["Database:User"],
            Password = configuration["Database:Password"]
        };
        return connectionBuilder.ConnectionString;
    }

    public static bool IsDockerDevelopment(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment(DockerDev);
    }
}