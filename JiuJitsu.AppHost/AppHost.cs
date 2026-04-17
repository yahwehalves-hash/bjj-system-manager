// Ponto de entrada do .NET Aspire — orquestra todos os serviços da aplicação
var builder = DistributedApplication.CreateBuilder(args);

// Banco de dados PostgreSQL
var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume();          // Persiste os dados entre restarts (porta dinâmica — veja no Aspire Dashboard)

var bancoDados = postgres.AddDatabase("jiujitsu-db");

// Mensageria RabbitMQ com painel de gerenciamento habilitado
var rabbitmq = builder
    .AddRabbitMQ("rabbitmq")
    .WithManagementPlugin(); // Habilita o painel em http://localhost:15672

// Servidor de email local para testes (MailHog)
var mailhog = builder
    .AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
    .WithEndpoint(port: 8025, targetPort: 8025, name: "ui");


// API REST — Publisher
// Recebe as requisições e publica mensagens no RabbitMQ
var api = builder
    .AddProject<Projects.JiuJitsu_Api>("api")
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("EvolutionApi__BaseUrl",  "http://localhost:8080")
    .WithEnvironment("EvolutionApi__ApiKey",   "jiujitsu-dev-key")
    .WithEnvironment("EvolutionApi__Instance", "jiujitsu");

// Worker Service — Consumer
// Lê mensagens do RabbitMQ, salva no banco e envia email
// Não depende da API — sobe em paralelo assim que RabbitMQ e DB estiverem prontos
builder
    .AddProject<Projects.JiuJitsu_Worker>("worker")
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("EvolutionApi__BaseUrl",  "http://localhost:8080")
    .WithEnvironment("EvolutionApi__ApiKey",   "jiujitsu-dev-key")
    .WithEnvironment("EvolutionApi__Instance", "jiujitsu");

builder.Build().Run();
