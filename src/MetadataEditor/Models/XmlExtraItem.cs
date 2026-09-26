using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetadataEditor.Models;

public class XmlExtraItem : INotifyPropertyChanged
{
    private string _tagName = string.Empty;
    private string _tagValue = string.Empty;
    private bool _isXmlBlock;

    public string TagName
    {
        get => _tagName;
        set => SetField(ref _tagName, value);
    }

    public string TagValue
    {
        get => _tagValue;
        set => SetField(ref _tagValue, value);
    }

    public bool IsXmlBlock
    {
        get => _isXmlBlock;
        set => SetField(ref _isXmlBlock, value);
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
