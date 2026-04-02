using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using MediatR;

namespace JiuJitsu.Application.Commands.ExcluirAtleta;

public class ExcluirAtletaCommandHandler : IRequestHandler<ExcluirAtletaCommand>
{
    private readonly IMessagePublisher    _publisher;
    private readonly IAtletaReadRepository _readRepository;

    public ExcluirAtletaCommandHandler(
        IMessagePublisher publisher,
        IAtletaReadRepository readRepository)
    {
        _publisher      = publisher;
        _readRepository = readRepository;
    }

    public async Task Handle(ExcluirAtletaCommand request, CancellationToken cancellationToken)
    {
        var existe = await _readRepository.ExistePorIdAsync(request.Id, cancellationToken);
        if (!existe)
            throw new KeyNotFoundException($"Atleta com Id '{request.Id}' não encontrado.");

        var mensagem = new AtletaMensagem
        {
            Operacao   = "Exclusao",
            AtletaId   = request.Id,
            OcorridoEm = DateTime.UtcNow,
            Payload    = null // Payload não necessário para exclusão
        };

        await _publisher.PublicarAsync(mensagem, cancellationToken);
    }
}
