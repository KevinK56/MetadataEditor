using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace MetadataEditor.Models;

public class FileListItem : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private string _fileName = string.Empty;
    private string _directoryName = string.Empty;
    private NfoFileType _fileType = NfoFileType.PlainText;
    private bool _isModified;

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetField(ref _filePath, value))
            {
                FileName = Path.GetFileName(value);
                DirectoryName = Path.GetFileName(Path.GetDirectoryName(value) ?? string.Empty);
            }
        }
    }

    public string FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public string DirectoryName
    {
        get => _directoryName;
        set => SetField(ref _directoryName, value);
    }

    public NfoFileType FileType
    {
        get => _fileType;
        set
        {
            if (SetField(ref _fileType, value))
            {
                OnPropertyChanged(nameof(IconText));
                OnPropertyChanged(nameof(TypeDisplay));
            }
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetField(ref _isModified, value);
    }

    public string IconText => FileType switch
    {
        NfoFileType.Movie => "🎬",
        NfoFileType.TvShow => "📺",
        NfoFileType.Episode => "🎞️",
        NfoFileType.MusicAlbum => "💿",
        NfoFileType.MusicArtist => "👤",
        NfoFileType.MusicVideo => "📽️",
        NfoFileType.GenericXml => "🏷️",
        _ => "📄"
    };

    public string TypeDisplay => FileType switch
    {
        NfoFileType.Movie => "Movie",
        NfoFileType.TvShow => "TV Show",
        NfoFileType.Episode => "Episode",
        NfoFileType.MusicAlbum => "Album",
        NfoFileType.MusicArtist => "Artist",
        NfoFileType.MusicVideo => "Music Video",
        NfoFileType.GenericXml => "XML",
        _ => "Text/ASCII"
    };

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
