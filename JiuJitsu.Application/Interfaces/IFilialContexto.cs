namespace JiuJitsu.Application.Interfaces;

/// <summary>
/// Fornece o contexto de filial do usuário autenticado, extraído do token JWT.
/// FilialId nulo indica Gestor Central com acesso a toda a rede.
/// </summary>
public interface IFilialContexto
{
    Guid? FilialId { get; }
    bool  IsAdmin  { get; }
}
