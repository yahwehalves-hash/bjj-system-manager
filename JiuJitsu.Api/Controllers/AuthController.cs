using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    /// <summary>Registra um novo usuário</summary>
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request, CancellationToken ct)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct))
            return BadRequest(new { erro = "Email já cadastrado." });

        var hash    = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        var usuario = new Usuario(request.Nome, request.Email, hash, request.Role ?? "Aluno");

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);

        return Created(string.Empty, new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
    }

    /// <summary>Autentica e retorna um JWT</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Unauthorized(new { erro = "Email ou senha inválidos." });

        var token = GerarToken(usuario);

        return Ok(new { token, usuario.Nome, usuario.Email, usuario.Role });
    }

    private string GerarToken(Usuario usuario)
    {
        var chave    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Chave"]!));
        var credencial = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name,               usuario.Nome),
            new Claim(ClaimTypes.Role,               usuario.Role),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Emissor"],
            audience:           _config["Jwt:Audiencia"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: credencial);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Senha);
public record RegistrarRequest(string Nome, string Email, string Senha, string? Role);
