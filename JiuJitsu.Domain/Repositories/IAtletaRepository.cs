using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Repositories;

// Contrato de persistência — a implementação fica na Infrastructure
public interface IAtletaRepository
{
    Task AdicionarAsync(Atleta atleta, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Atleta atleta, CancellationToken cancellationToken = default);
    Task<Atleta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Atleta?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
