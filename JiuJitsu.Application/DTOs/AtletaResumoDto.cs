namespace JiuJitsu.Application.DTOs;

// DTO usado na listagem paginada — contém apenas os campos essenciais
public record AtletaResumoDto(
    Guid   Id,
    string NomeCompleto,
    string Cpf,
    string Faixa,
    int    Grau,
    bool   Ativo
);
