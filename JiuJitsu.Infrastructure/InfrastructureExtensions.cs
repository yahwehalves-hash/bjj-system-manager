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

        // Repositórios — escrita (EF Core)
        services.AddScoped<IAtletaRepository, AtletaRepository>();
        services.AddScoped<IHistoricoGraduacaoRepository, HistoricoGraduacaoRepository>();
        services.AddScoped<IFilialRepository, FilialRepository>();
        services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
        services.AddScoped<IMensalidadeRepository, MensalidadeRepository>();
        services.AddScoped<IDespesaRepository, DespesaRepository>();

        // Repositórios — leitura (Dapper)
        services.AddScoped<IAtletaReadRepository, AtletaReadRepository>();
        services.AddScoped<IHistoricoGraduacaoReadRepository, HistoricoGraduacaoReadRepository>();
        services.AddScoped<IFilialReadRepository, FilialReadRepository>();
        services.AddScoped<IConfiguracaoReadRepository, ConfiguracaoReadRepository>();
        services.AddScoped<IMensalidadeReadRepository, MensalidadeReadRepository>();
        services.AddScoped<IDespesaReadRepository, DespesaReadRepository>();
        services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();

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
