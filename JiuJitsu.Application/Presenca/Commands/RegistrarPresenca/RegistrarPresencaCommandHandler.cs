using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Presenca.Commands.RegistrarPresenca;

public class RegistrarPresencaCommandHandler : IRequestHandler<RegistrarPresencaCommand, Guid>
{
    private readonly IRegistroPresencaRepository _repo;

    public RegistrarPresencaCommandHandler(IRegistroPresencaRepository repo) => _repo = repo;

    public async Task<Guid> Handle(RegistrarPresencaCommand request, CancellationToken cancellationToken)
    {
        var jaRegistrado = await _repo.JaRegistradoHojeAsync(request.AtletaId, request.TurmaId, cancellationToken);
        if (jaRegistrado)
            throw new InvalidOperationException("Presença já registrada para este atleta hoje nesta turma.");

        var registro = new RegistroPresenca(
            request.AtletaId,
            request.TurmaId,
            request.FilialId,
            request.Origem,
            request.RegistradoPor);

        await _repo.AdicionarAsync(registro, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);

        return registro.Id;
    }
}
