using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.CriarFilial;

public record CriarFilialCommand(
    string  Nome,
    string? Endereco,
    string? Cnpj,
    string? Telefone) : IRequest<Guid>;
