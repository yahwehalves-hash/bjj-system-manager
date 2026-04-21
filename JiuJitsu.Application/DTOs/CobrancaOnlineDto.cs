namespace JiuJitsu.Application.DTOs;

public record CobrancaOnlineDto(
    Guid    MensalidadeId,
    string  CobrancaExternaId,
    string? LinkPagamento,
    string? PixCopiaCola);
