using System.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Categories.CosmosDb;

public static class DependencyInjection
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IStorage, CosmosStorage>();

        return services;
    }
}