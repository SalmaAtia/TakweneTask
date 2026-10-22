using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Dsps;
using TrackManagement.Infrastructure.Persistence.Seed;
using TrackManagement.Shared;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

internal sealed class DspConfiguration : IEntityTypeConfiguration<Dsp>
{
    public void Configure(EntityTypeBuilder<Dsp> builder)
    {
        builder.ToTable("Dsps");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Name)
            .HasMaxLength(ValidationConstants.Dsp.NameMaxLength)
            .IsRequired();

        builder.HasIndex(d => d.Name).IsUnique();

        builder.HasData(SeedData.Dsps);
    }
}
