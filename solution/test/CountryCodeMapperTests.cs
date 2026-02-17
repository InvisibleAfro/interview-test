using HrmFileImport.Mappers;
using Xunit;

namespace HrmFileImport.Tests;

public sealed class CountryCodeMapperTests
{
    private readonly CountryCodeMapper _countryCodeMapper = new();

    [Fact]
    public void ToAlpha3_ValidCode_ReturnsAlpha3()
    {
        var result = _countryCodeMapper.ToAlpha3("NO");

        Assert.Equal("NOR", result);
    }

    [Fact]
    public void ToDialCode_ValidCode_ReturnsDialCode()
    {
        var result = _countryCodeMapper.ToDialCode("SE");

        Assert.Equal("+46", result);
    }

    [Fact]
    public void ToAlpha3_IsCaseInsensitive()
    {
        var result = _countryCodeMapper.ToAlpha3("no");

        Assert.Equal("NOR", result);
    }

    [Fact]
    public void ToDialCode_NullOrWhitespace_ReturnsNull()
    {
        Assert.Null(_countryCodeMapper.ToDialCode(null));
        Assert.Null(_countryCodeMapper.ToDialCode(""));
        Assert.Null(_countryCodeMapper.ToDialCode("   "));
    }

    [Fact]
    public void ToAlpha3_UnknownCode_Throws()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _countryCodeMapper.ToAlpha3("XX"));

        Assert.Contains("XX", ex.Message);
    }

    [Fact]
    public void ToDialCode_UnknownCode_Throws()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _countryCodeMapper.ToDialCode("XX"));

        Assert.Contains("XX", ex.Message);
    }
}
