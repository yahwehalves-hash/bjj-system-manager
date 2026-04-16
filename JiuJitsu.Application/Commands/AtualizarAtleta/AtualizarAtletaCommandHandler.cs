using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.ValueObjects;
using MediatR;

namespace JiuJitsu.Application.Commands.AtualizarAtleta;

public class AtualizarAtletaCommandHandler : IRequestHandler<AtualizarAtletaCommand>
{
    private readonly IMessagePublisher    _publisher;
    private readonly IAtletaReadRepository _readRepository;

    public AtualizarAtletaCommandHandler(
        IMessagePublisher publisher,
        IAtletaReadRepository readRepository)
    {
        _publisher      = publisher;
        _readRepository = readRepository;
    }

    public async Task Handle(AtualizarAtletaCommand request, CancellationToken cancellationToken)
    {
        Validar(request);

        // Valida existência antes de publicar
        var existe = await _readRepository.ExistePorIdAsync(request.Id, cancellationToken);
        if (!existe)
            throw new KeyNotFoundException($"Atleta com Id '{request.Id}' não encontrado.");

        var mensagem = new AtletaMensagem
        {
            Operacao    = "Atualizacao",
            AtletaId    = request.Id,
            OcorridoEm  = DateTime.UtcNow,
            Payload     = new AtletaPayload
            {
                NomeCompleto        = request.NomeCompleto,
                DataNascimento      = request.DataNascimento,
                Faixa               = request.Faixa.ToString(),
                Grau                = (int)request.Grau,
                DataUltimaGraduacao = request.DataUltimaGraduacao,
                Email               = request.Email,
                Telefone            = request.Telefone
            }
        };

        await _publisher.PublicarAsync(mensagem, cancellationToken);
    }

    private static void Validar(AtualizarAtletaCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");

        if (request.DataNascimento >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data de nascimento deve ser no passado.");

        if (request.DataUltimaGraduacao > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data da última graduação não pode ser futura.");

        _ = new Email(request.Email);
    }
}
