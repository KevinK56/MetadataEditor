using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetadataEditor.Models;

public class NfoMetadata : INotifyPropertyChanged
{
    private NfoFileType _fileType = NfoFileType.Movie;
    private string _rootElementName = "movie";

    // Common Core
    private string _title = string.Empty;
    private string _originalTitle = string.Empty;
    private string _sortTitle = string.Empty;
    private string _year = string.Empty;
    private string _premiered = string.Empty;
    private string _released = string.Empty;
    private string _plot = string.Empty;
    private string _outline = string.Empty;
    private string _tagline = string.Empty;
    private string _runtime = string.Empty;
    private string _rating = string.Empty;
    private string _votes = string.Empty;
    private string _mpaa = string.Empty;
    private string _trailer = string.Empty;

    // Identifiers
    private string _id = string.Empty;
    private string _imdbId = string.Empty;
    private string _tmdbId = string.Empty;
    private string _tvdbId = string.Empty;

    // Movie Specific
    private string _setName = string.Empty;
    private string _setOverview = string.Empty;
    private string _top250 = string.Empty;

    // TV Show / Episode Specific
    private string _status = "Continuing";
    private string _season = "1";
    private string _episode = "1";
    private string _showTitle = string.Empty;
    private string _aired = string.Empty;

    // Music Specific
    private string _artist = string.Empty;
    private string _album = string.Empty;
    private string _label = string.Empty;
    private string _albumType = "Album";
    private string _review = string.Empty;
    private string _biography = string.Empty;
    private string _artistType = "Person";
    private string _gender = string.Empty;
    private string _born = string.Empty;
    private string _formed = string.Empty;
    private string _disbanded = string.Empty;
    private string _died = string.Empty;

    // Raw text fallback
    private string _rawText = string.Empty;

    public NfoMetadata()
    {
        Genres.CollectionChanged += OnCollectionChanged;
        Studios.CollectionChanged += OnCollectionChanged;
        Countries.CollectionChanged += OnCollectionChanged;
        Directors.CollectionChanged += OnCollectionChanged;
        Writers.CollectionChanged += OnCollectionChanged;
        Actors.CollectionChanged += OnCollectionChanged;
        Tracks.CollectionChanged += OnCollectionChanged;
        ExtraNodes.CollectionChanged += OnCollectionChanged;
    }

    public NfoFileType FileType
    {
        get => _fileType;
        set => SetField(ref _fileType, value);
    }

    public string RootElementName
    {
        get => _rootElementName;
        set => SetField(ref _rootElementName, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string OriginalTitle
    {
        get => _originalTitle;
        set => SetField(ref _originalTitle, value);
    }

    public string SortTitle
    {
        get => _sortTitle;
        set => SetField(ref _sortTitle, value);
    }

    public string Year
    {
        get => _year;
        set => SetField(ref _year, value);
    }

    public string Premiered
    {
        get => _premiered;
        set => SetField(ref _premiered, value);
    }

    public string Released
    {
        get => _released;
        set => SetField(ref _released, value);
    }

    public string Plot
    {
        get => _plot;
        set => SetField(ref _plot, value);
    }

    public string Outline
    {
        get => _outline;
        set => SetField(ref _outline, value);
    }

    public string Tagline
    {
        get => _tagline;
        set => SetField(ref _tagline, value);
    }

    public string Runtime
    {
        get => _runtime;
        set => SetField(ref _runtime, value);
    }

    public string Rating
    {
        get => _rating;
        set => SetField(ref _rating, value);
    }

    public string Votes
    {
        get => _votes;
        set => SetField(ref _votes, value);
    }

    public string Mpaa
    {
        get => _mpaa;
        set => SetField(ref _mpaa, value);
    }

    public string Trailer
    {
        get => _trailer;
        set => SetField(ref _trailer, value);
    }

    public string Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string ImdbId
    {
        get => _imdbId;
        set => SetField(ref _imdbId, value);
    }

    public string TmdbId
    {
        get => _tmdbId;
        set => SetField(ref _tmdbId, value);
    }

    public string TvdbId
    {
        get => _tvdbId;
        set => SetField(ref _tvdbId, value);
    }

    public string SetName
    {
        get => _setName;
        set => SetField(ref _setName, value);
    }

    public string SetOverview
    {
        get => _setOverview;
        set => SetField(ref _setOverview, value);
    }

    public string Top250
    {
        get => _top250;
        set => SetField(ref _top250, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string Season
    {
        get => _season;
        set => SetField(ref _season, value);
    }

    public string Episode
    {
        get => _episode;
        set => SetField(ref _episode, value);
    }

    public string ShowTitle
    {
        get => _showTitle;
        set => SetField(ref _showTitle, value);
    }

    public string Aired
    {
        get => _aired;
        set => SetField(ref _aired, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetField(ref _artist, value);
    }

    public string Album
    {
        get => _album;
        set => SetField(ref _album, value);
    }

    public string Label
    {
        get => _label;
        set => SetField(ref _label, value);
    }

    public string AlbumType
    {
        get => _albumType;
        set => SetField(ref _albumType, value);
    }

    public string Review
    {
        get => _review;
        set => SetField(ref _review, value);
    }

    public string Biography
    {
        get => _biography;
        set => SetField(ref _biography, value);
    }

    public string ArtistType
    {
        get => _artistType;
        set => SetField(ref _artistType, value);
    }

    public string Gender
    {
        get => _gender;
        set => SetField(ref _gender, value);
    }

    public string Born
    {
        get => _born;
        set => SetField(ref _born, value);
    }

    public string Formed
    {
        get => _formed;
        set => SetField(ref _formed, value);
    }

    public string Disbanded
    {
        get => _disbanded;
        set => SetField(ref _disbanded, value);
    }

    public string Died
    {
        get => _died;
        set => SetField(ref _died, value);
    }

    public string RawText
    {
        get => _rawText;
        set => SetField(ref _rawText, value);
    }

    public ObservableCollection<string> Genres { get; } = new();
    public ObservableCollection<string> Studios { get; } = new();
    public ObservableCollection<string> Countries { get; } = new();
    public ObservableCollection<string> Directors { get; } = new();
    public ObservableCollection<string> Writers { get; } = new();
    public ObservableCollection<ActorItem> Actors { get; } = new();
    public ObservableCollection<TrackItem> Tracks { get; } = new();
    public ObservableCollection<XmlExtraItem> ExtraNodes { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? DataModified;

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is INotifyPropertyChanged notify)
                {
                    notify.PropertyChanged += (s, args) => DataModified?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        DataModified?.Invoke(this, EventArgs.Empty);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        DataModified?.Invoke(this, EventArgs.Empty);
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
