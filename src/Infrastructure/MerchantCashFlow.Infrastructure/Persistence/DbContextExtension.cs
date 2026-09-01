using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MerchantCashFlow.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public int MaxRetryCount { get; set; } = 5;
    public int MaxRetryDelaySeconds { get; set; } = 10;
}

public static class DbContextExtension
{
    public static IServiceCollection AddMerchantCashFlowDbContextPool<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        int maxPoolSize = 50,
        int minPoolSize = 0) where TDbContext : DbContext
    {
        var conn = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = maxPoolSize,
            MinPoolSize = minPoolSize
        };

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddDbContextPool<TDbContext>(options =>
        {
            options.UseNpgsql(conn.ToString(), builder =>
            {
                builder.EnableRetryOnFailure(databaseOptions.MaxRetryCount, TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds), null);
            }).UseSnakeCaseNamingConvention();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        }, conn.MaxPoolSize);

        return services;
    }
}
