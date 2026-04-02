using JiuJitsu.Application.Commands.CriarAtleta;
using Microsoft.Extensions.DependencyInjection;

namespace JiuJitsu.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra todos os handlers do MediatR a partir do assembly da Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CriarAtletaCommand).Assembly));

        return services;
    }
}
