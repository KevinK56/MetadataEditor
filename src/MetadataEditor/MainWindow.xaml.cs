using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MetadataEditor.Services;
using MetadataEditor.ViewModels;

namespace MetadataEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _lastSearchIndex = -1;

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_SETICON = 0x0080;
    private static readonly IntPtr ICON_SMALL = IntPtr.Zero;
    private static readonly IntPtr ICON_BIG = (IntPtr)1;

    private System.Drawing.Icon? _iconBig;
    private System.Drawing.Icon? _iconSmall;

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            var iconUri = new Uri("pack://application:,,,/MetadataEditor;component/Resources/app.ico", UriKind.Absolute);
            this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
        }
        catch
        {
            // Fallback
        }

        Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                ThemeService.ApplyTheme(vm.CurrentTheme);
            }
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        try
        {
            var iconUri = new Uri("pack://application:,,,/MetadataEditor;component/Resources/app.ico", UriKind.Absolute);
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            var streamInfo = Application.GetResourceStream(iconUri);
            if (streamInfo != null)
            {
                using var ms = new MemoryStream();
                streamInfo.Stream.CopyTo(ms);

                ms.Position = 0;
                _iconBig = new System.Drawing.Icon(ms, 32, 32);

                ms.Position = 0;
                _iconSmall = new System.Drawing.Icon(ms, 16, 16);

                SendMessage(helper.Handle, WM_SETICON, ICON_BIG, _iconBig.Handle);
                SendMessage(helper.Handle, WM_SETICON, ICON_SMALL, _iconSmall.Handle);
            }
        }
        catch
        {
            // Fallback
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _iconBig?.Dispose();
        _iconSmall?.Dispose();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string path = files[0];
                if (Directory.Exists(path))
                {
                    ViewModel.ExecuteOpenFolder(path);
                }
                else if (File.Exists(path))
                {
                    ViewModel.LoadFile(path);

                    // If file is inside a directory, also scan the directory to populate sidebar
                    string? dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        ViewModel.ExecuteOpenFolder(dir);
                        // Reselect current file
                        foreach (var item in ViewModel.AllFiles)
                        {
                            if (string.Equals(item.FilePath, path, StringComparison.OrdinalIgnoreCase))
                            {
                                ViewModel.SelectedFileItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void FindNext_Click(object sender, RoutedEventArgs e)
    {
        PerformFind();
    }

    private void FindTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PerformFind();
        }
    }

    private void PerformFind()
    {
        string query = FindTextBox.Text;
        if (string.IsNullOrEmpty(query)) return;

        string content = RawEditorTextBox.Text;
        int startIndex = _lastSearchIndex + 1;

        if (startIndex >= content.Length)
        {
            startIndex = 0;
        }

        int foundIndex = content.IndexOf(query, startIndex, StringComparison.OrdinalIgnoreCase);
        if (foundIndex < 0 && startIndex > 0)
        {
            // Wrap around
            foundIndex = content.IndexOf(query, 0, StringComparison.OrdinalIgnoreCase);
        }

        if (foundIndex >= 0)
        {
            _lastSearchIndex = foundIndex;
            RawEditorTextBox.Focus();
            RawEditorTextBox.Select(foundIndex, query.Length);
        }
        else
        {
            _lastSearchIndex = -1;
            MessageBox.Show($"'{query}' not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenFolderInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ViewModel.CurrentFilePath) && File.Exists(ViewModel.CurrentFilePath))
        {
            Process.Start("explorer.exe", $"/select,\"{ViewModel.CurrentFilePath}\"");
        }
        else if (!string.IsNullOrEmpty(ViewModel.CurrentFolderPath) && Directory.Exists(ViewModel.CurrentFolderPath))
        {
            Process.Start("explorer.exe", $"\"{ViewModel.CurrentFolderPath}\"");
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (ViewModel.IsModified)
        {
            var result = MessageBox.Show(
                $"You have unsaved changes in '{ViewModel.CurrentFileName}'.\nDo you want to save before closing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ViewModel.ExecuteSave();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
        base.OnClosing(e);
    }
}