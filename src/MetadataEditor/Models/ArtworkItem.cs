using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace MetadataEditor.Models;

public class ArtworkItem : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private string _fileName = string.Empty;
    private string _artworkType = "Poster";
    private string _aspect = "poster";
    private string _dimensions = string.Empty;
    private string _fileSizeFormatted = string.Empty;
    private bool _isLinkedInXml;

    public string FilePath
    {
        get => _filePath;
        set => SetField(ref _filePath, value);
    }

    public string FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public string ArtworkType
    {
        get => _artworkType;
        set => SetField(ref _artworkType, value);
    }

    public string Aspect
    {
        get => _aspect;
        set => SetField(ref _aspect, value);
    }

    public string Dimensions
    {
        get => _dimensions;
        set => SetField(ref _dimensions, value);
    }

    public string FileSizeFormatted
    {
        get => _fileSizeFormatted;
        set => SetField(ref _fileSizeFormatted, value);
    }

    public bool IsLinkedInXml
    {
        get => _isLinkedInXml;
        set
        {
            if (SetField(ref _isLinkedInXml, value))
            {
                OnPropertyChanged(nameof(XmlLinkStatusText));
            }
        }
    }

    public string XmlLinkStatusText => IsLinkedInXml ? "✓ Linked in XML" : "Local File (Unlinked)";

    public BitmapImage? ThumbnailSource
    {
        get
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(FilePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
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

