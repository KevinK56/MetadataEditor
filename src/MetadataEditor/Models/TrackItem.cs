using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetadataEditor.Models;

public class TrackItem : INotifyPropertyChanged
{
    private string _position = "1";
    private string _title = string.Empty;
    private string _duration = string.Empty;

    public string Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Duration
    {
        get => _duration;
        set => SetField(ref _duration, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
