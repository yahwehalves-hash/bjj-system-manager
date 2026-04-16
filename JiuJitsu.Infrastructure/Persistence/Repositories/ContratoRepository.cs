using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class ContratoRepository : IContratoRepository
{
    private readonly AppDbContext _db;

    public ContratoRepository(AppDbContext db) => _db = db;

    public Task<TemplateContrato?> ObterTemplateAtivoAsync(Guid? filialId, CancellationToken cancellationToken = default)
        => _db.TemplatesContrato
            .Where(t => t.Ativo && t.FilialId == filialId)
            .OrderByDescending(t => t.Versao)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AdicionarTemplateAsync(TemplateContrato template, CancellationToken cancellationToken = default)
        => await _db.TemplatesContrato.AddAsync(template, cancellationToken);

    public Task<Contrato?> ObterContratoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default)
        => _db.Contratos
            .OrderByDescending(c => c.DataAceite)
            .FirstOrDefaultAsync(c => c.AtletaId == atletaId, cancellationToken);

    public async Task AdicionarContratoAsync(Contrato contrato, CancellationToken cancellationToken = default)
        => await _db.Contratos.AddAsync(contrato, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
