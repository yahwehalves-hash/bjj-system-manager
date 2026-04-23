// Ponto de entrada do .NET Aspire — orquestra todos os serviços da aplicação
var builder = DistributedApplication.CreateBuilder(args);

// ── Infraestrutura principal ─────────────────────────────────────────────────

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithHostPort(5432);

var bancoDados = postgres.AddDatabase("jiujitsu-db");

var rabbitmq = builder
    .AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var mailhog = builder
    .AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(port: 8025, targetPort: 8025, name: "ui")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

// ── Frontend ─────────────────────────────────────────────────────────────────

var frontend = builder
    .AddNpmApp("frontend", "../jiujitsu-front", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints();

// ── API REST ─────────────────────────────────────────────────────────────────

// ── Asaas — Pagamento Online ─────────────────────────────────────────────────
// Em dev: usar sandbox.asaas.com. Deixar ApiKey vazio para desabilitar.
// Para ativar: crie uma conta sandbox em https://sandbox.asaas.com e cole a API key.
var asaasBaseUrl = builder.Configuration["Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3";
var asaasApiKey  = builder.Configuration["Asaas:ApiKey"]  ?? string.Empty;

// ── API REST ─────────────────────────────────────────────────────────────────

var api = builder
    .AddProject<Projects.JiuJitsu_Api>("api")
    .WithEndpoint("http", e => e.Port = 5207)
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("Asaas__BaseUrl", asaasBaseUrl)
    .WithEnvironment("Asaas__ApiKey", asaasApiKey);

// ── Worker ───────────────────────────────────────────────────────────────────

builder
    .AddProject<Projects.JiuJitsu_Worker>("worker")
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("Asaas__BaseUrl", asaasBaseUrl)
    .WithEnvironment("Asaas__ApiKey", asaasApiKey);

builder.Build().Run();
