using JiuJitsu.Application.Interfaces;
using System.Security.Claims;

namespace JiuJitsu.Api.Services;

/// <summary>
/// Implementação do IFilialContexto que extrai o FilialId e o Role do token JWT do usuário autenticado.
/// Registrado como Scoped para ter acesso ao HttpContext por requisição.
/// </summary>
public class FilialContexto : IFilialContexto
{
    public Guid? FilialId { get; }
    public bool  IsAdmin  { get; }

    public FilialContexto(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        IsAdmin = user?.IsInRole("Admin") ?? false;

        var filialClaim = user?.FindFirstValue("filialId");
        FilialId = filialClaim is not null && Guid.TryParse(filialClaim, out var id) ? id : null;
    }
}
