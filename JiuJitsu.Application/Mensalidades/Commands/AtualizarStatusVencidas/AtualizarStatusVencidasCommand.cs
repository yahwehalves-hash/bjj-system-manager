using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.AtualizarStatusVencidas;

/// <summary>
/// Atualiza o status de mensalidades vencidas. Executado pelo Worker diariamente.
/// Retorna o número de mensalidades atualizadas.
/// </summary>
public record AtualizarStatusVencidasCommand : IRequest<int>;
