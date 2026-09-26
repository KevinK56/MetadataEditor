using System.Windows;
using MetadataEditor.ViewModels;

namespace MetadataEditor.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSettings();
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

