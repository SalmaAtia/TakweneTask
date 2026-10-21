using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.Tracks;
using TrackManagement.Domain.ValueObjects;
using TrackManagement.Infrastructure.Persistence.Seed;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

internal sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("Tracks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasMaxLength(Track.TitleMaxLength)
            .IsRequired();

        builder.Property(t => t.Isrc)
            .HasConversion(i => i.Value, v => Isrc.Create(v))
            .HasMaxLength(Isrc.Length)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        builder.Property(t => t.ReleaseDate).IsRequired();

        builder.Property(t => t.Genre)
            .HasMaxLength(Track.GenreMaxLength)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(t => t.Isrc).IsUnique();

        // Artist is a separate aggregate: FK only, no navigation.
        // Restrict: deleting an artist must not silently wipe their catalog.
        builder.HasOne<Artist>()
            .WithMany()
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        // Distributions are owned by the Track aggregate, so they go with it.
        builder.HasMany(t => t.Distributions)
            .WithOne()
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Distributions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.Tracks);
    }
}
