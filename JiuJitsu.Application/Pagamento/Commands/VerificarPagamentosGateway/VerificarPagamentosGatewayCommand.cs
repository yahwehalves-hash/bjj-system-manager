using MediatR;

namespace JiuJitsu.Application.Pagamento.Commands.VerificarPagamentosGateway;

/// <summary>
/// Consulta o gateway de pagamento para sincronizar status de cobranças pendentes.
/// Equivale ao polling automático do Worker, mas disparado manualmente pelo Admin.
/// Retorna a quantidade de pagamentos confirmados nesta execução.
/// </summary>
public record VerificarPagamentosGatewayCommand : IRequest<int>;
