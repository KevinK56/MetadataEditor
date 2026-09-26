using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MetadataEditor.Models;
using MetadataEditor.Services;

namespace MetadataEditor.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private readonly UpdateService _updateService = new();

    private string _selectedTheme;
    private bool _checkForUpdatesOnStartup;
    private NfoFileType _defaultFileType;
    private string _defaultEncoding;
    private bool _backupBeforeSave;
    private bool _autoDetectArtwork;

    private string _updateStatusText = "Ready to check for updates.";
    private bool _isCheckingUpdate;
    private int _selectedTabIndex;

    public SettingsViewModel(SettingsService settingsService, int initialTabIndex = 0)
    {
        _settingsService = settingsService;
        _selectedTabIndex = initialTabIndex;

        // Initialize from current settings
        _selectedTheme = _settingsService.Settings.Theme;
        _checkForUpdatesOnStartup = _settingsService.Settings.CheckForUpdatesOnStartup;
        _defaultFileType = _settingsService.Settings.DefaultFileType;
        _defaultEncoding = _settingsService.Settings.DefaultEncoding;
        _backupBeforeSave = _settingsService.Settings.BackupBeforeSave;
        _autoDetectArtwork = _settingsService.Settings.AutoDetectArtwork;

        Themes = new ObservableCollection<string> { "Dark", "Midnight", "Light" };
        Encodings = new ObservableCollection<string> { "UTF-8", "UTF-8 (with BOM)", "DOS CP437 (ASCII)", "Windows-1252 (ANSI)" };
        FileTypes = new ObservableCollection<NfoFileType>
        {
            NfoFileType.Movie,
            NfoFileType.TvShow,
            NfoFileType.Episode,
            NfoFileType.MusicAlbum,
            NfoFileType.MusicArtist,
            NfoFileType.PlainText
        };

        CheckForUpdatesCommand = new RelayCommand(_ => _ = ExecuteCheckUpdates());
        OpenUrlCommand = new RelayCommand(p => ExecuteOpenUrl(p as string));
        SaveCommand = new RelayCommand(_ => SaveSettings());
    }

    public ObservableCollection<string> Themes { get; }
    public ObservableCollection<string> Encodings { get; }
    public ObservableCollection<NfoFileType> FileTypes { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetField(ref _selectedTheme, value))
            {
                ThemeService.ApplyTheme(value);
            }
        }
    }

    public bool CheckForUpdatesOnStartup
    {
        get => _checkForUpdatesOnStartup;
        set => SetField(ref _checkForUpdatesOnStartup, value);
    }

    public NfoFileType DefaultFileType
    {
        get => _defaultFileType;
        set => SetField(ref _defaultFileType, value);
    }

    public string DefaultEncoding
    {
        get => _defaultEncoding;
        set => SetField(ref _defaultEncoding, value);
    }

    public bool BackupBeforeSave
    {
        get => _backupBeforeSave;
        set => SetField(ref _backupBeforeSave, value);
    }

    public bool AutoDetectArtwork
    {
        get => _autoDetectArtwork;
        set => SetField(ref _autoDetectArtwork, value);
    }

    public string AppVersionString => UpdateService.GetCurrentVersionString();

    public string UpdateStatusText
    {
        get => _updateStatusText;
        set => SetField(ref _updateStatusText, value);
    }

    public bool IsCheckingUpdate
    {
        get => _isCheckingUpdate;
        set => SetField(ref _isCheckingUpdate, value);
    }

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand SaveCommand { get; }

    public void SaveSettings()
    {
        _settingsService.Settings.Theme = SelectedTheme;
        _settingsService.Settings.CheckForUpdatesOnStartup = CheckForUpdatesOnStartup;
        _settingsService.Settings.DefaultFileType = DefaultFileType;
        _settingsService.Settings.DefaultEncoding = DefaultEncoding;
        _settingsService.Settings.BackupBeforeSave = BackupBeforeSave;
        _settingsService.Settings.AutoDetectArtwork = AutoDetectArtwork;
        _settingsService.Save();
    }

    private async Task ExecuteCheckUpdates()
    {
        try
        {
            IsCheckingUpdate = true;
            UpdateStatusText = "Connecting to GitHub Releases...";
            var update = await _updateService.CheckForUpdatesAsync();
            if (update != null)
            {
                UpdateStatusText = $"Update available: v{update.LatestVersion}! Released {update.PublishedAt?.ToShortDateString() ?? ""}";
            }
            else
            {
                UpdateStatusText = $"You are running the latest version (v{AppVersionString}).";
            }
        }
        catch (Exception ex)
        {
            UpdateStatusText = $"Failed to check for updates: {ex.Message}";
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }

    private void ExecuteOpenUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

