using System;
using MetadataEditor.Services;
using Xunit;

namespace MetadataEditor.Tests;

public class UpdateServiceTests
{
    [Theory]
    [InlineData("2026.09.26.1", 2026, 9, 26, 1)]
    [InlineData("2026.9.26.42", 2026, 9, 26, 42)]
    [InlineData("1.0.0", 1, 0, 0, 0)]
    [InlineData("1.2.3.4", 1, 2, 3, 4)]
    [InlineData("2026.10.01.5", 2026, 10, 1, 5)]
    public void TestTryParseVersion(string input, int major, int minor, int build, int rev)
    {
        bool success = UpdateService.TryParseVersion(input, out var version);

        Assert.True(success);
        Assert.NotNull(version);
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(build, version.Build);
        Assert.Equal(rev, version.Revision);
    }

    [Fact]
    public void TestVersionComparison_NewerVersionDetected()
    {
        UpdateService.TryParseVersion("2026.09.26.1", out var v1);
        UpdateService.TryParseVersion("2026.09.26.2", out var v2);
        UpdateService.TryParseVersion("2026.09.27.1", out var v3);
        UpdateService.TryParseVersion("2027.01.01.1", out var v4);

        Assert.True(v2 > v1);
        Assert.True(v3 > v2);
        Assert.True(v4 > v3);
    }

    [Fact]
    public void TestGetCurrentVersionString_ReturnsValidFourPartVersion()
    {
        string verStr = UpdateService.GetCurrentVersionString();
        Assert.False(string.IsNullOrWhiteSpace(verStr));

        bool parsed = UpdateService.TryParseVersion(verStr, out var ver);
        Assert.True(parsed);
        Assert.NotNull(ver);
        Assert.True(ver.Major >= 1);
    }
}

