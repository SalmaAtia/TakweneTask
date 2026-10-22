namespace TrackManagement.Shared;

/// <summary>Hard-coded limits for login requests and JWT settings/validation.</summary>
public static class AuthValidationConstants
{
    public const int UsernameMaxLength = 256;
    public const int PasswordMaxLength = 256;

    /// <summary>HMAC-SHA256 needs a signing key of at least 256 bits.</summary>
    public const int MinSigningKeyBytes = 32;

    /// <summary>Tolerated clock difference when checking a token's lifetime.</summary>
    public const int TokenClockSkewSeconds = 30;
}
