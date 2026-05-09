namespace LaboratorioTlahuac.Domain.Security;

public static class SecurityTextNormalizer
{
    public static string NormalizeEmail(string email)
    {
        return Normalize(email);
    }

    public static string NormalizeName(string name)
    {
        return Normalize(name);
    }

    private static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant();
    }
}
