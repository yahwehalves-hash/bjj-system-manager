using System.Text;
using JiuJitsu.Application;
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

// Aplica migrations automaticamente no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var escopo = app.Services.CreateScope();
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await contexto.Database.MigrateAsync();
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
