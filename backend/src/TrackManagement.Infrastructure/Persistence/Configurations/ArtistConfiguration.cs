using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Artists;
using TrackManagement.Domain.ValueObjects;
using TrackManagement.Infrastructure.Persistence.Seed;
using TrackManagement.Shared;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

internal sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("Artists");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name)
            .HasMaxLength(ValidationConstants.Artist.NameMaxLength)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasMaxLength(ValidationConstants.Email.MaxLength)
            .IsRequired();

        builder.Property(a => a.Country)
            .HasConversion(c => c.Value, v => CountryCode.Create(v))
            .HasMaxLength(ValidationConstants.CountryCode.Length)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(a => a.Email).IsUnique();

        builder.HasData(SeedData.Artists);
    }
}
