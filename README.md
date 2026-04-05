MiniFlyout 🎵

MiniFlyout is a sleek, native-feeling media controller widget for Windows 10 and 11. Built with C# and .NET 10, it integrates seamlessly into your taskbar, providing beautiful and non-intrusive controls for your system's currently playing media (Spotify, YouTube, Windows Media Player, Apple Music, etc.).

✨ Features

Ambient Artwork Glow: Extracts the dominant colors from the currently playing album art and projects a beautiful, glowing acrylic blur behind the widget (similar to YouTube's ambient mode).

Smart Full-Screen Detection: Intelligently detects when you are playing a game or watching a full-screen video and politely hides itself so it never blocks your view.

Auto-Visibility: The widget automatically hides itself to save memory and CPU when no media is playing, and instantly reappears when you start a track.

System Tray Integration: Runs quietly in your system tray. Right-click the MiniFlyout icon to access settings or close the app.

Native Windows Aesthetics: Uses the native DWM API for hardware-accelerated rounded corners and seamless drop shadows.

Unobtrusive: Runs as a true background overlay (WS_EX_NOACTIVATE). Clicking the media controls will never steal focus from your active windows.

👥 How to Use (For Regular Users)

Once MiniFlyout is running, there is no setup required! It works completely automatically in the background:

Open your favorite media app or browser.

Start playing a song or video.

MiniFlyout will automatically appear on the bottom left of your taskbar, displaying the album artwork, track name, artist, and playback controls.

If you close your media player or go full-screen in a game, MiniFlyout will gracefully vanish until you need it again.

⚙️ Settings & Controls

Look for the MiniFlyout icon in your System Tray (the bottom right corner of your screen, near the Wi-Fi icon). Right-click it to:

Start with Windows: Check this box to make MiniFlyout launch automatically when you turn on your PC.

Exit MiniFlyout: Safely close the application.

🚀 Installation & Building (For Developers)

Prerequisites

Windows 10 or Windows 11

.NET 10 SDK

Build and Run locally

Clone the repository:

git clone [https://github.com/yourusername/media_controller.git](https://github.com/yourusername/media_controller.git)


Run the project:

dotnet run


📦 Building a Standalone Executable (To share with friends)

If you want to create a single .exe file that anyone can run without needing to install .NET, run this command:

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true


You will find your shareable MiniFlyout.exe in bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\.

⚠️ Antivirus / SmartScreen Notice: > Because this is a free, open-source project, the executable is not signed with an expensive Extended Validation (EV) developer certificate. When running the compiled .exe for the first time, Windows SmartScreen may show a "Windows protected your PC" warning. Simply click More info -> Run anyway.

🏗️ Architecture

The project follows a pragmatic Clean Architecture approach to separate UI from Windows OS dependencies:

Core: Contains domain models (TrackInfo) and interfaces (IMediaService).

Infrastructure: Handles the heavy lifting with the Windows OS. Contains P/Invoke wrappers, Windows Media Transport Controls (WindowsMediaService), and registry management (AutorunManager).

UI: Contains the WinForms visual elements, native window styling (WindowEffects), dynamic polling, image color extraction (ImageUtils), the System Tray module (TrayManager), and custom Fluent buttons (MainForm, StyledButton).

🛠️ Built With

C# / WinForms

.NET 10.0

Windows 10/11 SDK (net10.0-windows10.0.19041.0)