using System;

namespace MetadataEditor.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Dark"; // "Dark", "Midnight", "Light"
    public bool CheckForUpdatesOnStartup { get; set; } = true;
    public NfoFileType DefaultFileType { get; set; } = NfoFileType.Movie;
    public string DefaultEncoding { get; set; } = "UTF-8";
    public bool BackupBeforeSave { get; set; } = false;
    public bool AutoDetectArtwork { get; set; } = true;
}

