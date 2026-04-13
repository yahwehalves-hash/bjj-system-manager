using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>Registra um novo usuário — role sempre definida como Aluno</summary>
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request, CancellationToken ct)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct))
            return BadRequest(new { erro = "Email já cadastrado." });

        var hash    = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        var usuario = new Usuario(request.Nome, request.Email, hash, "Aluno");

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

        return Ok(new { token, usuario.Nome, usuario.Email, usuario.Role, usuario.DeveAlterarSenha });
    }

    /// <summary>Troca a senha do usuário autenticado</summary>
    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request, CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(idClaim, out var userId))
            return Unauthorized();

        var usuario = await _db.Usuarios.FindAsync([userId], ct);
        if (usuario is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, usuario.SenhaHash))
            return BadRequest(new { erro = "Senha atual incorreta." });

        if (request.NovaSenha.Length < 6)
            return BadRequest(new { erro = "A nova senha deve ter no mínimo 6 caracteres." });

        usuario.AlterarSenha(BCrypt.Net.BCrypt.HashPassword(request.NovaSenha));
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private string GerarToken(Usuario usuario)
    {
        var chave      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Chave"]!));
        var credencial = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name,               usuario.Nome),
            new Claim(ClaimTypes.Role,               usuario.Role),
        };

        // Inclui o FilialId no token para isolamento de dados por filial
        if (usuario.FilialId.HasValue)
            claims.Add(new Claim("filialId", usuario.FilialId.Value.ToString()));

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
public record RegistrarRequest(string Nome, string Email, string Senha);
public record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);
