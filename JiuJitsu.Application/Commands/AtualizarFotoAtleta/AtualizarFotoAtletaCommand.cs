using MediatR;

namespace JiuJitsu.Application.Commands.AtualizarFotoAtleta;

public record AtualizarFotoAtletaCommand(Guid AtletaId, string? FotoBase64) : IRequest;
