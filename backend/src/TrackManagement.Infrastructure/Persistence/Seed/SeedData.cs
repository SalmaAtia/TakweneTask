using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;
using TrackManagement.Shared.Enums;

namespace TrackManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Deterministic demo data (fixed IDs and UTC dates) applied via HasData.
/// Anonymous objects are used because entities have private setters; value-object
/// properties receive value objects so the EF converters apply.
/// The data mirrors the Track aggregate's invariants:
/// Draft = no distributions, Submitted = Pending only, Distributed = at least one Live,
/// and never the same DSP twice for one track.
/// </summary>
internal static class SeedData
{
    internal static class DspIds
    {
        public static readonly Guid Spotify = new("d0000000-0000-0000-0000-000000000001");
        public static readonly Guid AppleMusic = new("d0000000-0000-0000-0000-000000000002");
        public static readonly Guid YouTubeMusic = new("d0000000-0000-0000-0000-000000000003");
    }

    internal static class ArtistIds
    {
        public static readonly Guid NourElSayed = new("a0000000-0000-0000-0000-000000000001");
        public static readonly Guid FaisalAlHarbi = new("a0000000-0000-0000-0000-000000000002");
        public static readonly Guid LinaHaddad = new("a0000000-0000-0000-0000-000000000003");
        public static readonly Guid YassineBenali = new("a0000000-0000-0000-0000-000000000004");
    }

    internal static class TrackIds
    {
        public static readonly Guid ShobraNights = new("b0000000-0000-0000-0000-000000000001");
        public static readonly Guid TokTokAnthem = new("b0000000-0000-0000-0000-000000000002");
        public static readonly Guid CairoConcrete = new("b0000000-0000-0000-0000-000000000003");
        public static readonly Guid DesertRain = new("b0000000-0000-0000-0000-000000000004");
        public static readonly Guid NajdMoon = new("b0000000-0000-0000-0000-000000000005");
        public static readonly Guid RiyadhLights = new("b0000000-0000-0000-0000-000000000006");
        public static readonly Guid BeirutBalcony = new("b0000000-0000-0000-0000-000000000007");
        public static readonly Guid CedarHeart = new("b0000000-0000-0000-0000-000000000008");
        public static readonly Guid OujdaSunset = new("b0000000-0000-0000-0000-000000000009");
        public static readonly Guid CasaFlow = new("b0000000-0000-0000-0000-000000000010");
    }

    public static readonly object[] Dsps =
    [
        new { Id = DspIds.Spotify, Name = "Spotify" },
        new { Id = DspIds.AppleMusic, Name = "Apple Music" },
        new { Id = DspIds.YouTubeMusic, Name = "YouTube Music" },
    ];

    public static readonly object[] Artists =
    [
        Artist(ArtistIds.NourElSayed, "Nour El Sayed", "nour.elsayed@example.com", "EG"),
        Artist(ArtistIds.FaisalAlHarbi, "Faisal Al-Harbi", "faisal.alharbi@example.com", "SA"),
        Artist(ArtistIds.LinaHaddad, "Lina Haddad", "lina.haddad@example.com", "LB"),
        Artist(ArtistIds.YassineBenali, "Yassine Benali", "yassine.benali@example.com", "MA"),
    ];

    public static readonly object[] Tracks =
    [
        // Nour El Sayed (EG)
        Track(TrackIds.ShobraNights, "Shobra Nights", ArtistIds.NourElSayed, "EG-NES-25-00001", new(2025, 3, 14), "Mahraganat", TrackStatus.Distributed),
        Track(TrackIds.TokTokAnthem, "Tok Tok Anthem", ArtistIds.NourElSayed, "EG-NES-26-00002", new(2026, 11, 27), "Mahraganat", TrackStatus.Submitted),
        Track(TrackIds.CairoConcrete, "Cairo Concrete", ArtistIds.NourElSayed, "EG-NES-26-00003", new(2027, 1, 15), "Hip-Hop", TrackStatus.Draft),

        // Faisal Al-Harbi (SA)
        Track(TrackIds.DesertRain, "Desert Rain", ArtistIds.FaisalAlHarbi, "SA-FAH-25-00001", new(2025, 6, 20), "Khaleeji", TrackStatus.Distributed),
        Track(TrackIds.NajdMoon, "Najd Moon", ArtistIds.FaisalAlHarbi, "SA-FAH-26-00002", new(2026, 9, 12), "Khaleeji", TrackStatus.Distributed),
        Track(TrackIds.RiyadhLights, "Riyadh Lights", ArtistIds.FaisalAlHarbi, "SA-FAH-26-00003", new(2027, 2, 1), "Arabic Pop", TrackStatus.Draft),

        // Lina Haddad (LB)
        Track(TrackIds.BeirutBalcony, "Beirut Balcony", ArtistIds.LinaHaddad, "LB-LHD-25-00001", new(2025, 9, 5), "Arabic Pop", TrackStatus.Distributed),
        Track(TrackIds.CedarHeart, "Cedar Heart", ArtistIds.LinaHaddad, "LB-LHD-26-00002", new(2026, 11, 20), "Arabic Pop", TrackStatus.Submitted),

        // Yassine Benali (MA)
        Track(TrackIds.OujdaSunset, "Oujda Sunset", ArtistIds.YassineBenali, "MA-YBN-26-00001", new(2026, 12, 4), "Rai", TrackStatus.Submitted),
        Track(TrackIds.CasaFlow, "Casa Flow", ArtistIds.YassineBenali, "MA-YBN-26-00002", new(2027, 3, 10), "Hip-Hop", TrackStatus.Draft),
    ];

    public static readonly object[] TrackDistributions =
    [
        // Shobra Nights - Distributed
        Distribution(1, TrackIds.ShobraNights, DspIds.Spotify, Utc(2025, 2, 20, 9, 0), DistributionStatus.Live),
        Distribution(2, TrackIds.ShobraNights, DspIds.YouTubeMusic, Utc(2025, 2, 20, 9, 5), DistributionStatus.Live),
        Distribution(3, TrackIds.ShobraNights, DspIds.AppleMusic, Utc(2025, 2, 20, 9, 10), DistributionStatus.Rejected),

        // Tok Tok Anthem - Submitted
        Distribution(4, TrackIds.TokTokAnthem, DspIds.Spotify, Utc(2026, 10, 15, 11, 30), DistributionStatus.Pending),
        Distribution(5, TrackIds.TokTokAnthem, DspIds.YouTubeMusic, Utc(2026, 10, 15, 11, 35), DistributionStatus.Pending),

        // Desert Rain - Distributed
        Distribution(6, TrackIds.DesertRain, DspIds.Spotify, Utc(2025, 5, 28, 8, 0), DistributionStatus.Live),
        Distribution(7, TrackIds.DesertRain, DspIds.AppleMusic, Utc(2025, 5, 28, 8, 0), DistributionStatus.Live),
        Distribution(8, TrackIds.DesertRain, DspIds.YouTubeMusic, Utc(2025, 5, 28, 8, 0), DistributionStatus.Live),

        // Najd Moon - Distributed
        Distribution(9, TrackIds.NajdMoon, DspIds.Spotify, Utc(2026, 8, 25, 14, 0), DistributionStatus.Live),
        Distribution(10, TrackIds.NajdMoon, DspIds.AppleMusic, Utc(2026, 8, 25, 14, 0), DistributionStatus.Pending),

        // Beirut Balcony - Distributed
        Distribution(11, TrackIds.BeirutBalcony, DspIds.AppleMusic, Utc(2025, 8, 18, 10, 0), DistributionStatus.Live),
        Distribution(12, TrackIds.BeirutBalcony, DspIds.Spotify, Utc(2025, 8, 18, 10, 15), DistributionStatus.Live),

        // Cedar Heart - Submitted
        Distribution(13, TrackIds.CedarHeart, DspIds.AppleMusic, Utc(2026, 10, 18, 16, 45), DistributionStatus.Pending),

        // Oujda Sunset - Submitted
        Distribution(14, TrackIds.OujdaSunset, DspIds.Spotify, Utc(2026, 10, 20, 7, 0), DistributionStatus.Pending),
        Distribution(15, TrackIds.OujdaSunset, DspIds.AppleMusic, Utc(2026, 10, 20, 7, 0), DistributionStatus.Pending),
        Distribution(16, TrackIds.OujdaSunset, DspIds.YouTubeMusic, Utc(2026, 10, 20, 7, 0), DistributionStatus.Pending),
    ];

    private static object Artist(Guid id, string name, string email, string country) => new
    {
        Id = id,
        Name = name,
        Email = Email.Create(email),
        Country = CountryCode.Create(country),
    };

    private static object Track(
        Guid id, string title, Guid artistId, string isrc, DateOnly releaseDate, string genre, TrackStatus status) => new
    {
        Id = id,
        Title = title,
        ArtistId = artistId,
        Isrc = Isrc.Create(isrc),
        ReleaseDate = releaseDate,
        Genre = genre,
        Status = status,
    };

    private static object Distribution(
        int number, Guid trackId, Guid dspId, DateTimeOffset submittedAt, DistributionStatus status) => new
    {
        Id = new Guid($"c0000000-0000-0000-0000-{number:D12}"),
        TrackId = trackId,
        DspId = dspId,
        SubmittedAt = submittedAt,
        Status = status,
    };

    private static DateTimeOffset Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, TimeSpan.Zero);
}
