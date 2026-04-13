using System.Text;
using JiuJitsu.Application;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Api.Services;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Infrastructure;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Aspire — telemetria, health checks e service discovery
builder.AddServiceDefaults();

// PostgreSQL via Aspire — injeta DbContext com connection string do AppHost
builder.AddNpgsqlDbContext<AppDbContext>("jiujitsu-db");

// RabbitMQ via Aspire — injeta IConnection com configurações do AppHost
builder.AddRabbitMQClient("rabbitmq");

// Camadas da aplicação
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddEmailConfiguracoes(builder.Configuration);

// Contexto de filial — extrai FilialId do JWT por requisição
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFilialContexto, FilialContexto>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtChave = builder.Configuration["Jwt:Chave"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Emissor"],
            ValidAudience            = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtChave))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JiuJitsu CRUD API", Version = "v1" });
});

var app = builder.Build();

// Aplica migrations e seed de admin inicial
{
    using var escopo = app.Services.CreateScope();
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger   = escopo.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await contexto.Database.MigrateAsync();

    var adminEmail = app.Configuration["AdminInicial:Email"];
    var adminSenha = app.Configuration["AdminInicial:Senha"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminSenha))
    {
        var jaExisteAdmin = await contexto.Usuarios.AnyAsync(u => u.Role == "Admin");
        if (!jaExisteAdmin)
        {
            var hash  = BCrypt.Net.BCrypt.HashPassword(adminSenha);
            var admin = new Usuario("Administrador", adminEmail, hash, "Admin", deveAlterarSenha: true);
            contexto.Usuarios.Add(admin);
            await contexto.SaveChangesAsync();
            logger.LogInformation("Admin inicial criado: {Email}. Troque a senha no primeiro acesso.", adminEmail);
        }
    }
    else
    {
        logger.LogWarning("AdminInicial não configurado. Nenhum admin foi criado automaticamente.");
    }
}

app.MapDefaultEndpoints();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode = ex switch
    {
        ArgumentException   => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        _                   => StatusCodes.Status500InternalServerError
    };
    await ctx.Response.WriteAsJsonAsync(new { erro = ex?.Message });
}));

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
