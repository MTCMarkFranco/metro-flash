# Visual Metronome

A full-screen visual metronome application built with WPF for Windows 11. The application provides visual feedback through background color flashing to help musicians keep time.

## Features

- **Visual Beat Indication**: Background flashes between dark blue-gray and green to indicate beats
- **BPM Input**: Real-time tempo adjustment from 1-300 BPM
- **Full-Screen Mode**: Toggle full-screen with ESC key or F11
- **Keyboard Shortcuts**:
  - `ESC` or `F11`: Toggle full-screen mode
  - `Space`: Pause/resume metronome
  - `1-9`: Quick BPM presets (1=80 BPM, 2=100 BPM, 3=120 BPM, etc.)

## Usage

1. **Set BPM**: Enter your desired beats per minute in the BPM input field
2. **Full-Screen**: Click the "Toggle Fullscreen" button or press ESC
3. **Start/Stop**: The metronome starts automatically when you enter a valid BPM
4. **Pause**: Press Space to pause/resume

## Default Settings

- Default BPM: 120
- Default colors: Dark blue-gray background with green flash
- Maximized window on startup

## Building and Running

### Prerequisites
- .NET 9.0 SDK
- Windows 11 (or Windows 10 with WPF support)

### Build
```bash
dotnet build VisualMetronome.csproj
```

### Run
```bash
dotnet run
```

## Technical Details

- Built with WPF (.NET 9.0)
- Uses `DispatcherTimer` for precise timing
- Color animations handled by WPF's `ColorAnimation`
- Responsive UI that adapts to window size changes

## Controls

- **BPM Range**: 1-300 beats per minute
- **Flash Duration**: 100ms flash with smooth transitions
- **Timer Precision**: Millisecond-accurate timing based on BPM calculation

Perfect for musicians, music students, and anyone needing a reliable visual metronome!
