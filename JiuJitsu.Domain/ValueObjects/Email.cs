using System.Text.RegularExpressions;

namespace JiuJitsu.Domain.ValueObjects;

// Value Object imutável — garante que o email tenha formato válido
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex FormatoValido =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email não pode ser vazio.", nameof(valor));

        var normalizado = valor.Trim().ToLowerInvariant();

        if (!FormatoValido.IsMatch(normalizado))
            throw new ArgumentException("Formato de email inválido.", nameof(valor));

        Valor = normalizado;
    }

    public bool Equals(Email? outro) => outro is not null && Valor == outro.Valor;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Valor.GetHashCode();

    public override string ToString() => Valor;
}
