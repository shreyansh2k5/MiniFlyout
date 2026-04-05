MiniFlyout 🎵

MiniFlyout is a sleek, native-feeling media controller widget for Windows 11. Built with C# and .NET 10, it integrates seamlessly into your taskbar, providing beautiful and non-intrusive controls for your system's currently playing media (Spotify, YouTube, Windows Media Player, etc.).

✨ Features

Native Windows 11 Aesthetics: Uses DWM API for hardware-accelerated rounded corners and user32 for the beautiful Acrylic blur (glass effect) background.

Taskbar Integration: Sits flush on the bottom-left of your screen, perfectly matching the height of the Windows 11 taskbar.

Live Media Sync: Uses Windows Runtime APIs to dynamically fetch the currently playing Track Name, Artist, and Album Art thumbnail.

Fluent Icons: Implements native Segoe Fluent Icons for crisp, recognizable media controls that react with solid backgrounds on hover.

Unobtrusive: Runs as a background overlay (WS_EX_NOACTIVATE) so clicking the media controls won't steal focus from your active game or browser window.

Clean Architecture: Codebase is separated into Core, Infrastructure, and UI layers for high maintainability.

🚀 Getting Started

Prerequisites

Windows 10 or Windows 11

.NET 10 SDK

Installation & Running

Clone the repository:

git clone [https://github.com/yourusername/media_controller.git](https://github.com/yourusername/media_controller.git)
cd media_controller/MiniFlyout


Build and run the project using the .NET CLI:

dotnet run


(Note: Windows Defender might flag the application locally during development due to the use of global keystroke injection (user32.dll) to control media. You may need to add a folder exclusion in Windows Security).

🏗️ Architecture

The project follows a pragmatic Clean Architecture approach:

Core: Contains domain models (TrackInfo) and interfaces (IMediaService).

Infrastructure: Handles the heavy lifting with the Windows OS. Contains P/Invoke wrappers (NativeMethods) and the implementation for Windows Media Transport Controls (WindowsMediaService).

UI: Contains the WinForms visual elements, native window styling, dynamic polling, and custom Fluent buttons (MainForm, StyledButton).

🛠️ Built With

C# / WinForms

.NET 10.0

Windows 10/11 SDK (net10.0-windows10.0.19041.0)
