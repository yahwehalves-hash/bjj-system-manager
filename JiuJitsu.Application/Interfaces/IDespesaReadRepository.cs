using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IDespesaReadRepository
{
    Task<DespesaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ListaDespesasDto> ListarAsync(
        Guid?    filialId,
        string?  categoria,
        string?  status,
        DateOnly? dataInicio,
        DateOnly? dataFim,
        int      pagina,
        int      tamanhoPagina,
        CancellationToken cancellationToken = default);
}
