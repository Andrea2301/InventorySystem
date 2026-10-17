; Script de Inno Setup para InventorySystem (WPF Desktop Companion)
; Requiere Inno Setup (descarga gratuita desde https://jrsoftware.org/isdl.php)

#define MyAppName "Inventory System Desktop"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Inventory System Cloud"
#define MyAppURL "https://github.com/Andrea2301/InventorySystem"
#define MyAppExeName "InventorySystem.exe"

[Setup]
; Identificador de la aplicación
AppId={{9B78B452-9428-4E4D-A2C3-2D3B88A0194B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Icono del instalador
SetupIconFile=..\Assets\Img\modelado_multi.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\bin\Installer
OutputBaseFilename=InventorySystem-Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Publica la salida de compilación win-x64
Source: "..\bin\Release\net8.0-windows\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Filename: "{app}\{#MyAppExeName}"; Flags: nowait postinstall skipifsilent
