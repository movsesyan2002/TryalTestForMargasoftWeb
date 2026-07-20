using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TryalTestForMargasoftInfrastrcture.Data;
using TryalTestForMargasoftInfrastrcture.Repositories;

namespace TryalTestForMargasoftInfrastrcture;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the PostgreSQL database context and EF repositories used by the claim workflow.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A database connection string is required.", nameof(connectionString));
        }

        services.AddDbContext<TryalTestDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<EfMedicalClaimRepository>();
        services.AddScoped<EfClaimRecommendationRepository>();

        return services;
    }
}
