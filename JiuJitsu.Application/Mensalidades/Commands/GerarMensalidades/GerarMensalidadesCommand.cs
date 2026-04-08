using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.GerarMensalidades;

/// <summary>
/// Gera as mensalidades de todos os atletas ativos para a competência informada.
/// Chamado pelo Worker no 1º dia de cada mês ou via endpoint manual (Admin).
/// </summary>
public record GerarMensalidadesCommand(DateOnly Competencia) : IRequest<int>;
