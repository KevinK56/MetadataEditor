using System.IO;
using System.Windows;

namespace MetadataEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow();
        mainWindow.Show();

        if (e.Args.Length > 0)
        {
            string path = e.Args[0];
            if (File.Exists(path))
            {
                mainWindow.ViewModel.LoadFile(path);
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    mainWindow.ViewModel.ExecuteOpenFolder(dir);
                    foreach (var item in mainWindow.ViewModel.AllFiles)
                    {
                        if (string.Equals(item.FilePath, path, StringComparison.OrdinalIgnoreCase))
                        {
                            mainWindow.ViewModel.SelectedFileItem = item;
                            break;
                        }
                    }
                }
            }
            else if (Directory.Exists(path))
            {
                mainWindow.ViewModel.ExecuteOpenFolder(path);
            }
        }
    }
}
