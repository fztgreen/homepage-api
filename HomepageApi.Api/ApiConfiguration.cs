using Microsoft.Extensions.DependencyInjection;

namespace HomepageApi.Api;

public static class ApiConfiguration
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
