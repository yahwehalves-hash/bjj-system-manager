using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.DesativarFilial;

public record DesativarFilialCommand(Guid Id) : IRequest;
