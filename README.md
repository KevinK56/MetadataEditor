# 🎬 Metadata Editor

A powerful, universal Windows desktop application to view, edit, convert, and save `.nfo` metadata files for **Movies**, **TV Shows**, **Episodes**, **Music Albums & Artists**, and **Scene ASCII / Plain Text** releases.

---

## 📁 Repository Structure

```
c:\Dev\MetadataEditor\
├── MetadataEditor.sln                    # Standard Visual Studio Solution (at root, above src)
├── MetadataEditor.slnx                   # Modern VS 2022 / .NET 10 Solution
│
├── src/                                  # All source code
│   ├── MetadataEditor/                   # Main WPF Application Project
│   │   ├── MetadataEditor.csproj
│   │   ├── app.manifest                  # asInvoker & PerMonitorV2 High-DPI
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── MainWindow.xaml / .cs
│   │   ├── Models/                       # Domain models & extra XML preservation
│   │   ├── Services/                     # NFO parser, serializer & encoding detector
│   │   ├── ViewModels/                   # MVVM ViewModel & commands
│   │   ├── Views/                        # Value converters
│   │   ├── Styles/                       # Dark theme XAML styles
│   │   └── Resources/                    # Multi-resolution app.ico
│   └── tests/                            # Automated test suite
│       └── MetadataEditor.Tests/
│           ├── MetadataEditor.Tests.csproj
│           └── NfoParsingTests.cs
│
├── release/                              # Release distributions organized by build/version
│   └── v1.0.0/
│       ├── single-file/                  # Standalone self-contained MetadataEditor.exe (zero dependencies)
│       └── portable/                     # Lightweight framework-dependent build (~200 KB)
│
├── Samples/                              # Sample test NFO files (Movie, TV, Episode, Music, Scene ASCII)
├── build-single-file.bat                 # 1-click batch script to produce standalone release
├── build-portable.bat                    # 1-click batch script to produce lightweight release
├── installer.iss                         # Inno Setup freeware installer script
├── LICENSE                               # Permissive MIT Freeware License
└── README.md                             # Documentation
```

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

- **Local Artwork Detection**:
  - Automatically discovers and displays posters/cover art (`poster.jpg`, `folder.jpg`, `cover.jpg`, `<name>-poster.jpg`) alongside metadata.

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

## 🛠️ Building from Source

To build or publish using the .NET CLI:

```bash
# Build Solution (contains both app and tests)
dotnet build MetadataEditor.sln

# Run automated tests
dotnet test MetadataEditor.sln

# Or run the convenient build batch script:
build-single-file.bat
```

---

## 📦 Freeware Distribution & Installer

To distribute this application as professional freeware:

1. **Standalone Portable Zip**:
   - Simply zip `release\v1.0.0\single-file\MetadataEditor.exe`, `README.md`, and `LICENSE`. Users can extract and run it immediately from a USB flash drive or any folder.

2. **Inno Setup Installer (`installer.iss`)**:
   - Install the free [Inno Setup Compiler](https://jrsoftware.org/isdl.php).
   - Right-click `installer.iss` and click **Compile**.
   - This creates `release\v1.0.0\installer\MetadataEditor_Setup_v1.0.0.exe` with:
     - Windows Start Menu & Desktop shortcuts
     - Context menu integration ("Edit with Metadata Editor" when right-clicking `.nfo` files)
     - Clean Windows Add/Remove Programs uninstaller

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

## 📄 License

This software is released under the **MIT License** — 100% free to use, modify, and distribute as freeware.
