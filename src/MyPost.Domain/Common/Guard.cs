namespace MyPost.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string name, int maximumLength = 500)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{name} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new DomainException($"{name} must not exceed {maximumLength} characters.");
        }

        return normalized;
    }
}
