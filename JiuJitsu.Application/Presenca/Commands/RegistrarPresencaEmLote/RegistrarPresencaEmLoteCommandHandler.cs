using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Presenca.Commands.RegistrarPresencaEmLote;

public class RegistrarPresencaEmLoteCommandHandler : IRequestHandler<RegistrarPresencaEmLoteCommand, int>
{
    private readonly IRegistroPresencaRepository _repo;

    public RegistrarPresencaEmLoteCommandHandler(IRegistroPresencaRepository repo) => _repo = repo;

    public async Task<int> Handle(RegistrarPresencaEmLoteCommand request, CancellationToken cancellationToken)
    {
        var novos = new List<RegistroPresenca>();

        foreach (var atletaId in request.AtletaIds)
        {
            var jaRegistrado = await _repo.JaRegistradoHojeAsync(atletaId, request.TurmaId, cancellationToken);
            if (jaRegistrado) continue;

            novos.Add(new RegistroPresenca(
                atletaId,
                request.TurmaId,
                request.FilialId,
                OrigemPresenca.Manual,
                request.RegistradoPor));
        }

        if (novos.Count > 0)
        {
            await _repo.AdicionarEmLoteAsync(novos, cancellationToken);
            await _repo.SalvarAlteracoesAsync(cancellationToken);
        }

        return novos.Count;
    }
}
