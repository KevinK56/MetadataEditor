using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetadataEditor.Models;

public class ActorItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _role = string.Empty;
    private int _order;
    private string _thumb = string.Empty;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Role
    {
        get => _role;
        set => SetField(ref _role, value);
    }

    public int Order
    {
        get => _order;
        set => SetField(ref _order, value);
    }

    public string Thumb
    {
        get => _thumb;
        set => SetField(ref _thumb, value);
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
