namespace JiuJitsu.Infrastructure.Messaging.Constantes;

// Centraliza os nomes de exchanges, filas e routing keys para evitar strings espalhadas
public static class RabbitMqConstantes
{
    // ── Atletas ──────────────────────────────────────────────────────────────
    public const string Exchange           = "atletas.exchange";
    public const string Fila               = "atletas.queue";
    public const string ExchangeDlx        = "atletas.dlx";
    public const string FilaDlq            = "atletas.dlq";
    public const string RoutingCriacao     = "atleta.criado";
    public const string RoutingAtualizacao = "atleta.atualizado";
    public const string RoutingExclusao    = "atleta.excluido";

    // ── Financeiro ───────────────────────────────────────────────────────────
    public const string FinanceiroExchange             = "financeiro.exchange";
    public const string FinanceiroFila                 = "financeiro.queue";
    public const string FinanceiroExchangeDlx          = "financeiro.dlx";
    public const string FinanceiroFilaDlq              = "financeiro.dlq";
    public const string RoutingMensalidadeGerada       = "mensalidade.gerada";
    public const string RoutingMensalidadePaga         = "mensalidade.paga";
    public const string RoutingMensalidadeVencida      = "mensalidade.vencida";
    public const string RoutingMensalidadeInadimplente = "mensalidade.inadimplente";

    // ── Notificações ─────────────────────────────────────────────────────────
    public const string NotificacaoExchange         = "notificacoes.exchange";
    public const string NotificacaoFila             = "notificacoes.queue";
    public const string NotificacaoExchangeDlx      = "notificacoes.dlx";
    public const string NotificacaoFilaDlq          = "notificacoes.dlq";
    public const string RoutingNotificacaoCobranca  = "notificacao.cobranca";
    public const string RoutingNotificacaoAniversario = "notificacao.aniversario";
}
