using JiuJitsu.Application;
using JiuJitsu.Infrastructure;
using JiuJitsu.Infrastructure.Persistence.Context;
using JiuJitsu.Worker.Consumers;
using JiuJitsu.Worker.Handlers;
using JiuJitsu.Worker.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Aspire — telemetria, health checks e service discovery
builder.AddServiceDefaults();

// PostgreSQL via Aspire
builder.AddNpgsqlDbContext<AppDbContext>("jiujitsu-db");

// RabbitMQ via Aspire
builder.AddRabbitMQClient("rabbitmq", configureConnectionFactory: factory =>
{
    factory.DispatchConsumersAsync = true;
});

// Camada de Application (MediatR — necessário para os handlers financeiros)
builder.Services.AddApplication();

// Camada de Infrastructure (repositórios, email)
builder.Services.AddInfrastructure();
builder.Services.AddEmailConfiguracoes(builder.Configuration);

// Handlers do Worker — atletas (Scoped pois dependem do DbContext)
builder.Services.AddScoped<CriarAtletaHandler>();
builder.Services.AddScoped<AtualizarAtletaHandler>();
builder.Services.AddScoped<ExcluirAtletaHandler>();

// Handlers do Worker — financeiro
builder.Services.AddScoped<GerarMensalidadesHandler>();
builder.Services.AddScoped<AtualizarStatusMensalidadesHandler>();

// Handlers do Worker — notificações
builder.Services.AddScoped<DispararAniversariosHandler>();

// Consumers e Jobs como HostedService (BackgroundService)
builder.Services.AddHostedService<AtletaConsumer>();
builder.Services.AddHostedService<FinanceiroJob>();
builder.Services.AddHostedService<AniversarioJob>();
builder.Services.AddHostedService<NotificacaoConsumer>();

var host = builder.Build();

// Aplica migrations na inicialização
using (var escopo = host.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await contexto.Database.MigrateAsync();
}

await host.RunAsync();
