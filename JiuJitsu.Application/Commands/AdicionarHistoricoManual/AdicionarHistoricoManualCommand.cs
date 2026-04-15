using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Commands.AdicionarHistoricoManual;

public record AdicionarHistoricoManualCommand(
    Guid     AtletaId,
    Faixa    Faixa,
    Grau     Grau,
    DateOnly DataInicio,
    DateOnly? DataFim
) : IRequest;
