; -- MKSlideShop.iss --
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=MKSlideShop
AppVersion=1.0
WizardStyle=modern
DefaultDirName={autopf}\MKSlideShop
DefaultGroupName=MKSlideShop
UninstallDisplayIcon={app}\MKSlideShop.exe
Compression=lzma2
SolidCompression=yes
OutputDir=SetupFiles
OutputBaseFilename=MKSlideShop.1.0.0
SourceDir=.\bin\Release\net5.0-windows

[Files]
Source: "MKSlideShop.exe"; DestDir: "{app}"
Source: "MetadataExtractor.dll"; DestDir: "{app}"
Source: "Microsoft.WindowsAPICodePack.dll"; DestDir: "{app}"
Source: "Microsoft.WindowsAPICodePack.Shell.dll"; DestDir: "{app}"
Source: "Microsoft.WindowsAPICodePack.ShellExtensions.dll"; DestDir: "{app}"
Source: "MKSlideShop.dll"; DestDir: "{app}"
Source: "MKSlideShop.dll.config"; DestDir: "{app}"
Source: "MKSlideShop.runtimeconfig.json"; DestDir: "{app}"
Source: "NLog.config"; DestDir: "{app}"
Source: "NLog.dll"; DestDir: "{app}"
Source: "NLog.Extensions.Logging.dll"; DestDir: "{app}"
Source: "NLog.Web.AspNetCore.dll"; DestDir: "{app}"
Source: "SlideStore.dll"; DestDir: "{app}"
Source: "SlideWalker.dll"; DestDir: "{app}"
Source: "XmpCore.dll"; DestDir: "{app}"   
                                              
[Icons]
Name: "{group}\MKSlideShop"; Filename: "{app}\MKSlideShop.exe"
