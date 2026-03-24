using EffortEstimator.Core.Application.Interfaces.Services;
using EffortEstimator.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EffortEstimator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEstimationEngine, PertEngine>();
        return services;
    }
}
