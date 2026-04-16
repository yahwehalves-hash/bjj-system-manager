namespace JiuJitsu.Application.DTOs;

public record TurmaResumoDto(
    Guid    Id,
    Guid    FilialId,
    string  NomeFilial,
    string  Nome,
    Guid?   ProfessorId,
    string? NomeProfessor,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima,
    int     TotalAlunos,
    bool    Ativo);

public record TurmaDetalheDto(
    Guid    Id,
    Guid    FilialId,
    string  NomeFilial,
    string  Nome,
    Guid?   ProfessorId,
    string? NomeProfessor,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima,
    int     TotalAlunos,
    bool    Ativo,
    DateTime CriadoEm,
    IEnumerable<TurmaAtletaDto> Atletas);

public record TurmaAtletaDto(
    Guid     AtletaId,
    string   NomeAtleta,
    string   Faixa,
    int      Grau,
    DateTime VinculadoEm);

public record ListaTurmasDto(
    IEnumerable<TurmaResumoDto> Itens,
    int TotalItens);
