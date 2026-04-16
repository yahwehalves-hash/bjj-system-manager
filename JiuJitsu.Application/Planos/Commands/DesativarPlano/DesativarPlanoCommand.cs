using MediatR;

namespace JiuJitsu.Application.Planos.Commands.DesativarPlano;

public record DesativarPlanoCommand(Guid Id) : IRequest;
