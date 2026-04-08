namespace JiuJitsu.Application.DTOs;

// DTO usado na listagem paginada — contém apenas os campos essenciais
public record AtletaResumoDto(
    Guid   Id,
    Guid   FilialId,
    string NomeCompleto,
    string Cpf,
    string Faixa,
    int    Grau,
    bool   Ativo
);
