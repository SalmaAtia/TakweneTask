using TrackManagement.Domain.Common.Exceptions;

namespace TrackManagement.Domain.Common;

internal static class Guard
{
    public static string RequiredText(string? value, string name, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new BusinessRuleViolationException($"{name} is required.");

        if (trimmed.Length > maxLength)
            throw new BusinessRuleViolationException($"{name} must be at most {maxLength} characters.");

        return trimmed;
    }

    public static Guid RequiredId(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new BusinessRuleViolationException($"{name} is required.");

        return value;
    }
}
