using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Repositories;

public interface IMensalidadeRepository
{
    Task AdicionarAsync(Mensalidade mensalidade, CancellationToken cancellationToken = default);
    Task AdicionarVariasAsync(IEnumerable<Mensalidade> mensalidades, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Mensalidade mensalidade, CancellationToken cancellationToken = default);
    Task<Mensalidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteParaAtletaNoMesAsync(Guid atletaId, DateOnly competencia, CancellationToken cancellationToken = default);

    // Busca mensalidades vencidas para o job diário de atualização de status
    Task<IEnumerable<Mensalidade>> ListarPendentesVencidasAsync(
        DateOnly dataReferencia,
        int toleranciaDias,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Mensalidade>> ListarVencidasParaInadimplenciaAsync(
        DateOnly dataReferencia,
        int diasParaInadimplencia,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
