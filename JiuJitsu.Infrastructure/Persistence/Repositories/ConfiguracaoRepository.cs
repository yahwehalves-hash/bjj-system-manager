using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class ConfiguracaoRepository : IConfiguracaoRepository
{
    private readonly AppDbContext _contexto;

    public ConfiguracaoRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task<ConfiguracaoGlobal?> ObterGlobalAsync(CancellationToken cancellationToken = default)
        => await _contexto.ConfiguracaoGlobal.FirstOrDefaultAsync(cancellationToken);

    public async Task AdicionarGlobalAsync(ConfiguracaoGlobal configuracao, CancellationToken cancellationToken = default)
        => await _contexto.ConfiguracaoGlobal.AddAsync(configuracao, cancellationToken);

    public Task AtualizarGlobalAsync(ConfiguracaoGlobal configuracao, CancellationToken cancellationToken = default)
    {
        _contexto.ConfiguracaoGlobal.Update(configuracao);
        return Task.CompletedTask;
    }

    public async Task<ConfiguracaoFilial?> ObterPorFilialAsync(Guid filialId, CancellationToken cancellationToken = default)
        => await _contexto.ConfiguracaoFilial
            .FirstOrDefaultAsync(c => c.FilialId == filialId, cancellationToken);

    public async Task AdicionarFilialAsync(ConfiguracaoFilial configuracao, CancellationToken cancellationToken = default)
        => await _contexto.ConfiguracaoFilial.AddAsync(configuracao, cancellationToken);

    public Task AtualizarFilialAsync(ConfiguracaoFilial configuracao, CancellationToken cancellationToken = default)
    {
        _contexto.ConfiguracaoFilial.Update(configuracao);
        return Task.CompletedTask;
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
