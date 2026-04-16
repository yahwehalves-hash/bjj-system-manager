using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface ITurmaRepository
{
    Task AdicionarAsync(Turma turma, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Turma turma, CancellationToken cancellationToken = default);
    Task<Turma?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAtletaAsync(AtletaTurma atletaTurma, CancellationToken cancellationToken = default);
    Task RemoverAtletaAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default);
    Task<bool> AtletaJaVinculadoAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
