using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Pagamento.Commands.CriarCobrancaOnline;

/// <summary>
/// Cria uma cobrança no gateway de pagamento para uma mensalidade e vincula os dados de pagamento.
/// </summary>
public record CriarCobrancaOnlineCommand(Guid MensalidadeId) : IRequest<CobrancaOnlineDto>;
