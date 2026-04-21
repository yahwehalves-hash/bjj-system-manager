namespace JiuJitsu.Application.Interfaces;

public record CobrancaResultado(
    string  CobrancaId,
    string? LinkPagamento,
    string? PixCopiaCola);

public interface IGatewayPagamento
{
    /// <summary>Cria ou recupera um cliente no gateway pelo CPF.</summary>
    Task<string> ObterOuCriarClienteAsync(
        string cpf, string nome, string email, string? telefone,
        CancellationToken cancellationToken = default);

    /// <summary>Cria uma cobrança no gateway e retorna o link de pagamento e PIX.</summary>
    Task<CobrancaResultado> CriarCobrancaAsync(
        string   clienteId,
        decimal  valor,
        DateOnly dataVencimento,
        string   descricao,
        string   referenciaExterna,
        CancellationToken cancellationToken = default);

    /// <summary>Consulta o status atual de uma cobrança. Retorna null se não encontrada.</summary>
    Task<string?> ConsultarStatusCobrancaAsync(
        string cobrancaId, CancellationToken cancellationToken = default);

    /// <summary>Indica se o gateway está configurado.</summary>
    bool Configurado { get; }
}
