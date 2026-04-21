namespace JiuJitsu.Application.DTOs;

// DTO usado na consulta por ID — contém todos os campos do atleta
// IMPORTANTE: deve ser record não-posicional com { get; set; } para que o Dapper
// use constructor vazio + atribuição por nome, permitindo que o DateOnlyTypeHandler
// converta DateOnly corretamente. Records posicionais fazem Dapper inspecionar
// GetFieldType() que retorna DateTime para colunas "date" do PostgreSQL,
// causando falha na busca pelo constructor antes do TypeHandler ser invocado.
public record AtletaDetalheDto
{
    public Guid      Id                    { get; set; }
    public Guid      FilialId              { get; set; }
    public string?   NomeFilial            { get; set; }
    public string    NomeCompleto          { get; set; } = "";
    public string    Cpf                   { get; set; } = "";
    public DateOnly  DataNascimento        { get; set; }
    public string    Faixa                 { get; set; } = "";
    public int       Grau                  { get; set; }
    public string    TipoAtleta            { get; set; } = "";
    public DateOnly  DataUltimaGraduacao   { get; set; }
    public string    Email                 { get; set; } = "";
    public string?   Telefone              { get; set; }
    public string?   FotoBase64            { get; set; }
    public bool      Ativo                 { get; set; }
    public DateTime  CriadoEm             { get; set; }
    public DateTime? AtualizadoEm         { get; set; }
}
