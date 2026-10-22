using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackManagement.Domain.Dsps;
using TrackManagement.Domain.Tracks;
using TrackManagement.Infrastructure.Persistence.Seed;
using TrackManagement.Shared;

namespace TrackManagement.Infrastructure.Persistence.Configurations;

internal sealed class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.ToTable("TrackDistributions");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.SubmittedAt).IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(ValidationConstants.Status.MaxLength)
            .IsRequired();

        // A track can be sent to a given DSP only once.
        builder.HasIndex(d => new { d.TrackId, d.DspId }).IsUnique();

        // Dsp is a separate aggregate: FK only, no navigation.
        // Restrict: a DSP with distribution history can't be deleted out from under it.
        // (Also avoids SQL Server's multiple-cascade-paths error.)
        builder.HasOne<Dsp>()
            .WithMany()
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.TrackDistributions);
    }
}
