using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Commands.AtualizarFotoAtleta;

public class AtualizarFotoAtletaCommandHandler : IRequestHandler<AtualizarFotoAtletaCommand>
{
    private readonly IAtletaRepository _atletaRepositorio;

    public AtualizarFotoAtletaCommandHandler(IAtletaRepository atletaRepositorio)
        => _atletaRepositorio = atletaRepositorio;

    public async Task Handle(AtualizarFotoAtletaCommand request, CancellationToken cancellationToken)
    {
        var atleta = await _atletaRepositorio.ObterPorIdAsync(request.AtletaId, cancellationToken);
        if (atleta is null)
            throw new KeyNotFoundException($"Atleta com Id '{request.AtletaId}' não encontrado.");

        atleta.AtualizarFoto(request.FotoBase64);

        await _atletaRepositorio.AtualizarAsync(atleta, cancellationToken);
        await _atletaRepositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
