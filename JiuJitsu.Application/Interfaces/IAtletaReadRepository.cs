using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

// Interface de leitura — implementada com Dapper na Infrastructure
public interface IAtletaReadRepository
{
    Task<AtletaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ListaAtletasDto> ListarAsync(
        string? nome,
        string? faixa,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);

    Task<bool> ExistePorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
