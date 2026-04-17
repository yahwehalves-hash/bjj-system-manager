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

// ── Stack Evolution API ──────────────────────────────────────────────────────

var evolutionPostgres = builder
    .AddContainer("evolution-postgres", "postgres", "16-alpine")
    .WithEnvironment("POSTGRES_USER", "evolution")
    .WithEnvironment("POSTGRES_PASSWORD", "evolution123")
    .WithEnvironment("POSTGRES_DB", "evolution")
    .WithVolume("evolution-postgres-data", "/var/lib/postgresql/data")
    .WithEndpoint(port: 5433, targetPort: 5432, name: "tcp");

var evolutionRedis = builder
    .AddContainer("evolution-redis", "redis", "7-alpine")
    .WithVolume("evolution-redis-data", "/data");

var evolutionApi = builder
    .AddContainer("evolution-api", "evoapicloud/evolution-api", "latest")
    .WithEnvironment("DATABASE_PROVIDER", "postgresql")
    .WithEnvironment("DATABASE_ENABLED", "true")
    .WithEnvironment("DATABASE_CONNECTION_URI", "postgresql://evolution:evolution123@evolution-postgres:5432/evolution")
    .WithEnvironment("DATABASE_CONNECTION_CLIENT_NAME", "evolution")
    .WithEnvironment("CACHE_REDIS_ENABLED", "true")
    .WithEnvironment("CACHE_REDIS_URI", "redis://evolution-redis:6379")
    .WithEnvironment("AUTHENTICATION_TYPE", "apikey")
    .WithEnvironment("AUTHENTICATION_API_KEY", "jiujitsu-dev-key")
    .WithEnvironment("SERVER_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEnvironment("DOCKER_ENV", "true")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WaitFor(evolutionPostgres)
    .WaitFor(evolutionRedis);

// ── Frontend ─────────────────────────────────────────────────────────────────

var frontend = builder
    .AddNpmApp("frontend", "../jiujitsu-front", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints();

// ── API REST ─────────────────────────────────────────────────────────────────

var api = builder
    .AddProject<Projects.JiuJitsu_Api>("api")
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("EvolutionApi__BaseUrl", evolutionApi.GetEndpoint("http"))
    .WithEnvironment("EvolutionApi__ApiKey", "jiujitsu-dev-key")
    .WithEnvironment("EvolutionApi__Instance", "jiujitsu");

// ── Worker ───────────────────────────────────────────────────────────────────

builder
    .AddProject<Projects.JiuJitsu_Worker>("worker")
    .WithReference(rabbitmq)
    .WithReference(bancoDados)
    .WaitFor(rabbitmq)
    .WaitFor(bancoDados)
    .WithEnvironment("EvolutionApi__BaseUrl", evolutionApi.GetEndpoint("http"))
    .WithEnvironment("EvolutionApi__ApiKey", "jiujitsu-dev-key")
    .WithEnvironment("EvolutionApi__Instance", "jiujitsu");

builder.Build().Run();
