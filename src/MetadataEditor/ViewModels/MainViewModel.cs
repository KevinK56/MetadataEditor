using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MetadataEditor.Models;
using MetadataEditor.Services;

namespace MetadataEditor.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private NfoMetadata _metadata = new();
    private string _rawText = string.Empty;
    private string? _currentFilePath;
    private string _currentFileName = "Untitled.nfo";
    private bool _isModified;
    private string _statusMessage = "Ready";
    private string _encodingName = "UTF-8";
    private Encoding _currentEncoding = new UTF8Encoding(false);
    private bool _hasBom;
    private int _selectedTabIndex;
    private string _searchText = string.Empty;
    private string _selectedFilterType = "All";
    private FileListItem? _selectedFileItem;
    private string? _currentFolderPath;
    private string? _artworkImagePath;
    private bool _hasArtwork;
    private ArtworkItem? _selectedArtworkItem;

    // Sub-item input buffers for quick adding
    private string _newGenreText = string.Empty;
    private string _newStudioText = string.Empty;
    private string _newDirectorText = string.Empty;
    private string _newWriterText = string.Empty;
    private string _newCountryText = string.Empty;

    // Auto-Update & Settings
    private readonly UpdateService _updateService = new();
    private readonly SettingsService _settingsService = new();
    private UpdateInfo? _availableUpdate;
    private bool _isUpdateAvailable;
    private bool _isDownloadingUpdate;
    private double _updateDownloadProgress;
    private string _updateStatusText = string.Empty;

    public MainViewModel()
    {
        SupportedEncodings = new ObservableCollection<EncodingInfoItem>(FileEncodingDetector.GetSupportedEncodings());
        FilterTypes = new ObservableCollection<string> { "All", "Movies", "TV Shows", "Episodes", "Music", "Plain Text" };

        // Commands
        OpenFileCommand = new RelayCommand(_ => ExecuteOpenFile());
        OpenFolderCommand = new RelayCommand(_ => ExecuteOpenFolder());
        SaveCommand = new RelayCommand(_ => ExecuteSave(), () => CanSave());
        SaveAsCommand = new RelayCommand(_ => ExecuteSaveAs());
        NewNfoCommand = new RelayCommand(p => ExecuteNewNfo(p as string));
        ReloadCommand = new RelayCommand(_ => ExecuteReload(), () => !string.IsNullOrEmpty(CurrentFilePath));
        FormatXmlCommand = new RelayCommand(_ => ExecuteFormatXml());
        OpenInBrowserCommand = new RelayCommand(p => ExecuteOpenInBrowser(p as string));

        // Auto-update commands
        CheckForUpdatesCommand = new RelayCommand(_ => _ = CheckForUpdatesAsync(silent: false));
        DownloadAndInstallUpdateCommand = new RelayCommand(_ => _ = ExecuteDownloadAndInstallUpdate());
        DismissUpdateCommand = new RelayCommand(_ => IsUpdateAvailable = false);

        // Menu Bar & Settings commands
        OpenSettingsCommand = new RelayCommand(_ => ExecuteOpenSettings(0));
        OpenThemeSettingsCommand = new RelayCommand(_ => ExecuteOpenSettings(0));
        OpenUpdateSettingsCommand = new RelayCommand(_ => ExecuteOpenSettings(1));
        OpenAboutCommand = new RelayCommand(_ => ExecuteOpenSettings(3));
        ChangeThemeCommand = new RelayCommand(p => ExecuteChangeTheme(p as string));
        ExitCommand = new RelayCommand(_ => ExecuteExit());
        ConvertTypeCommand = new RelayCommand(p => ExecuteConvertType(p));
        ScanArtworkCommand = new RelayCommand(_ => ExecuteScanArtwork());
        SyncArtworkToXmlCommand = new RelayCommand(_ => ExecuteSyncArtworkToXml());
        OpenArtworkFileCommand = new RelayCommand(p => ExecuteOpenArtworkFile(p as ArtworkItem));

        // List item commands
        AddGenreCommand = new RelayCommand(_ => ExecuteAddGenre());
        RemoveGenreCommand = new RelayCommand(p => ExecuteRemoveGenre(p as string));
        AddStudioCommand = new RelayCommand(_ => ExecuteAddStudio());
        RemoveStudioCommand = new RelayCommand(p => ExecuteRemoveStudio(p as string));
        AddDirectorCommand = new RelayCommand(_ => ExecuteAddDirector());
        RemoveDirectorCommand = new RelayCommand(p => ExecuteRemoveDirector(p as string));
        AddWriterCommand = new RelayCommand(_ => ExecuteAddWriter());
        RemoveWriterCommand = new RelayCommand(p => ExecuteRemoveWriter(p as string));
        AddCountryCommand = new RelayCommand(_ => ExecuteAddCountry());
        RemoveCountryCommand = new RelayCommand(p => ExecuteRemoveCountry(p as string));

        AddActorCommand = new RelayCommand(_ => ExecuteAddActor());
        RemoveActorCommand = new RelayCommand(p => ExecuteRemoveActor(p as ActorItem));
        MoveActorUpCommand = new RelayCommand(p => ExecuteMoveActor(p as ActorItem, -1));
        MoveActorDownCommand = new RelayCommand(p => ExecuteMoveActor(p as ActorItem, 1));

        AddTrackCommand = new RelayCommand(_ => ExecuteAddTrack());
        RemoveTrackCommand = new RelayCommand(p => ExecuteRemoveTrack(p as TrackItem));

        AddExtraNodeCommand = new RelayCommand(_ => ExecuteAddExtraNode());
        RemoveExtraNodeCommand = new RelayCommand(p => ExecuteRemoveExtraNode(p as XmlExtraItem));

        // Apply saved theme
        ThemeService.ApplyTheme(_settingsService.Settings.Theme);

        // Create default template using settings
        InitTemplate(_settingsService.Settings.DefaultFileType);

        // Check for updates asynchronously after launch if enabled
        if (_settingsService.Settings.CheckForUpdatesOnStartup)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(2500);
                await CheckForUpdatesAsync(silent: true);
            });
        }
    }

    public ObservableCollection<EncodingInfoItem> SupportedEncodings { get; }
    public ObservableCollection<string> FilterTypes { get; }
    public ObservableCollection<FileListItem> AllFiles { get; } = new();
    public ObservableCollection<FileListItem> FilteredFiles { get; } = new();
    public ObservableCollection<ArtworkItem> ArtworkItems { get; } = new();

    public NfoMetadata Metadata
    {
        get => _metadata;
        set
        {
            if (_metadata != null)
            {
                _metadata.DataModified -= OnMetadataModified;
            }
            var newMeta = value ?? new NfoMetadata();
            if (!ReferenceEquals(_metadata, newMeta))
            {
                _metadata = newMeta;
                _metadata.DataModified += OnMetadataModified;
                OnPropertyChanged(nameof(Metadata));
                OnPropertyChanged(nameof(SelectedFileType));
                OnPropertyChanged(nameof(IsMovieType));
                OnPropertyChanged(nameof(IsTvShowType));
                OnPropertyChanged(nameof(IsEpisodeType));
                OnPropertyChanged(nameof(IsMusicType));
                OnPropertyChanged(nameof(IsGenericXmlType));
                OnPropertyChanged(nameof(IsPlainTextType));
            }
        }
    }

    private void OnMetadataModified(object? sender, EventArgs e)
    {
        IsModified = true;
    }

    public string RawText
    {
        get => _rawText;
        set
        {
            if (SetField(ref _rawText, value))
            {
                IsModified = true;
            }
        }
    }

    public string? CurrentFilePath
    {
        get => _currentFilePath;
        set
        {
            if (SetField(ref _currentFilePath, value))
            {
                CurrentFileName = string.IsNullOrEmpty(value) ? "Untitled.nfo" : Path.GetFileName(value);
                CheckArtwork();
            }
        }
    }

    public string CurrentFileName
    {
        get => _currentFileName;
        set => SetField(ref _currentFileName, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (SetField(ref _isModified, value))
            {
                if (SelectedFileItem != null && SelectedFileItem.FilePath == CurrentFilePath)
                {
                    SelectedFileItem.IsModified = value;
                }
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public string EncodingName
    {
        get => _encodingName;
        set => SetField(ref _encodingName, value);
    }

    public Encoding CurrentEncoding
    {
        get => _currentEncoding;
        set => SetField(ref _currentEncoding, value);
    }

    public bool HasBom
    {
        get => _hasBom;
        set => SetField(ref _hasBom, value);
    }

    private EncodingInfoItem? _selectedEncodingItem;
    public EncodingInfoItem? SelectedEncodingItem
    {
        get => _selectedEncodingItem ?? SupportedEncodings.FirstOrDefault();
        set
        {
            if (SetField(ref _selectedEncodingItem, value) && value != null)
            {
                CurrentEncoding = value.Encoding;
                HasBom = value.HasBom;
                EncodingName = value.DisplayName;
                IsModified = true;
            }
        }
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            int prev = _selectedTabIndex;
            if (SetField(ref _selectedTabIndex, value))
            {
                OnTabChanged(prev, value);
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                ApplyFileFilters();
            }
        }
    }

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set
        {
            if (SetField(ref _selectedFilterType, value))
            {
                ApplyFileFilters();
            }
        }
    }

    public FileListItem? SelectedFileItem
    {
        get => _selectedFileItem;
        set
        {
            if (SetField(ref _selectedFileItem, value) && value != null)
            {
                if (!string.Equals(value.FilePath, CurrentFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    LoadFile(value.FilePath);
                }
            }
        }
    }

    public string? CurrentFolderPath
    {
        get => _currentFolderPath;
        set => SetField(ref _currentFolderPath, value);
    }

    public string? ArtworkImagePath
    {
        get => _artworkImagePath;
        set => SetField(ref _artworkImagePath, value);
    }

    public bool HasArtwork
    {
        get => _hasArtwork;
        set => SetField(ref _hasArtwork, value);
    }

    public ArtworkItem? SelectedArtworkItem
    {
        get => _selectedArtworkItem;
        set
        {
            if (SetField(ref _selectedArtworkItem, value) && value != null)
            {
                ArtworkImagePath = value.FilePath;
            }
        }
    }

    // Input buffers
    public string NewGenreText
    {
        get => _newGenreText;
        set => SetField(ref _newGenreText, value);
    }

    public string NewStudioText
    {
        get => _newStudioText;
        set => SetField(ref _newStudioText, value);
    }

    public string NewDirectorText
    {
        get => _newDirectorText;
        set => SetField(ref _newDirectorText, value);
    }

    public string NewWriterText
    {
        get => _newWriterText;
        set => SetField(ref _newWriterText, value);
    }

    public string NewCountryText
    {
        get => _newCountryText;
        set => SetField(ref _newCountryText, value);
    }

    // Type views
    public NfoFileType SelectedFileType
    {
        get => Metadata.FileType;
        set
        {
            if (Metadata.FileType != value)
            {
                Metadata.FileType = value;
                OnPropertyChanged(nameof(SelectedFileType));
                OnPropertyChanged(nameof(IsMovieType));
                OnPropertyChanged(nameof(IsTvShowType));
                OnPropertyChanged(nameof(IsEpisodeType));
                OnPropertyChanged(nameof(IsMusicType));
                OnPropertyChanged(nameof(IsGenericXmlType));
                OnPropertyChanged(nameof(IsPlainTextType));
                IsModified = true;

                if (SelectedFileItem != null && SelectedFileItem.FilePath == CurrentFilePath)
                {
                    SelectedFileItem.FileType = value;
                }
            }
        }
    }

    public bool IsMovieType => Metadata.FileType == NfoFileType.Movie;
    public bool IsTvShowType => Metadata.FileType == NfoFileType.TvShow;
    public bool IsEpisodeType => Metadata.FileType == NfoFileType.Episode;
    public bool IsMusicType => Metadata.FileType == NfoFileType.MusicAlbum || Metadata.FileType == NfoFileType.MusicArtist || Metadata.FileType == NfoFileType.MusicVideo;
    public bool IsGenericXmlType => Metadata.FileType == NfoFileType.GenericXml;
    public bool IsPlainTextType => Metadata.FileType == NfoFileType.PlainText;

    // Commands
    public ICommand OpenFileCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand NewNfoCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand FormatXmlCommand { get; }
    public ICommand OpenInBrowserCommand { get; }

    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenThemeSettingsCommand { get; }
    public ICommand OpenUpdateSettingsCommand { get; }
    public ICommand OpenAboutCommand { get; }
    public ICommand ChangeThemeCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand ConvertTypeCommand { get; }
    public ICommand ScanArtworkCommand { get; }
    public ICommand SyncArtworkToXmlCommand { get; }
    public ICommand OpenArtworkFileCommand { get; }

    public ICommand AddGenreCommand { get; }
    public ICommand RemoveGenreCommand { get; }
    public ICommand AddStudioCommand { get; }
    public ICommand RemoveStudioCommand { get; }
    public ICommand AddDirectorCommand { get; }
    public ICommand RemoveDirectorCommand { get; }
    public ICommand AddWriterCommand { get; }
    public ICommand RemoveWriterCommand { get; }
    public ICommand AddCountryCommand { get; }
    public ICommand RemoveCountryCommand { get; }
    public ICommand AddActorCommand { get; }
    public ICommand RemoveActorCommand { get; }
    public ICommand MoveActorUpCommand { get; }
    public ICommand MoveActorDownCommand { get; }
    public ICommand AddTrackCommand { get; }
    public ICommand RemoveTrackCommand { get; }
    public ICommand AddExtraNodeCommand { get; }
    public ICommand RemoveExtraNodeCommand { get; }

    private void OnTabChanged(int previousTab, int newTab)
    {
        // 0 = Visual Editor, 1 = Raw Text Editor, 2 = Artwork Preview
        if (previousTab == 0 && newTab == 1)
        {
            // Sync from visual to raw text
            if (Metadata.FileType != NfoFileType.PlainText)
            {
                bool wasModified = IsModified;
                RawText = NfoParserService.SerializeToXml(Metadata);
                IsModified = wasModified;
            }
        }
        else if (previousTab == 1 && newTab == 0)
        {
            // Sync from raw text to visual
            if (!string.IsNullOrWhiteSpace(RawText))
            {
                try
                {
                    bool wasModified = IsModified;
                    var doc = System.Xml.Linq.XDocument.Parse(RawText);
                    var newMeta = NfoParserService.ParseXmlToMetadata(doc, RawText);
                    Metadata = newMeta;
                    IsModified = wasModified;
                    UpdateArtworkXmlLinkStatus();
                    StatusMessage = "Visual editor updated from raw XML source.";
                }
                catch (Exception)
                {
                    StatusMessage = "Note: Raw text is not valid XML. Showing in plain text / current mode.";
                }
            }
        }
        else if (newTab == 2)
        {
            UpdateArtworkXmlLinkStatus();
        }
    }

    public void LoadFile(string filePath)
    {
        if (IsModified && !PromptSaveBeforeAction())
        {
            return;
        }

        try
        {
            var result = NfoParserService.LoadFromFile(filePath);
            Metadata = result.Metadata;
            RawText = result.RawText;
            CurrentFilePath = filePath;
            CurrentEncoding = result.Encoding;
            HasBom = result.HasBom;
            EncodingName = result.EncodingName;
            _selectedEncodingItem = SupportedEncodings.FirstOrDefault(x => x.DisplayName.StartsWith(result.EncodingName, StringComparison.OrdinalIgnoreCase)) ?? SupportedEncodings.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedEncodingItem));
            IsModified = false;
            StatusMessage = $"Loaded {Path.GetFileName(filePath)} ({result.EncodingName})";

            // If it's plain text or ASCII art, switch to tab 1 (Raw Text / ASCII tab)
            if (result.Metadata.FileType == NfoFileType.PlainText)
            {
                SelectedTabIndex = 1;
            }
            else if (SelectedTabIndex == 1 && result.Metadata.FileType != NfoFileType.PlainText)
            {
                SelectedTabIndex = 0;
            }

            CheckArtwork();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load file:\n{ex.Message}", "Error Loading NFO", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExecuteOpenFile()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Open NFO or Metadata File",
            Filter = "NFO Files (*.nfo)|*.nfo|XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            Multiselect = false
        };

        if (dlg.ShowDialog() == true)
        {
            LoadFile(dlg.FileName);
        }
    }

    public void ExecuteOpenFolder(string? folder = null)
    {
        if (folder == null)
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Select Folder Containing Media & NFO Files",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
            {
                return;
            }
            folder = dlg.FolderName;
        }

        if (!Directory.Exists(folder)) return;

        CurrentFolderPath = folder;
        AllFiles.Clear();

        try
        {
            var files = Directory.GetFiles(folder, "*.nfo", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var item = new FileListItem { FilePath = f };
                // Quick inspect first 512 bytes to determine file type icon
                try
                {
                    using var stream = File.OpenRead(f);
                    using var reader = new StreamReader(stream, true);
                    char[] buffer = new char[512];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    string snippet = new string(buffer, 0, read).ToLowerInvariant();

                    if (snippet.Contains("<movie")) item.FileType = NfoFileType.Movie;
                    else if (snippet.Contains("<tvshow")) item.FileType = NfoFileType.TvShow;
                    else if (snippet.Contains("<episodedetails") || snippet.Contains("<episode")) item.FileType = NfoFileType.Episode;
                    else if (snippet.Contains("<album")) item.FileType = NfoFileType.MusicAlbum;
                    else if (snippet.Contains("<artist")) item.FileType = NfoFileType.MusicArtist;
                    else if (snippet.Contains("<?xml") || snippet.Contains("<")) item.FileType = NfoFileType.GenericXml;
                    else item.FileType = NfoFileType.PlainText;
                }
                catch
                {
                    item.FileType = NfoFileType.PlainText;
                }

                AllFiles.Add(item);
            }

            ApplyFileFilters();
            StatusMessage = $"Scanned {AllFiles.Count} NFO file(s) in {Path.GetFileName(folder)}";

            if (AllFiles.Count > 0 && SelectedFileItem == null)
            {
                SelectedFileItem = AllFiles[0];
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error scanning folder:\n{ex.Message}", "Folder Scan Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyFileFilters()
    {
        FilteredFiles.Clear();
        string q = SearchText.Trim().ToLowerInvariant();

        foreach (var file in AllFiles)
        {
            // Type filter
            bool typeMatch = SelectedFilterType switch
            {
                "Movies" => file.FileType == NfoFileType.Movie,
                "TV Shows" => file.FileType == NfoFileType.TvShow,
                "Episodes" => file.FileType == NfoFileType.Episode,
                "Music" => file.FileType == NfoFileType.MusicAlbum || file.FileType == NfoFileType.MusicArtist || file.FileType == NfoFileType.MusicVideo,
                "Plain Text" => file.FileType == NfoFileType.PlainText,
                _ => true
            };

            if (!typeMatch) continue;

            // Search query filter
            if (!string.IsNullOrEmpty(q))
            {
                if (!file.FileName.ToLowerInvariant().Contains(q) && !file.DirectoryName.ToLowerInvariant().Contains(q))
                {
                    continue;
                }
            }

            FilteredFiles.Add(file);
        }
    }

    private bool CanSave() => IsModified || !string.IsNullOrEmpty(CurrentFilePath);

    public void ExecuteSave()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            ExecuteSaveAs();
            return;
        }

        SaveToFile(CurrentFilePath);
    }

    public void ExecuteSaveAs()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Save NFO File As",
            Filter = "NFO File (*.nfo)|*.nfo|XML File (*.xml)|*.xml|All Files (*.*)|*.*",
            FileName = CurrentFileName
        };

        if (dlg.ShowDialog() == true)
        {
            SaveToFile(dlg.FileName);
            CurrentFilePath = dlg.FileName;
        }
    }

    private void SaveToFile(string targetPath)
    {
        try
        {
            if (_settingsService.Settings.BackupBeforeSave && File.Exists(targetPath))
            {
                try
                {
                    string backupPath = targetPath + ".bak";
                    File.Copy(targetPath, backupPath, true);
                }
                catch
                {
                    // Ignore backup copy errors
                }
            }

            // If saving in Visual Tab or Artwork Tab mode and it's XML, serialize visual model
            string textToSave;
            if (SelectedTabIndex != 1 && Metadata.FileType != NfoFileType.PlainText)
            {
                textToSave = NfoParserService.SerializeToXml(Metadata);
                RawText = textToSave;
            }
            else
            {
                textToSave = RawText;
            }

            NfoParserService.SaveToFile(targetPath, textToSave, CurrentEncoding, HasBom);
            IsModified = false;
            StatusMessage = $"Saved successfully: {Path.GetFileName(targetPath)} at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save file:\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ExecuteNewNfo(string? typeName = null)
    {
        if (IsModified && !PromptSaveBeforeAction())
        {
            return;
        }

        NfoFileType type = NfoFileType.Movie;
        if (!string.IsNullOrEmpty(typeName))
        {
            if (Enum.TryParse(typeName, true, out NfoFileType parsed))
            {
                type = parsed;
            }
        }

        InitTemplate(type);
        CurrentFilePath = null;
        CurrentFileName = $"New_{type}.nfo";
        IsModified = true;
        StatusMessage = $"Created new {type} NFO template";
    }

    private void InitTemplate(NfoFileType type)
    {
        Metadata = new NfoMetadata
        {
            FileType = type,
            Title = "New Title",
            Year = DateTime.Now.Year.ToString()
        };

        switch (type)
        {
            case NfoFileType.Movie:
                Metadata.RootElementName = "movie";
                Metadata.Genres.Add("Action");
                Metadata.Genres.Add("Adventure");
                Metadata.Plot = "Add movie plot summary here.";
                break;
            case NfoFileType.TvShow:
                Metadata.RootElementName = "tvshow";
                Metadata.Status = "Continuing";
                Metadata.Plot = "Add TV show series overview here.";
                break;
            case NfoFileType.Episode:
                Metadata.RootElementName = "episodedetails";
                Metadata.Season = "1";
                Metadata.Episode = "1";
                Metadata.ShowTitle = "Series Name";
                Metadata.Plot = "Add episode plot here.";
                break;
            case NfoFileType.MusicAlbum:
                Metadata.RootElementName = "album";
                Metadata.Artist = "Artist Name";
                Metadata.Album = "Album Name";
                Metadata.AlbumType = "Album";
                Metadata.Tracks.Add(new TrackItem { Position = "1", Title = "Track 1", Duration = "3:30" });
                break;
            case NfoFileType.MusicArtist:
                Metadata.RootElementName = "artist";
                Metadata.Artist = "Artist Name";
                Metadata.ArtistType = "Group";
                Metadata.Biography = "Add artist biography here.";
                break;
            case NfoFileType.PlainText:
                Metadata.RawText = "Artist / Title / Release Details\r\n\r\nRelease notes and tracklist...";
                break;
        }

        RawText = type == NfoFileType.PlainText ? Metadata.RawText : NfoParserService.SerializeToXml(Metadata);
        SelectedTabIndex = type == NfoFileType.PlainText ? 1 : 0;
    }

    private void ExecuteReload()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath))
        {
            LoadFile(CurrentFilePath);
        }
    }

    private void ExecuteFormatXml()
    {
        try
        {
            string source = SelectedTabIndex == 0 ? NfoParserService.SerializeToXml(Metadata) : RawText;
            var doc = System.Xml.Linq.XDocument.Parse(source);
            var sb = new StringBuilder();
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };
            using (var writer = System.Xml.XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            RawText = sb.ToString();
            StatusMessage = "XML Formatted and formatted cleanly.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to format XML:\n{ex.Message}", "XML Format Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExecuteOpenInBrowser(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            if (url.StartsWith("tt") && url.Length >= 7) // IMDb ID
            {
                url = $"https://www.imdb.com/title/{url}/";
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open link:\n{ex.Message}", "Link Error", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // List item helpers
    private void ExecuteAddGenre()
    {
        if (!string.IsNullOrWhiteSpace(NewGenreText) && !Metadata.Genres.Contains(NewGenreText.Trim()))
        {
            Metadata.Genres.Add(NewGenreText.Trim());
            NewGenreText = string.Empty;
        }
    }

    private void ExecuteRemoveGenre(string? g)
    {
        if (g != null) Metadata.Genres.Remove(g);
    }

    private void ExecuteAddStudio()
    {
        if (!string.IsNullOrWhiteSpace(NewStudioText) && !Metadata.Studios.Contains(NewStudioText.Trim()))
        {
            Metadata.Studios.Add(NewStudioText.Trim());
            NewStudioText = string.Empty;
        }
    }

    private void ExecuteRemoveStudio(string? s)
    {
        if (s != null) Metadata.Studios.Remove(s);
    }

    private void ExecuteAddDirector()
    {
        if (!string.IsNullOrWhiteSpace(NewDirectorText) && !Metadata.Directors.Contains(NewDirectorText.Trim()))
        {
            Metadata.Directors.Add(NewDirectorText.Trim());
            NewDirectorText = string.Empty;
        }
    }

    private void ExecuteRemoveDirector(string? d)
    {
        if (d != null) Metadata.Directors.Remove(d);
    }

    private void ExecuteAddWriter()
    {
        if (!string.IsNullOrWhiteSpace(NewWriterText) && !Metadata.Writers.Contains(NewWriterText.Trim()))
        {
            Metadata.Writers.Add(NewWriterText.Trim());
            NewWriterText = string.Empty;
        }
    }

    private void ExecuteRemoveWriter(string? w)
    {
        if (w != null) Metadata.Writers.Remove(w);
    }

    private void ExecuteAddCountry()
    {
        if (!string.IsNullOrWhiteSpace(NewCountryText) && !Metadata.Countries.Contains(NewCountryText.Trim()))
        {
            Metadata.Countries.Add(NewCountryText.Trim());
            NewCountryText = string.Empty;
        }
    }

    private void ExecuteRemoveCountry(string? c)
    {
        if (c != null) Metadata.Countries.Remove(c);
    }

    private void ExecuteAddActor()
    {
        Metadata.Actors.Add(new ActorItem
        {
            Name = "New Actor",
            Role = "Character Name",
            Order = Metadata.Actors.Count
        });
    }

    private void ExecuteRemoveActor(ActorItem? actor)
    {
        if (actor != null) Metadata.Actors.Remove(actor);
    }

    private void ExecuteMoveActor(ActorItem? actor, int direction)
    {
        if (actor == null) return;
        int idx = Metadata.Actors.IndexOf(actor);
        int newIdx = idx + direction;
        if (newIdx >= 0 && newIdx < Metadata.Actors.Count)
        {
            Metadata.Actors.Move(idx, newIdx);
            for (int i = 0; i < Metadata.Actors.Count; i++)
            {
                Metadata.Actors[i].Order = i;
            }
        }
    }

    private void ExecuteAddTrack()
    {
        Metadata.Tracks.Add(new TrackItem
        {
            Position = (Metadata.Tracks.Count + 1).ToString(),
            Title = "New Track",
            Duration = "3:00"
        });
    }

    private void ExecuteRemoveTrack(TrackItem? track)
    {
        if (track != null) Metadata.Tracks.Remove(track);
    }

    private void ExecuteAddExtraNode()
    {
        Metadata.ExtraNodes.Add(new XmlExtraItem
        {
            TagName = "customtag",
            TagValue = "value"
        });
    }

    private void ExecuteRemoveExtraNode(XmlExtraItem? extra)
    {
        if (extra != null) Metadata.ExtraNodes.Remove(extra);
    }

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tbn"
    };

    private void CheckArtwork()
    {
        ArtworkItems.Clear();
        SelectedArtworkItem = null;
        ArtworkImagePath = null;
        HasArtwork = false;

        if (!_settingsService.Settings.AutoDetectArtwork) return;

        string? dir = !string.IsNullOrEmpty(CurrentFilePath)
            ? Path.GetDirectoryName(CurrentFilePath)
            : CurrentFolderPath;

        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

        string baseName = !string.IsNullOrEmpty(CurrentFilePath)
            ? Path.GetFileNameWithoutExtension(CurrentFilePath)
            : string.Empty;

        try
        {
            var imageFiles = Directory.GetFiles(dir)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f)))
                .ToList();

            var items = new List<(ArtworkItem Item, int SortPriority)>();

            foreach (var imgPath in imageFiles)
            {
                var info = new FileInfo(imgPath);
                string fileName = info.Name;
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                var (artType, aspect, priority) = ClassifyArtworkFile(nameWithoutExt, baseName);
                var (width, height) = GetImageDimensions(imgPath);

                string dims = (width > 0 && height > 0) ? $"{width} × {height}" : "Image";
                string sizeStr = FormatFileSize(info.Length);

                var artItem = new ArtworkItem
                {
                    FilePath = imgPath,
                    FileName = fileName,
                    ArtworkType = artType,
                    Aspect = aspect,
                    Dimensions = dims,
                    FileSizeFormatted = sizeStr
                };

                items.Add((artItem, priority));
            }

            foreach (var entry in items.OrderBy(x => x.SortPriority).ThenBy(x => x.Item.FileName, StringComparer.OrdinalIgnoreCase))
            {
                ArtworkItems.Add(entry.Item);
            }

            UpdateArtworkXmlLinkStatus();

            if (ArtworkItems.Count > 0)
            {
                HasArtwork = true;
                SelectedArtworkItem = ArtworkItems[0];
            }
        }
        catch
        {
            // Ignore directory read errors
        }
    }

    private static (string DisplayType, string Aspect, int Priority) ClassifyArtworkFile(string nameWithoutExt, string nfoBaseName)
    {
        string lower = nameWithoutExt.ToLowerInvariant();

        // Strip "<moviename>-" prefix if present
        if (!string.IsNullOrEmpty(nfoBaseName) && lower.StartsWith(nfoBaseName.ToLowerInvariant() + "-"))
        {
            lower = lower.Substring(nfoBaseName.Length + 1);
        }
        else if (lower.Contains('-'))
        {
            // Also check suffix after last hyphen (e.g. "Movie Title (1989)-poster")
            string suffix = lower.Substring(lower.LastIndexOf('-') + 1);
            if (IsKnownArtworkKeyword(suffix))
            {
                lower = suffix;
            }
        }

        if (lower == "poster" || lower == "folder" || lower == "cover" || lower == "movie" ||
            (!string.IsNullOrEmpty(nfoBaseName) && string.Equals(nameWithoutExt, nfoBaseName, StringComparison.OrdinalIgnoreCase)))
        {
            return ("Poster", "poster", 1);
        }
        if (lower.StartsWith("fanart") || lower.StartsWith("backdrop") || lower == "background" || lower == "art")
        {
            return ("Fanart / Backdrop", "fanart", 2);
        }
        if (lower == "clearlogo" || lower == "logo")
        {
            return ("ClearLogo", "clearlogo", 3);
        }
        if (lower == "landscape" || lower == "thumb")
        {
            return ("Landscape / Thumb", "landscape", 4);
        }
        if (lower == "banner")
        {
            return ("Banner", "banner", 5);
        }
        if (lower == "clearart")
        {
            return ("ClearArt", "clearart", 6);
        }
        if (lower == "disc" || lower == "discart" || lower == "cdart")
        {
            return ("DiscArt", "discart", 7);
        }
        if (lower == "keyart")
        {
            return ("KeyArt", "keyart", 8);
        }

        return ("Artwork", "thumb", 9);
    }

    private static bool IsKnownArtworkKeyword(string keyword)
    {
        return keyword is "poster" or "folder" or "cover" or "fanart" or "backdrop" or "background"
            or "clearlogo" or "logo" or "landscape" or "thumb" or "banner" or "clearart"
            or "disc" or "discart" or "cdart" or "keyart";
    }

    private static (int Width, int Height) GetImageDimensions(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var frame = System.Windows.Media.Imaging.BitmapFrame.Create(
                stream,
                System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,
                System.Windows.Media.Imaging.BitmapCacheOption.None);
            return (frame.PixelWidth, frame.PixelHeight);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):0.00} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:0.#} KB";
        return $"{bytes} B";
    }

    private void UpdateArtworkXmlLinkStatus()
    {
        foreach (var item in ArtworkItems)
        {
            bool linked = Metadata.ExtraNodes.Any(x =>
                !string.IsNullOrWhiteSpace(x.TagValue) &&
                x.TagValue.Contains(item.FileName, StringComparison.OrdinalIgnoreCase));
            item.IsLinkedInXml = linked;
        }
    }

    public void ExecuteSyncArtworkToXml()
    {
        if (Metadata.FileType == NfoFileType.PlainText)
        {
            MessageBox.Show("Cannot link XML artwork tags in Plain Text mode. Convert the format to Movie, TV Show, or Music first.", "Plain Text Mode", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (ArtworkItems.Count == 0)
        {
            CheckArtwork();
            if (ArtworkItems.Count == 0)
            {
                StatusMessage = "No local artwork files found in folder to link.";
                return;
            }
        }

        int addedCount = 0;
        var fanartFiles = new List<string>();

        foreach (var art in ArtworkItems)
        {
            if (art.Aspect == "fanart")
            {
                fanartFiles.Add(art.FileName);
            }
            else
            {
                bool alreadyExists = Metadata.ExtraNodes.Any(x =>
                    x.TagName.Equals("thumb", StringComparison.OrdinalIgnoreCase) &&
                    x.TagValue.Contains(art.FileName, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    string xmlBlock = $"<thumb aspect=\"{art.Aspect}\">{System.Security.SecurityElement.Escape(art.FileName)}</thumb>";
                    Metadata.ExtraNodes.Add(new XmlExtraItem
                    {
                        TagName = "thumb",
                        TagValue = xmlBlock,
                        IsXmlBlock = true
                    });
                    addedCount++;
                }
            }
        }

        if (fanartFiles.Count > 0)
        {
            var existingFanartNode = Metadata.ExtraNodes.FirstOrDefault(x =>
                x.TagName.Equals("fanart", StringComparison.OrdinalIgnoreCase));

            if (existingFanartNode == null)
            {
                var fanartElem = new System.Xml.Linq.XElement("fanart");
                foreach (var f in fanartFiles)
                {
                    fanartElem.Add(new System.Xml.Linq.XElement("thumb", f));
                }
                Metadata.ExtraNodes.Add(new XmlExtraItem
                {
                    TagName = "fanart",
                    TagValue = fanartElem.ToString(System.Xml.Linq.SaveOptions.DisableFormatting),
                    IsXmlBlock = true
                });
                addedCount += fanartFiles.Count;
            }
            else
            {
                try
                {
                    var fanartElem = existingFanartNode.IsXmlBlock
                        ? System.Xml.Linq.XElement.Parse(existingFanartNode.TagValue)
                        : new System.Xml.Linq.XElement("fanart", new System.Xml.Linq.XElement("thumb", existingFanartNode.TagValue));

                    foreach (var f in fanartFiles)
                    {
                        bool hasThumb = fanartElem.Elements("thumb")
                            .Any(e => string.Equals(e.Value?.Trim(), f, StringComparison.OrdinalIgnoreCase));
                        if (!hasThumb)
                        {
                            fanartElem.Add(new System.Xml.Linq.XElement("thumb", f));
                            addedCount++;
                        }
                    }

                    existingFanartNode.TagValue = fanartElem.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
                    existingFanartNode.IsXmlBlock = true;
                }
                catch
                {
                    // Leave existing custom fanart intact if unparseable
                }
            }
        }

        RawText = NfoParserService.SerializeToXml(Metadata);
        UpdateArtworkXmlLinkStatus();

        if (addedCount > 0)
        {
            IsModified = true;
            StatusMessage = $"Linked {addedCount} local artwork image(s) into NFO XML (<thumb> / <fanart>). Press Ctrl+S to save.";
        }
        else
        {
            StatusMessage = "All detected local artwork files are already linked in the XML.";
        }
    }

    private void ExecuteOpenArtworkFile(ArtworkItem? item)
    {
        string? target = item?.FilePath ?? ArtworkImagePath;
        if (string.IsNullOrEmpty(target) || !File.Exists(target)) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open image file:\n{ex.Message}", "Error Opening Image", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetField(ref _isUpdateAvailable, value);
    }

    public UpdateInfo? AvailableUpdate
    {
        get => _availableUpdate;
        set => SetField(ref _availableUpdate, value);
    }

    public bool IsDownloadingUpdate
    {
        get => _isDownloadingUpdate;
        set => SetField(ref _isDownloadingUpdate, value);
    }

    public double UpdateDownloadProgress
    {
        get => _updateDownloadProgress;
        set => SetField(ref _updateDownloadProgress, value);
    }

    public string UpdateStatusText
    {
        get => _updateStatusText;
        set => SetField(ref _updateStatusText, value);
    }

    public string AppVersionString => UpdateService.GetCurrentVersionString();

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand DownloadAndInstallUpdateCommand { get; }
    public ICommand DismissUpdateCommand { get; }

    public async Task CheckForUpdatesAsync(bool silent = false)
    {
        try
        {
            if (!silent)
            {
                StatusMessage = "Checking for updates on GitHub...";
            }

            var update = await _updateService.CheckForUpdatesAsync();
            if (update != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableUpdate = update;
                    IsUpdateAvailable = true;
                    UpdateStatusText = $"Version v{update.LatestVersion} available!";
                    StatusMessage = $"Update available: v{update.LatestVersion}";
                });
            }
            else if (!silent)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "You are on the latest version.";
                    MessageBox.Show(
                        $"You are running the latest version of Metadata Editor (v{AppVersionString}).",
                        "No Updates Available",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            }
        }
        catch
        {
            if (!silent)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "Could not check for updates.";
                });
            }
        }
    }

    public async Task ExecuteDownloadAndInstallUpdate()
    {
        if (AvailableUpdate == null) return;

        if (string.IsNullOrEmpty(AvailableUpdate.InstallerUrl))
        {
            ExecuteOpenInBrowser(AvailableUpdate.ReleasePageUrl);
            return;
        }

        try
        {
            IsDownloadingUpdate = true;
            UpdateStatusText = "Downloading update...";
            var progress = new Progress<double>(p =>
            {
                UpdateDownloadProgress = p;
                UpdateStatusText = $"Downloading update... {p:0}%";
            });

            string installerPath = await _updateService.DownloadInstallerAsync(AvailableUpdate.InstallerUrl, progress);
            UpdateStatusText = "Launching installer...";

            UpdateService.LaunchInstallerAndShutdown(installerPath);
        }
        catch (Exception ex)
        {
            IsDownloadingUpdate = false;
            UpdateStatusText = "Update failed.";
            MessageBox.Show($"Failed to download or run update:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ExecuteOpenSettings(int tabIndex = 0)
    {
        var vm = new SettingsViewModel(_settingsService, tabIndex);
        var win = new Views.SettingsWindow(vm)
        {
            Owner = Application.Current.MainWindow
        };
        if (win.ShowDialog() == true)
        {
            StatusMessage = "Settings updated successfully.";
        }
    }

    public string CurrentTheme => _settingsService.Settings.Theme;

    public void ExecuteChangeTheme(string? theme)
    {
        if (!string.IsNullOrEmpty(theme))
        {
            _settingsService.Settings.Theme = theme;
            _settingsService.Save();
            ThemeService.ApplyTheme(theme);
            OnPropertyChanged(nameof(CurrentTheme));
            StatusMessage = $"Theme switched to {theme}.";
        }
    }

    public void ExecuteExit()
    {
        if (IsModified && !PromptSaveBeforeAction())
        {
            return;
        }
        Application.Current.Shutdown();
    }

    public void ExecuteConvertType(object? param)
    {
        if (param is string str && Enum.TryParse<NfoFileType>(str, true, out var parsed))
        {
            SelectedFileType = parsed;
            StatusMessage = $"Converted metadata type to {parsed}.";
        }
        else if (param is NfoFileType nfoType)
        {
            SelectedFileType = nfoType;
            StatusMessage = $"Converted metadata type to {nfoType}.";
        }
    }

    public void ExecuteScanArtwork()
    {
        CheckArtwork();
        if (HasArtwork)
        {
            StatusMessage = $"Found {ArtworkItems.Count} artwork file(s) in directory ({string.Join(", ", ArtworkItems.Select(a => a.FileName))}).";
        }
        else
        {
            StatusMessage = "No local artwork images found in directory.";
        }
    }

    private bool PromptSaveBeforeAction()
    {
        var result = MessageBox.Show(
            $"You have unsaved changes in '{CurrentFileName}'.\nDo you want to save before continuing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ExecuteSave();
            return true;
        }
        if (result == MessageBoxResult.No)
        {
            return true;
        }
        return false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
