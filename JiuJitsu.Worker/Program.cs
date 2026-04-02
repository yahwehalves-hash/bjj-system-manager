using JiuJitsu.Infrastructure;
using JiuJitsu.Infrastructure.Persistence.Context;
using JiuJitsu.Worker.Consumers;
using JiuJitsu.Worker.Handlers;
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

// Camada de Infrastructure (repositórios, email)
builder.Services.AddInfrastructure();
builder.Services.AddEmailConfiguracoes(builder.Configuration);

// Handlers do Worker (Scoped pois dependem do DbContext)
builder.Services.AddScoped<CriarAtletaHandler>();
builder.Services.AddScoped<AtualizarAtletaHandler>();
builder.Services.AddScoped<ExcluirAtletaHandler>();

// Consumer como HostedService (BackgroundService)
builder.Services.AddHostedService<AtletaConsumer>();

var host = builder.Build();

// Aplica migrations na inicialização
using (var escopo = host.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await contexto.Database.MigrateAsync();
}

await host.RunAsync();
