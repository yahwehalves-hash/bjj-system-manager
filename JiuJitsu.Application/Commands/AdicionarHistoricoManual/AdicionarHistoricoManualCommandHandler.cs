using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Commands.AdicionarHistoricoManual;

public class AdicionarHistoricoManualCommandHandler : IRequestHandler<AdicionarHistoricoManualCommand>
{
    private readonly IHistoricoGraduacaoRepository _historicoRepositorio;
    private readonly IAtletaReadRepository         _atletaReadRepository;

    public AdicionarHistoricoManualCommandHandler(
        IHistoricoGraduacaoRepository historicoRepositorio,
        IAtletaReadRepository         atletaReadRepository)
    {
        _historicoRepositorio = historicoRepositorio;
        _atletaReadRepository = atletaReadRepository;
    }

    public async Task Handle(AdicionarHistoricoManualCommand request, CancellationToken cancellationToken)
    {
        var existe = await _atletaReadRepository.ExistePorIdAsync(request.AtletaId, cancellationToken);
        if (!existe)
            throw new KeyNotFoundException($"Atleta com Id '{request.AtletaId}' não encontrado.");

        if (request.DataFim.HasValue && request.DataFim <= request.DataInicio)
            throw new ArgumentException("Data de fim deve ser posterior à data de início.");

        var historico = new HistoricoGraduacao(
            atletaId:   request.AtletaId,
            faixa:      request.Faixa,
            grau:       request.Grau,
            dataInicio: request.DataInicio);

        if (request.DataFim.HasValue)
            historico.Fechar(request.DataFim.Value);

        await _historicoRepositorio.AdicionarAsync(historico, cancellationToken);
        await _historicoRepositorio.SalvarAlteracoesAsync(cancellationToken);
    }
}
