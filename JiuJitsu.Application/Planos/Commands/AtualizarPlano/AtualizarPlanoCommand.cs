using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Planos.Commands.AtualizarPlano;

public record AtualizarPlanoCommand(
    Guid              Id,
    string            Nome,
    string?           Descricao,
    decimal           Valor,
    TipoPeriodicidade Periodicidade) : IRequest;
