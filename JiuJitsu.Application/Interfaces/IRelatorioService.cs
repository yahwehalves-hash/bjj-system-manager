namespace JiuJitsu.Application.Interfaces;

public interface IRelatorioService
{
    Task<byte[]> GerarInadimplenciaXlsxAsync(Guid? filialId, string competencia, CancellationToken ct = default);
    Task<byte[]> GerarDreXlsxAsync(Guid? filialId, string competencia, CancellationToken ct = default);
    Task<byte[]> GerarAtletasPorFaixaXlsxAsync(Guid? filialId, CancellationToken ct = default);
}
