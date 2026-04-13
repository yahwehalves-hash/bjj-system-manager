using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    /// <summary>Lista todos os usuários do sistema</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarios = await _db.Usuarios
            .OrderBy(u => u.Nome)
            .Select(u => new { u.Id, u.Nome, u.Email, u.Role, u.DeveAlterarSenha, u.CriadoEm })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    /// <summary>Altera o perfil (role) de um usuário</summary>
    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarRole(Guid id, [FromBody] AlterarRoleRequest request, CancellationToken ct)
    {
        var rolesValidas = new[] { "Admin", "GestorFilial", "Professor", "Aluno" };
        if (!rolesValidas.Contains(request.Role))
            return BadRequest(new { erro = $"Role inválida. Valores aceitos: {string.Join(", ", rolesValidas)}" });

        var usuario = await _db.Usuarios.FindAsync([id], ct);
        if (usuario is null) return NotFound();

        usuario.AlterarRole(request.Role);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}

public record AlterarRoleRequest(string Role);
