using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Planos.Commands.CriarPlano;

public record CriarPlanoCommand(
    Guid?            FilialId,
    string           Nome,
    string?          Descricao,
    decimal          Valor,
    TipoPeriodicidade Periodicidade) : IRequest<Guid>;
