<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Visual Metronome WPF Application

This is a WPF application that creates a visual metronome for Windows 11. The application features:

- Full-screen capability
- BPM input field for tempo control
- Background flashing animation that alternates between default background color and green
- Real-time tempo adjustment when BPM value changes

## Technical Guidelines

- Use WPF animations and timers for smooth flashing effects
- Implement responsive UI that works in both windowed and full-screen modes
- Follow MVVM pattern where appropriate for maintainable code
- Use DispatcherTimer for precise timing control
- Ensure proper resource cleanup for timers and animations
