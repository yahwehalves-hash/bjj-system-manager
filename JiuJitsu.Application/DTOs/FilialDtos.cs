namespace JiuJitsu.Application.DTOs;

public record FilialResumoDto(
    Guid    Id,
    string  Nome,
    string? Cnpj,
    string? Telefone,
    bool    Ativo);

public record FilialDetalheDto(
    Guid     Id,
    string   Nome,
    string?  Endereco,
    string?  Cnpj,
    string?  Telefone,
    bool     Ativo,
    DateTime CriadoEm);
