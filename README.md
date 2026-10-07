# 🎬 Metadata Editor

A powerful, universal Windows desktop application to view, edit, convert, and save `.nfo` metadata files for **Movies**, **TV Shows**, **Episodes**, **Music Albums & Artists**, and **Scene ASCII / Plain Text** releases.

---

## Preview

<img width="1915" height="1032" alt="image" src="https://github.com/user-attachments/assets/a712a473-06a1-4b1e-a653-1628fdf2e5aa" />

<img width="1918" height="1033" alt="image" src="https://github.com/user-attachments/assets/dcd2c29b-0c6d-4e2a-af57-d8c3f8de47aa" />


---

## ✨ Features

- **Universal NFO Support**:
  - **Movies**: Title, Original Title, Sort Title, Year, Premiered/Released Date, Runtime, Rating & Votes, Content Rating (MPAA/Certification), Tagline, Plot, Outline, Movie Set/Collection, Genres, Studios, Directors, Writers, Cast & Characters, Online IDs (IMDb, TMDb, TVDb), Trailer URL.
  - **TV Shows**: Show Name, Original Title, Series Status (Continuing/Ended), Year, Premiered, Seasons count, Episodes count, Plot, Genres, Studios/Networks, Actors, IDs.
  - **TV Episodes**: Series Title, Episode Title, Season Number, Episode Number, Aired Date, Rating & Votes, Director, Writer, Plot summary.
  - **Music Albums**: Album Title, Artist Name, Release Date, Label, Album Type, Genres, Rating, Review, and an interactive **Tracklist Editor** (Track #, Title, Duration).
  - **Music Artists**: Artist Name, Biography, Formation/Birth Date, Disbanded Date, Moods, and Styles.
  - **Generic / Custom XML**: Any valid XML file can be viewed and edited without data loss.
  - **Scene / Plain Text ASCII Art**: Automatically detects DOS Code Page 437 (`CP437`) and ANSI art. Renders crisp monospace ASCII logos without corrupting characters or throwing XML errors!
  - **One-Click Format Conversion**: Easily convert plain text NFOs to structured Movie/TV/Music XML, or switch formats anytime.

- **100% Lossless XML Preservation**:
  - Unknown or custom XML elements (e.g. `<fileinfo>`, `<streamdetails>`, `<art>`, `<resume>`) are preserved in an editable **Extra / Custom XML Tags** section. Nothing is ever stripped or lost!

- **Dual-View Editing with Bidirectional Sync**:
  - **📋 Metadata Form Tab**: Rich visual form with tags, tables, and quick action buttons.
  - **📝 Raw Text / XML Editor Tab**: Monospace code view with XML formatting/indentation, word wrap toggle, and search/find.
  - Changes in the visual form automatically update the raw XML; changes in the raw XML update the visual form!

- **Folder Scanner & Media Browser**:
  - Open an entire media directory (or drag & drop a folder) to scan and list all `.nfo` files with instant search and type filters (*All*, *Movies*, *TV Shows*, *Episodes*, *Music*, *Text*).

- **Local Artwork Gallery & NFO XML Linking**:
  - Automatically discovers and displays all local media artwork (`poster.jpg`, `fanart.jpg`, `clearlogo.png`, `landscape.jpg`, `banner.jpg`, `clearart.png`, `discart.png`, `keyart.jpg`, and `<name>-<type>.*`) with dimensions and file size, plus one-click linking into NFO XML (`<thumb aspect="...">` and `<fanart>`).

- **Encoding Support**:
  - Auto-detects UTF-8, UTF-8 with BOM, DOS CP437, Windows-1252 (ANSI), and UTF-16. Preserves original encoding on save.

- **Windows Integration**:
  - Full Drag & Drop support (drag files or folders directly into the window).
  - Command-line support: double-click `.nfo` files or pass file paths to `MetadataEditor.exe`.
  - High-DPI Per-Monitor V2 aware (crisp on 4K / scaling).
  - `asInvoker` security manifest to avoid UAC elevation popups.

---

## 🚀 Running the Application

### Option A: Standalone Single-File Executable (Recommended for Distribution)
The single-file executable contains the embedded .NET desktop runtime, meaning **it can run directly on any Windows 10/11 64-bit PC without installing .NET or any dependencies**:

```bash
release\v1.0.0\single-file\MetadataEditor.exe
```

### Option B: Portable Framework-Dependent Executable
A tiny (~200 KB) distribution for machines with .NET 8 Desktop Runtime installed:

```bash
release\v1.0.0\portable\MetadataEditor.exe
```

---

### 🛡️ Browser Download & Windows SmartScreen Notice

Because **Metadata Editor** is a free, newly published open-source project without a paid commercial EV Code Signing certificate, your browser (Edge, Chrome) or Windows SmartScreen may display an initial safety prompt on newly compiled releases:

#### 1. In Chrome / Edge (Browser Download Bar)
- Message: *"This file is not commonly downloaded and may be dangerous"* or *"MetadataEditor-Setup... was reported as unsafe"*.
- **Solution:** Click the three dots `...` (or dropdown arrow) next to the download > Click **Keep** > Click **Keep anyway**.

#### 2. In Windows SmartScreen (When launching the installer)
- Message: *"Windows protected your PC — Microsoft Defender SmartScreen prevented an unrecognized app from starting"*.
- **Solution:** Click **More info** (under the text) > Click the **Run anyway** button that appears.

> [!NOTE]
> This is standard Windows behavior for all newly released open-source software before it accumulates download volume. The application is 100% open source, virus-free, and you can inspect the full source code directly in this repository. Alternatively, you can download the **Portable ZIP archive**, which avoids the installer entirely.

---

## 🔄 Automatic Updates & GitHub Releases Hosting

### Automatic Background Updates
- When **Metadata Editor** starts, it seamlessly checks GitHub Releases (`https://api.github.com/repos/KevinK56/MetadataEditor/releases/latest`) in the background.
- If a newer version is found, an update banner appears with version information, release notes link, and a **"⬇️ Download & Install"** button.
- You can also manually trigger an update check at any time by clicking the version badge (e.g. `🔄 v2026.09.26.2`) in the bottom-right status bar.
- Clicking install downloads the setup package with a progress bar and automatically launches the installer to upgrade seamlessly in place.

### Continuous Integration & Release Automation (GitHub Actions)
- Every check-in / push against `master` automatically triggers the GitHub Actions workflow [`.github/workflows/build-and-release.yml`](.github/workflows/build-and-release.yml).
- **Automated Versioning Scheme**:
  - The build number is dynamically calculated from the build date and git commit count: `YYYY.MM.DD.<CommitCount>` (e.g. `2026.09.26.3` / Tag `v2026.09.26.3`).
  - Sets the Windows `AssemblyVersion`, `FileVersion`, and `InformationalVersion`.
- **Automated Assets Published to GitHub Releases**:
  1. `MetadataEditor-Setup-v<Version>.exe`: Inno Setup installer with automatic application closing/restarting for seamless updates.
  2. `MetadataEditor-Portable-v<Version>.zip`: Portable zero-install zip bundle.
  3. `MetadataEditor.exe`: Standalone self-contained single-file binary.

---

## 🛠️ Building from Source

To build or publish using the .NET CLI:

```bash
# Build Solution (contains both app and tests)
dotnet build MetadataEditor.sln

# Run automated tests
dotnet test MetadataEditor.sln

# Or run the convenient build batch scripts (uses dynamic date + commit count versioning):
build-single-file.bat
build-portable.bat
```

---

## 📦 Freeware Distribution & Installer

To distribute this application as professional freeware:

1. **GitHub Releases Hosting (Recommended)**:
   - Push your changes to `master` on GitHub.
   - The CI/CD pipeline builds and publishes portable ZIPs and installers automatically to `https://github.com/KevinK56/MetadataEditor/releases`.

2. **Standalone Portable Zip**:
   - Simply download or zip `MetadataEditor.exe`, `README.md`, and `LICENSE`. Users can extract and run it immediately from a USB flash drive or any folder.

3. **Inno Setup Installer (`installer.iss`)**:
   - Install the free [Inno Setup Compiler](https://jrsoftware.org/isdl.php).
   - Compile using `installer.iss` or run:
     ```cmd
     "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" /DMyAppVersion="2026.09.26.1" installer.iss
     ```
   - This creates `MetadataEditor-Setup-v<Version>.exe` with:
     - Windows Start Menu & Desktop shortcuts
     - Context menu integration ("Edit with Metadata Editor" when right-clicking `.nfo` files)
     - Clean Windows Add/Remove Programs uninstaller
     - Safe update handling (`CloseApplications=yes`, `RestartApplications=yes`)

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
| :--- | :--- |
| `Ctrl + O` | Open File |
| `Ctrl + S` | Save |
| `Ctrl + Shift + S` | Save As |
| `Ctrl + N` | New NFO Template |
| `F5` | Reload Current File |
| `Enter` (in Find box) | Find Next |

---

## 💖 Support the Developer

If you find **Metadata Editor** helpful for organizing your home theater, Plex, Kodi, or Jellyfin library and would like to support ongoing development, maintenance, and new features, any contribution is warmly appreciated!

- **GitHub Sponsors:** Sponsor directly on GitHub via the **💖 Sponsor** button at the top of the repository.
- **Support / Donate:** You can also sponsor via PayPal.

Thank you for supporting free and open-source software!

---

## 📄 License

This software is licensed under the **GNU General Public License v3.0 (GPL-3.0)** — see the [LICENSE](LICENSE) file for details.
