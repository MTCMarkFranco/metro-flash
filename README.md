# Visual Metronome

A full-screen visual metronome application built with WPF for Windows 11. The application provides visual feedback through background color flashing to help musicians keep time, with integrated Spotify playlist support for automatic BPM detection.

## Features

- **Visual Beat Indication**: Background flashes between dark blue-gray and green to indicate beats
- **BPM Input**: Real-time tempo adjustment from 1-300 BPM
- **Spotify Integration**: 
  - Connect to your Spotify account
  - Browse and select your playlists
  - Automatically fetch BPM data for each song
  - Navigate through playlist tracks with Previous/Next buttons
  - Metronome automatically adjusts to each song's tempo
- **Full-Screen Mode**: Toggle full-screen with ESC key or F11
- **Keyboard Shortcuts**:
  - `ESC` or `F11`: Toggle full-screen mode
  - `Space`: Pause/resume metronome
  - `Enter`: Apply BPM from text field

## Setup

### Prerequisites
- .NET 9.0 SDK
- Windows 11 (or Windows 10 with WPF support)
- Spotify account (for playlist integration)

### Spotify Setup (Required for playlist features)
**See [SPOTIFY_SETUP.md](SPOTIFY_SETUP.md) for detailed instructions.**

1. Create a Spotify Developer App at https://developer.spotify.com/dashboard
2. Set redirect URI to `http://localhost:5000/callback`
3. Copy your Client ID
4. Open `MainWindow.xaml.cs` and replace `YOUR_CLIENT_ID_HERE` with your Client ID

## Usage

### Manual Mode
1. **Set BPM**: Enter your desired beats per minute in the BPM input field
2. **Start**: Press Enter to apply the BPM and start the metronome
3. **Full-Screen**: Click the "Toggle Fullscreen" button or press ESC
4. **Pause**: Press Space to pause/resume

### Spotify Mode
1. **Connect**: Click "Connect to Spotify" button
2. **Authorize**: Log in via browser when prompted
3. **Select Playlist**: Choose a playlist from the dropdown menu
4. **Start**: Click "Start" to load the first song's BPM
5. **Navigate**: Use Previous/Next buttons to move through tracks
6. The metronome automatically applies each song's BPM

## Default Settings

- Default BPM: 120
- Default colors: Dark blue-gray background with green flash
- Maximized window on startup

## Building and Running

### Build
```bash
dotnet restore VisualMetronome.csproj
dotnet build VisualMetronome.csproj
```

### Run
```bash
dotnet run --project VisualMetronome.csproj
```

Or use the provided VS Code tasks:
- Build: Press `Ctrl+Shift+B` or run the "build" task
- Run: Run the "run" task from VS Code

## Technical Details

- Built with WPF (.NET 9.0)
- Uses `DispatcherTimer` for precise timing
- Spotify API integration via SpotifyAPI.Web and SpotifyAPI.Web.Auth packages
- OAuth 2.0 PKCE flow for secure authentication
- Color animations handled by WPF's `ColorAnimation`
- Responsive UI that adapts to window size changes

## Controls

- **BPM Range**: 1-300 beats per minute
- **Flash Duration**: 100ms flash with smooth transitions
- **Timer Precision**: Millisecond-accurate timing based on BPM calculation

Perfect for musicians, music students, and anyone needing a reliable visual metronome!
