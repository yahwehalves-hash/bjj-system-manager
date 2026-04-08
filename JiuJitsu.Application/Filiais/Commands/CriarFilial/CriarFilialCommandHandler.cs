using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.CriarFilial;

public class CriarFilialCommandHandler : IRequestHandler<CriarFilialCommand, Guid>
{
    private readonly IFilialRepository       _filialRepo;
    private readonly IConfiguracaoRepository _configuracaoRepo;

    public CriarFilialCommandHandler(
        IFilialRepository filialRepo,
        IConfiguracaoRepository configuracaoRepo)
    {
        _filialRepo       = filialRepo;
        _configuracaoRepo = configuracaoRepo;
    }

    public async Task<Guid> Handle(CriarFilialCommand request, CancellationToken cancellationToken)
    {
        var filial = new Filial(request.Nome, request.Endereco, request.Cnpj, request.Telefone);

        await _filialRepo.AdicionarAsync(filial, cancellationToken);

        // Cria o registro de configuração da filial (com todos os campos nulos = herda global)
        var configuracao = new ConfiguracaoFilial(filial.Id);
        await _configuracaoRepo.AdicionarFilialAsync(configuracao, cancellationToken);

        await _filialRepo.SalvarAlteracoesAsync(cancellationToken);

        return filial.Id;
    }
}
