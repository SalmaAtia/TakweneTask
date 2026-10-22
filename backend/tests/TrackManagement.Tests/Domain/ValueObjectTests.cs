using TrackManagement.Domain.Common.Exceptions;
using TrackManagement.Domain.ValueObjects;

namespace TrackManagement.Tests.Domain;

public sealed class IsrcTests
{
    [Theory]
    [InlineData("US-RC1-76-07839")]
    [InlineData("USRC17607839")]
    [InlineData("us-rc1-76-07839")]
    [InlineData(" US RC1 76 07839 ")]
    public void Accepts_input_with_or_without_hyphens_and_normalizes_it(string input)
    {
        Assert.Equal("US-RC1-76-07839", Isrc.Create(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US-RC1-76-0783")]    // too short
    [InlineData("US-RC1-76-078390")]  // too long
    [InlineData("1S-RC1-76-07839")]   // country code must be letters
    [InlineData("US-RC1-7A-07839")]   // year must be digits
    [InlineData("US-RC1-76-0783X")]   // designation must be digits
    [InlineData("US_RC1_76_07839")]   // only hyphens/spaces are ignored
    public void Rejects_invalid_input(string? input)
    {
        Assert.Throws<DomainException>(() => Isrc.Create(input));
    }
}

public sealed class EmailTests
{
    [Fact]
    public void Trims_and_lowercases()
    {
        Assert.Equal("nour@example.com", Email.Create("  Nour@Example.COM ").Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nour")]
    [InlineData("nour@")]
    [InlineData("@example.com")]
    [InlineData("nour@example")]
    [InlineData("nour el@example.com")]
    [InlineData("nour@@example.com")]
    public void Rejects_invalid_input(string? input)
    {
        Assert.Throws<DomainException>(() => Email.Create(input));
    }

    [Fact]
    public void Rejects_input_longer_than_max_length()
    {
        var tooLong = new string('a', Email.MaxLength - "@example.com".Length + 1) + "@example.com";

        Assert.Throws<DomainException>(() => Email.Create(tooLong));
    }
}

public sealed class CountryCodeTests
{
    [Theory]
    [InlineData("EG")]
    [InlineData("eg")]
    [InlineData(" Eg ")]
    public void Trims_and_uppercases(string input)
    {
        Assert.Equal("EG", CountryCode.Create(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("E")]
    [InlineData("EGY")]
    [InlineData("E1")]
    [InlineData("É G")]
    public void Rejects_invalid_input(string? input)
    {
        Assert.Throws<DomainException>(() => CountryCode.Create(input));
    }
}
