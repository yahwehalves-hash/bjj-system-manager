using Dapper;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Email;
using JiuJitsu.Infrastructure.Email.Configuracoes;
using JiuJitsu.Infrastructure.Messaging;
using JiuJitsu.Infrastructure.Persistence.Repositories;
using JiuJitsu.Infrastructure.ReadModel;
using Microsoft.Extensions.DependencyInjection;

namespace JiuJitsu.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Registra handler de tipo para DateOnly no Dapper
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        // Repositórios
        services.AddScoped<IAtletaRepository, AtletaRepository>();
        services.AddScoped<IAtletaReadRepository, AtletaReadRepository>();

        // Mensageria
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        // Email
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddEmailConfiguracoes(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<EmailConfiguracoes>(configuration.GetSection("Email"));
        return services;
    }
}
