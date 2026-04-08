using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IMensalidadeReadRepository
{
    Task<MensalidadeDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ListaMensalidadesDto> ListarAsync(
        Guid?   filialId,
        Guid?   atletaId,
        string? status,
        string? competencia,
        int     pagina,
        int     tamanhoPagina,
        CancellationToken cancellationToken = default);
}
