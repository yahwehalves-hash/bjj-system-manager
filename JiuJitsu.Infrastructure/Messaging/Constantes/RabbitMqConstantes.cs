namespace JiuJitsu.Infrastructure.Messaging.Constantes;

// Centraliza os nomes de exchanges, filas e routing keys para evitar strings espalhadas
public static class RabbitMqConstantes
{
    public const string Exchange       = "atletas.exchange";
    public const string Fila           = "atletas.queue";
    public const string ExchangeDlx    = "atletas.dlx";
    public const string FilaDlq        = "atletas.dlq";

    public const string RoutingCriacao     = "atleta.criado";
    public const string RoutingAtualizacao = "atleta.atualizado";
    public const string RoutingExclusao    = "atleta.excluido";
}
