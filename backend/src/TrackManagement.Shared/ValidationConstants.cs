namespace TrackManagement.Shared;

/// <summary>
/// Every hard-coded limit and pattern for the catalog's data, in one place. Used by the domain
/// (invariants), FluentValidation (request shape) and EF Core (column sizes), so they can't drift apart.
/// Changing a length here changes the database schema: add a migration.
/// </summary>
public static class ValidationConstants
{
    public static class Track
    {
        public const int TitleMaxLength = 200;
        public const int GenreMaxLength = 50;

        /// <summary>Release dates before 1 January of this year are rejected.</summary>
        public const int EarliestReleaseYear = 1900;
    }

    public static class Artist
    {
        public const int NameMaxLength = 200;
    }

    public static class Dsp
    {
        public const int NameMaxLength = 100;
    }

    /// <summary>Column size for enums stored as strings (TrackStatus, DistributionStatus).</summary>
    public static class Status
    {
        public const int MaxLength = 20;
    }

    /// <summary>International Standard Recording Code: CC-XXX-YY-NNNNN.</summary>
    public static class Isrc
    {
        /// <summary>Canonical form with hyphens, e.g. "US-RC1-76-07839".</summary>
        public const int Length = 15;

        /// <summary>Normalized value without hyphens: CC (country) + XXX (registrant) + YY (year) + NNNNN (designation).</summary>
        public const string CompactPattern = "^[A-Z]{2}[A-Z0-9]{3}[0-9]{2}[0-9]{5}$";

        /// <summary>Request input: any case, hyphens optional.</summary>
        public const string InputPattern = "^[A-Za-z]{2}-?[A-Za-z0-9]{3}-?[0-9]{2}-?[0-9]{5}$";
    }

    public static class Email
    {
        public const int MaxLength = 254;
        public const string Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    }

    /// <summary>ISO 3166-1 alpha-2 (format check only), e.g. "EG".</summary>
    public static class CountryCode
    {
        public const int Length = 2;

        /// <summary>Normalized (uppercase) value.</summary>
        public const string Pattern = "^[A-Z]{2}$";

        /// <summary>Request input: any case.</summary>
        public const string InputPattern = "^[A-Za-z]{2}$";
    }
}
