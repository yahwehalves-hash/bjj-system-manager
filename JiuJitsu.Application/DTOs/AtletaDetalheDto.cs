namespace JiuJitsu.Application.DTOs;

// DTO usado na consulta por ID — contém todos os campos do atleta
public record AtletaDetalheDto(
    Guid      Id,
    Guid      FilialId,
    string    NomeCompleto,
    string    Cpf,
    DateOnly  DataNascimento,
    string    Faixa,
    int       Grau,
    DateOnly  DataUltimaGraduacao,
    string    Email,
    bool      Ativo,
    DateTime  CriadoEm,
    DateTime? AtualizadoEm
);
