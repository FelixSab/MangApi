using Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Logging;

public static class Extensions
{
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        services.AddTransient<ILogger, Logger>();
        return services;
    }
}
