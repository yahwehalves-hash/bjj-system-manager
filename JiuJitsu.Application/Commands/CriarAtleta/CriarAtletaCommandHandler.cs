using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.ValueObjects;
using MediatR;

namespace JiuJitsu.Application.Commands.CriarAtleta;

public class CriarAtletaCommandHandler : IRequestHandler<CriarAtletaCommand, Guid>
{
    private readonly IMessagePublisher _publisher;

    public CriarAtletaCommandHandler(IMessagePublisher publisher) => _publisher = publisher;

    public async Task<Guid> Handle(CriarAtletaCommand request, CancellationToken cancellationToken)
    {
        Validar(request);

        // Gera o ID aqui para retornar imediatamente ao cliente
        // O Worker usará este mesmo ID ao salvar no banco
        var atletaId = Guid.CreateVersion7();

        var mensagem = new AtletaMensagem
        {
            Operacao    = "Criacao",
            AtletaId    = atletaId,
            OcorridoEm  = DateTime.UtcNow,
            Payload     = new AtletaPayload
            {
                FilialId            = request.FilialId,
                NomeCompleto        = request.NomeCompleto,
                Cpf                 = request.Cpf,
                DataNascimento      = request.DataNascimento,
                Faixa               = request.Faixa.ToString(),
                Grau                = (int)request.Grau,
                DataUltimaGraduacao = request.DataUltimaGraduacao,
                Email               = request.Email
            }
        };

        await _publisher.PublicarAsync(mensagem, cancellationToken);

        return atletaId;
    }

    private static void Validar(CriarAtletaCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");

        if (request.DataNascimento >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data de nascimento deve ser no passado.");

        if (request.DataUltimaGraduacao > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data da última graduação não pode ser futura.");

        // Reutiliza os Value Objects do domínio para validar CPF e Email
        _ = new Cpf(request.Cpf);
        _ = new Email(request.Email);
    }
}
