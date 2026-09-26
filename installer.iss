; Inno Setup Script for Metadata Editor Freeware Distribution
; Download free Inno Setup compiler from: https://jrsoftware.org/isdl.php

#define MyAppName "Metadata Editor"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Freeware"
#define MyAppURL "https://github.com"
#define MyAppExeName "MetadataEditor.exe"

[Setup]
AppId={{D37F279A-875D-4E7B-8CA9-8A1A24FB65C2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=release\v1.0.0\installer
OutputBaseFilename=MetadataEditor_Setup_v1.0.0
SetupIconFile=src\MetadataEditor\Resources\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "associate_nfo"; Description: "Associate with .nfo files (Open with Metadata Editor)"; GroupDescription: "File Associations:"

[Files]
Source: "release\v1.0.0\single-file\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\MetadataEditor\Resources\app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

[Registry]
; Associate .nfo extension if task selected
Root: HKA; Subkey: "Software\Classes\.nfo\OpenWithProgids"; ValueType: string; ValueName: "MetadataEditor.nfo"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate_nfo
Root: HKA; Subkey: "Software\Classes\MetadataEditor.nfo"; ValueType: string; ValueName: ""; ValueData: "NFO Metadata File"; Flags: uninsdeletekey; Tasks: associate_nfo
Root: HKA; Subkey: "Software\Classes\MetadataEditor.nfo\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\app.ico,0"; Tasks: associate_nfo
Root: HKA; Subkey: "Software\Classes\MetadataEditor.nfo\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate_nfo
Root: HKA; Subkey: "Software\Classes\MetadataEditor.nfo\shell\Edit with Metadata Editor\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associate_nfo

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
