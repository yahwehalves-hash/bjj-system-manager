namespace JiuJitsu.Application.Interfaces;

public interface IContratoService
{
    Task<byte[]> GerarPdfAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task<Guid> RegistrarAceiteAsync(Guid atletaId, string? ipAceite, CancellationToken cancellationToken = default);
    Task<bool> PossuiContratoAsync(Guid atletaId, CancellationToken cancellationToken = default);
}
