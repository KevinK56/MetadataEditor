using MetadataEditor.Models;
using MetadataEditor.Services;
using Xunit;

namespace MetadataEditor.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void TestSettingsService_DefaultValues()
    {
        var settings = new AppSettings();

        Assert.Equal("Dark", settings.Theme);
        Assert.True(settings.CheckForUpdatesOnStartup);
        Assert.Equal(NfoFileType.Movie, settings.DefaultFileType);
        Assert.Equal("UTF-8", settings.DefaultEncoding);
        Assert.False(settings.BackupBeforeSave);
        Assert.True(settings.AutoDetectArtwork);
    }

    [Fact]
    public void TestSettingsService_SaveAndReload()
    {
        var service = new SettingsService();
        service.Settings.Theme = "Midnight";
        service.Settings.BackupBeforeSave = true;
        service.Save();

        var reloaded = new SettingsService();
        Assert.Equal("Midnight", reloaded.Settings.Theme);
        Assert.True(reloaded.Settings.BackupBeforeSave);

        // Reset to default
        service.Settings.Theme = "Dark";
        service.Settings.BackupBeforeSave = false;
        service.Save();
    }
}

