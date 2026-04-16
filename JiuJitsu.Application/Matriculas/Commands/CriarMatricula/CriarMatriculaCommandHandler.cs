using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Matriculas.Commands.CriarMatricula;

public class CriarMatriculaCommandHandler : IRequestHandler<CriarMatriculaCommand, Guid>
{
    private readonly IMatriculaRepository _matriculaRepo;
    private readonly IPlanoRepository     _planoRepo;

    public CriarMatriculaCommandHandler(
        IMatriculaRepository matriculaRepo,
        IPlanoRepository planoRepo)
    {
        _matriculaRepo = matriculaRepo;
        _planoRepo     = planoRepo;
    }

    public async Task<Guid> Handle(CriarMatriculaCommand request, CancellationToken cancellationToken)
    {
        // Verifica se plano existe
        var plano = await _planoRepo.ObterPorIdAsync(request.PlanoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plano {request.PlanoId} não encontrado.");

        // Cancela matrícula ativa anterior automaticamente
        var matriculaAtiva = await _matriculaRepo.ObterAtivaDoAtletaAsync(request.AtletaId, cancellationToken);
        if (matriculaAtiva is not null)
            matriculaAtiva.Cancelar("Substituída por nova matrícula.");

        var matricula = new Matricula(
            request.AtletaId,
            request.PlanoId,
            request.DataInicio,
            request.DataFim,
            request.ValorCustomizado,
            request.Observacao);

        await _matriculaRepo.AdicionarAsync(matricula, cancellationToken);
        await _matriculaRepo.SalvarAlteracoesAsync(cancellationToken);

        return matricula.Id;
    }
}
