# Spotify Integration Setup

To use the Spotify integration features, you need to create a Spotify Developer App and get a Client ID.

## Steps to Set Up:

1. **Create a Spotify Developer Account**
   - Go to https://developer.spotify.com/dashboard
   - Log in with your Spotify account
   - Accept the Developer Terms of Service

2. **Create a New App**
   - Click "Create app"
   - Fill in the app details:
     - App name: "Visual Metronome" (or any name you prefer)
     - App description: "BPM synchronization for visual metronome"
     - Redirect URI: `http://127.0.0.1:5000/callback` (IMPORTANT!)
     - Select "Web API" as the API you'll use
   - Accept the Terms of Service and click "Save"

3. **IMPORTANT: Check App Settings**
   - In your app's settings, make sure the app is in "Development Mode"
   - Under "Edit Settings", verify the Redirect URI is exactly: `http://127.0.0.1:5000/callback`
   - Some users report needing to add their Spotify account email to the "Users and Access" section for development apps

4. **Get Your Client ID**
   - On your app's dashboard, you'll see your **Client ID**
   - Copy this Client ID

5. **Add Client ID to the Application**
   - Open `MainWindow.xaml.cs`
   - Find this line near the top of the MainWindow class:
     ```csharp
     private const string SpotifyClientId = "YOUR_CLIENT_ID_HERE";
     ```
   - Replace `YOUR_CLIENT_ID_HERE` with your actual Client ID
   - Save the file

6. **Build and Run**
   - Build the project: `dotnet build VisualMetronome.csproj`
   - Run the application: `dotnet run --project VisualMetronome.csproj`

## Using Spotify Features:

1. Click "Connect to Spotify" button
2. A browser window will open asking you to authorize the app
3. After authorization, your playlists will load in the dropdown
4. Select a playlist from the dropdown
5. Click "Start" to load the first song's BPM
6. Use "Previous" and "Next" buttons to navigate through songs
7. The metronome will automatically adjust to each song's BPM

## Troubleshooting:

- **Port 5000 already in use**: Make sure no other application is using port 5000
- **Authentication fails**: Double-check that the Redirect URI is exactly `http://localhost:5000/callback` in your Spotify app settings
- **No playlists showing**: Make sure you have playlists in your Spotify account
- **BPM not found**: Some tracks may not have BPM data in Spotify's database

## Privacy Note:

This app only requests permission to read your playlists and does not access or modify your playback or any other personal data.
