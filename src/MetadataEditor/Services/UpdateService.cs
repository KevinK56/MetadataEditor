using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MetadataEditor.Services;

public record UpdateInfo(
    string CurrentVersion,
    string LatestVersion,
    string ReleaseTitle,
    string ReleaseNotes,
    string ReleasePageUrl,
    string? InstallerUrl,
    string? PortableZipUrl,
    long? InstallerSize,
    DateTime? PublishedAt
);

public class UpdateService
{
    public const string GitHubOwner = "KevinK56";
    public const string GitHubRepo = "MetadataEditor";
    private static readonly HttpClient _httpClient = new HttpClient();

    static UpdateService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MetadataEditor-App", "1.0"));
        _httpClient.Timeout = TimeSpan.FromSeconds(20);
    }

    /// <summary>
    /// Gets the current running application version.
    /// </summary>
    public static string GetCurrentVersionString()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            return $"{version.Major}.{version.Minor:D2}.{version.Build:D2}.{version.Revision}";
        }
        return "1.0.0.0";
    }

    /// <summary>
    /// Checks GitHub Releases for a newer version than the current running assembly.
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken ct = default)
    {
        try
        {
            string url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            using var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
            string name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            string body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
            string htmlUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? "" : "";

            DateTime? publishedAt = null;
            if (root.TryGetProperty("published_at", out var pubProp) && pubProp.TryGetDateTime(out var dt))
            {
                publishedAt = dt;
            }

            string cleanTag = tagName.TrimStart('v', 'V').Trim();
            if (string.IsNullOrEmpty(cleanTag))
            {
                return null;
            }

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
            if (!TryParseVersion(cleanTag, out var remoteVersion))
            {
                return null;
            }

            if (remoteVersion <= currentVersion)
            {
                // Already up to date
                return null;
            }

            string? installerUrl = null;
            string? portableZipUrl = null;
            long? installerSize = null;

            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    string assetName = asset.TryGetProperty("name", out var aName) ? aName.GetString() ?? "" : "";
                    string downloadUrl = asset.TryGetProperty("browser_download_url", out var aUrl) ? aUrl.GetString() ?? "" : "";
                    long size = asset.TryGetProperty("size", out var aSize) ? aSize.GetInt64() : 0;

                    if (assetName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        if (assetName.Contains("Setup", StringComparison.OrdinalIgnoreCase) || installerUrl == null)
                        {
                            installerUrl = downloadUrl;
                            installerSize = size;
                        }
                    }
                    else if (assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        portableZipUrl = downloadUrl;
                    }
                }
            }

            return new UpdateInfo(
                CurrentVersion: GetCurrentVersionString(),
                LatestVersion: cleanTag,
                ReleaseTitle: string.IsNullOrWhiteSpace(name) ? $"Release {tagName}" : name,
                ReleaseNotes: body,
                ReleasePageUrl: htmlUrl,
                InstallerUrl: installerUrl,
                PortableZipUrl: portableZipUrl,
                InstallerSize: installerSize,
                PublishedAt: publishedAt
            );
        }
        catch
        {
            // Network failure or rate-limited; gracefully proceed without breaking app flow
            return null;
        }
    }

    /// <summary>
    /// Downloads the setup installer to a temporary location with progress reporting.
    /// </summary>
    public async Task<string> DownloadInstallerAsync(string downloadUrl, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "MetadataEditor_Updates");
        Directory.CreateDirectory(tempDir);
        string fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "MetadataEditor-Setup.exe";
        }
        string targetPath = Path.Combine(tempDir, fileName);

        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;

        using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        byte[] buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
            totalRead += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                double percentage = (double)totalRead / totalBytes.Value * 100.0;
                progress?.Report(percentage);
            }
        }

        return targetPath;
    }

    /// <summary>
    /// Launches the installer executable and terminates the running instance so files can be updated.
    /// </summary>
    public static void LaunchInstallerAndShutdown(string installerPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            };
            Process.Start(psi);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to launch installer:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static bool TryParseVersion(string versionStr, out Version version)
    {
        // Supports formats: 2026.09.26.4, 2026.9.26.4, 1.0.0, 1.0.0.4
        // If segments have leading zeros or 3 parts, normalize to 4 parts
        string[] parts = versionStr.Split(new[] { '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            int major = int.TryParse(parts[0], out int v0) ? v0 : 1;
            int minor = parts.Length > 1 && int.TryParse(parts[1], out int v1) ? v1 : 0;
            int build = parts.Length > 2 && int.TryParse(parts[2], out int v2) ? v2 : 0;
            int rev = parts.Length > 3 && int.TryParse(parts[3], out int v3) ? v3 : 0;

            version = new Version(major, minor, build, rev);
            return true;
        }

        return Version.TryParse(versionStr, out version!);
    }
}
