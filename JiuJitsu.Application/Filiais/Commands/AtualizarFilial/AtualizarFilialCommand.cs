using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.AtualizarFilial;

public record AtualizarFilialCommand(
    Guid    Id,
    string  Nome,
    string? Endereco,
    string? Cnpj,
    string? Telefone) : IRequest;
