namespace JiuJitsu.Domain.ValueObjects;

// Value Object imutável — CPF é apenas um identificador único, sem validação de dígitos
public sealed class Cpf : IEquatable<Cpf>
{
    public string Valor { get; }

    public Cpf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CPF não pode ser vazio.", nameof(valor));

        // Remove caracteres de formatação antes de armazenar
        Valor = valor.Replace(".", "").Replace("-", "").Trim();

        if (Valor.Length != 11)
            throw new ArgumentException("CPF deve conter 11 dígitos.", nameof(valor));
    }

    public bool Equals(Cpf? outro) => outro is not null && Valor == outro.Valor;

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public override int GetHashCode() => Valor.GetHashCode();

    public override string ToString() =>
        $"{Valor[..3]}.{Valor[3..6]}.{Valor[6..9]}-{Valor[9..]}";
}
