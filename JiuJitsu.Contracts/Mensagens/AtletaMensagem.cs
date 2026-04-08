namespace JiuJitsu.Contracts.Mensagens;

// Contrato de mensagem trafegado entre a API (Publisher) e o Worker (Consumer) via RabbitMQ
public class AtletaMensagem
{
    // Valores possíveis: "Criacao", "Atualizacao", "Exclusao"
    public string Operacao { get; set; } = string.Empty;
    public Guid AtletaId { get; set; }
    // Nulo quando a operação é Exclusao
    public AtletaPayload? Payload { get; set; }
    public DateTime OcorridoEm { get; set; }
}

public class AtletaPayload
{
    public Guid FilialId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    // Nome da faixa em texto (ex: "Azul")
    public string Faixa { get; set; } = string.Empty;
    // Valor inteiro do enum Grau (0 = SemGrau, 1 = Primeiro, etc.)
    public int Grau { get; set; }
    public DateOnly DataUltimaGraduacao { get; set; }
    public string Email { get; set; } = string.Empty;
}
