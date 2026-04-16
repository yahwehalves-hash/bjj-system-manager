using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Commands.SalvarRegraGraduacao;

public record SalvarRegraGraduacaoCommand(
    Guid?  FilialId,
    Faixa  Faixa,
    Grau   Grau,
    int    TempoMinimoMeses) : IRequest;
