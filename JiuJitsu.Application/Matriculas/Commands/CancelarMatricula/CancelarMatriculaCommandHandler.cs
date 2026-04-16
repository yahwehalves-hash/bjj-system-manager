using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Matriculas.Commands.CancelarMatricula;

public class CancelarMatriculaCommandHandler : IRequestHandler<CancelarMatriculaCommand>
{
    private readonly IMatriculaRepository _matriculaRepo;

    public CancelarMatriculaCommandHandler(IMatriculaRepository matriculaRepo)
        => _matriculaRepo = matriculaRepo;

    public async Task Handle(CancelarMatriculaCommand request, CancellationToken cancellationToken)
    {
        var matricula = await _matriculaRepo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Matrícula {request.Id} não encontrada.");

        matricula.Cancelar(request.Motivo);
        await _matriculaRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
