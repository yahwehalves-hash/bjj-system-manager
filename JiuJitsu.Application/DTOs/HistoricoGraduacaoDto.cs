namespace JiuJitsu.Application.DTOs;

public record HistoricoGraduacaoDto(
    Guid      Id,
    string    Faixa,
    int       Grau,
    DateOnly  DataInicio,
    DateOnly? DataFim,
    int       DiasNaGraduacao
);

public record HistoricoAtletaDto(
    Guid     AtletaId,
    string   NomeCompleto,
    string?  FotoBase64,
    string   FaixaAtual,
    int      GrauAtual,
    int      TotalDiasNaArte,
    IReadOnlyList<HistoricoGraduacaoDto> Historico
);
