#define MyAppName "TrayProxy"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "N0rmalUser"
#define MyAppExeName "TrayProxy.exe"

[Setup]
AppId={{B56C12E1-9C0D-4F4D-A734-26D6A6E89B11}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\TrayProxy
PrivilegesRequired=admin
DefaultGroupName=TrayProxy
DisableProgramGroupPage=yes
OutputDir=installer-output
OutputBaseFilename=Setup
SetupIconFile="C:\Users\normal\Desktop\TrayProxy\TrayProxy.ico"
UninstallDisplayIcon={app}\TrayProxy.exe
Uninstallable=yes
CloseApplications=yes
RestartApplications=no
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[CustomMessages]
russian.StartupTask=Запускать TrayProxy при входе в Windows
english.StartupTask=Start TrayProxy when I sign in to Windows

russian.LaunchApp=Запустить TrayProxy
english.LaunchApp=Launch TrayProxy

[Files]
Source: "C:\Users\normal\Desktop\TrayProxy\bin\Release\net8.0-windows\win-x64\publish\TrayProxy.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\TrayProxy"; Filename: "{app}\TrayProxy.exe"; IconFilename: "{app}\TrayProxy.exe"
Name: "{group}\Удалить TrayProxy"; Filename: "{uninstallexe}"; IconFilename: "{app}\TrayProxy.exe"
Name: "{userstartup}\TrayProxy"; Filename: "{app}\TrayProxy.exe"; IconFilename: "{app}\TrayProxy.exe"; Tasks: startup

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "{cm:StartupTask}"; Flags: checkedonce

[Run]
Filename: "{app}\TrayProxy.exe"; Description: "{cm:LaunchApp}"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/IM TrayProxy.exe /T /F"; Flags: runhidden