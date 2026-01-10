using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using SpotifyAPI.Web;

namespace VisualMetronome;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer metronomeTimer = null!;
    private bool isFlashing = false;
    private Color defaultColor = Color.FromRgb(44, 62, 80); // Dark blue-gray
    private Color flashColor = Colors.Green;
    private int currentBpm = 120;
    private bool isFullScreen = false;
    private WindowState previousWindowState;
    private WindowStyle previousWindowStyle;
    
    // Spotify integration
    private SpotifyService? _spotifyService;
    private BpmScraperService? _bpmScraperService;
    private List<PlaylistInfo> _playlists = new();
    private List<TrackInfo> _currentPlaylistTracks = new();
    private int _currentTrackIndex = -1;
    private const string SpotifyClientId = "214b11d0ab194e38a855e9201129b2bd";

    public MainWindow()
    {
        InitializeComponent();
        InitializeMetronome();
        InitializeSpotify();
        
        // Set initial focus to BPM textbox and set initial BPM after loading
        this.Loaded += MainWindow_Loaded;
    }
    
    private void InitializeSpotify()
    {
        _spotifyService = new SpotifyService(SpotifyClientId);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Set initial BPM after all controls are loaded
        UpdateBpm(120);
        
        // Set initial focus to BPM textbox and add event handlers
        if (BpmTextBox != null)
        {
            BpmTextBox.GotFocus += BpmTextBox_GotFocus;
            BpmTextBox.KeyDown += BpmTextBox_KeyDown;
            BpmTextBox.Focus();
            BpmTextBox.SelectAll();
        }
    }

    private void InitializeMetronome()
    {
        metronomeTimer = new DispatcherTimer();
        metronomeTimer.Tick += MetronomeTimer_Tick;
        // Start with default BPM
        double intervalMs = (60.0 / currentBpm) * 1000;
        metronomeTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        metronomeTimer.Start(); // Start once and keep running
    }

    private void MetronomeTimer_Tick(object? sender, EventArgs e)
    {
        FlashBackground();
    }

    private void FlashBackground()
    {
        // Alternate between the two colors on each beat
        if (BackgroundBrush != null)
        {
            if (isFlashing)
            {
                // Change to green
                BackgroundBrush.Color = flashColor;
                if (StatusText != null)
                    StatusText.Text = "♪ Beat";
            }
            else
            {
                // Change to default color
                BackgroundBrush.Color = defaultColor;
                if (StatusText != null)
                    StatusText.Text = "Ready";
            }
        }
        
        // Toggle the state for next beat
        isFlashing = !isFlashing;
    }

    private void BpmTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // Clear the textbox when it gets focus
        if (sender is TextBox textBox)
        {
            textBox.Clear();
            if (StatusText != null)
                StatusText.Text = "Enter BPM (1-300)";
        }
    }

    private void BpmTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Handle Enter key to apply BPM
        if (e.Key == Key.Enter && sender is TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int bpm))
            {
                if (bpm > 0 && bpm <= 300)
                {
                    UpdateBpm(bpm);
                    // Update the textbox to show the current BPM
                    textBox.Text = bpm.ToString();
                    // Remove focus from textbox
                    this.Focus();
                }
                else
                {
                    if (StatusText != null)
                        StatusText.Text = "BPM must be 1-300";
                }
            }
            else
            {
                if (StatusText != null)
                    StatusText.Text = "Invalid BPM";
            }
        }
    }

    private void UpdateBpm(int bpm)
    {
        if (bpm <= 0 || bpm > 300) return;
        
        currentBpm = bpm;
        
        // Always update the display
        if (BpmDisplay != null)
        {
            BpmDisplay.Text = $"{bpm} BPM";
        }
        
        // Calculate interval: 60 seconds / BPM = seconds per beat
        double intervalMs = (60.0 / bpm) * 1000;
        
        // Simply update the interval - timer keeps running smoothly
        if (metronomeTimer != null)
        {
            metronomeTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        }
        
        // Update status text
        if (StatusText != null)
        {
            StatusText.Text = $"Running at {bpm} BPM";
        }
    }

    private void FullScreenButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ToggleFullScreen();
        }
        else if (e.Key == Key.F11)
        {
            ToggleFullScreen();
        }
        // Allow space bar to start/stop
        else if (e.Key == Key.Space)
        {
            if (metronomeTimer != null && metronomeTimer.IsEnabled)
            {
                metronomeTimer.Stop();
                if (StatusText != null)
                    StatusText.Text = "Paused (Space to resume)";
                if (BackgroundBrush != null)
                    BackgroundBrush.Color = defaultColor;
            }
            else if (currentBpm > 0 && metronomeTimer != null)
            {
                metronomeTimer.Start();
                if (StatusText != null)
                    StatusText.Text = $"Resumed at {currentBpm} BPM";
            }
        }
    }

    private void ToggleFullScreen()
    {
        if (isFullScreen)
        {
            // Exit fullscreen
            this.WindowState = previousWindowState;
            this.WindowStyle = previousWindowStyle;
            this.Topmost = false;
            FullScreenButton.Content = "Toggle Fullscreen (ESC)";
            isFullScreen = false;
        }
        else
        {
            // Enter fullscreen
            previousWindowState = this.WindowState;
            previousWindowStyle = this.WindowStyle;
            
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.Topmost = true;
            FullScreenButton.Content = "Exit Fullscreen (ESC)";
            isFullScreen = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Clean up timer
        metronomeTimer?.Stop();
        base.OnClosed(e);
    }
    
    // Spotify Integration Methods
    private async void SpotifyLoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_spotifyService == null) return;
        
        SpotifyLoginButton.IsEnabled = false;
        SpotifyLoginButton.Content = "Connecting...";
        StatusText.Text = "Opening browser for Spotify login...";
        
        try
        {
            var success = await _spotifyService.AuthenticateAsync();
            
            if (success)
            {
                StatusText.Text = "Connected to Spotify!";
                SpotifyLoginButton.Content = "✓ Connected";
                SpotifyLoginButton.IsEnabled = false;
                
                await LoadPlaylists();
            }
            else
            {
                StatusText.Text = "Spotify connection failed";
                SpotifyLoginButton.Content = "Retry Connection";
                SpotifyLoginButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
            SpotifyLoginButton.Content = "Retry Connection";
            SpotifyLoginButton.IsEnabled = true;
        }
    }
    
    private async Task LoadPlaylists()
    {
        if (_spotifyService == null || !_spotifyService.IsAuthenticated) return;
        
        try
        {
            StatusText.Text = "Loading playlists...";
            var playlists = await _spotifyService.GetUserPlaylistsAsync();
            
            _playlists.Clear();
            _playlists.AddRange(playlists);
            
            PlaylistComboBox.ItemsSource = _playlists;
            PlaylistComboBox.IsEnabled = true;
            StatusText.Text = $"Loaded {_playlists.Count} playlists";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error loading playlists: {ex.Message}";
        }
    }
    
    private async void PlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlaylistComboBox.SelectedItem is not PlaylistInfo selectedPlaylist) return;
        if (_spotifyService == null) return;
        
        try
        {
            StatusText.Text = "Loading tracks...";
            StartButton.IsEnabled = false;
            PreviousButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            
            var tracks = await _spotifyService.GetPlaylistTracksAsync(selectedPlaylist.Id);
            
            _currentPlaylistTracks.Clear();
            foreach (var playlistTrack in tracks)
            {
                if (playlistTrack.Track is FullTrack track && !string.IsNullOrEmpty(track.Id))
                {
                    _currentPlaylistTracks.Add(new TrackInfo
                    {
                        Id = track.Id,
                        Name = track.Name,
                        Artists = string.Join(", ", track.Artists.Select(a => a.Name))
                    });
                    System.Diagnostics.Debug.WriteLine($"Added track: {track.Name} - ID: {track.Id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Skipped track - Type: {playlistTrack.Track?.Type}, ID null: {playlistTrack.Track is FullTrack ft && string.IsNullOrEmpty(ft.Id)}");
                }
            }
            
            _currentTrackIndex = -1;
            CurrentTrackText.Text = $"Loaded {_currentPlaylistTracks.Count} tracks";
            StartButton.IsEnabled = _currentPlaylistTracks.Count > 0;
            StatusText.Text = "Ready to start";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error loading tracks: {ex.Message}";
        }
    }
    
    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlaylistTracks.Count == 0) return;
        
        _currentTrackIndex = 0;
        await LoadAndApplyTrackBpm();
    }
    
    private async void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTrackIndex > 0)
        {
            _currentTrackIndex--;
            await LoadAndApplyTrackBpm();
        }
    }
    
    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTrackIndex < _currentPlaylistTracks.Count - 1)
        {
            _currentTrackIndex++;
            await LoadAndApplyTrackBpm();
        }
    }
    
    private async Task LoadAndApplyTrackBpm()
    {
        if (_spotifyService == null || _currentTrackIndex < 0 || 
            _currentTrackIndex >= _currentPlaylistTracks.Count) return;
        
        var track = _currentPlaylistTracks[_currentTrackIndex];
        
        // Always enable navigation buttons
        PreviousButton.IsEnabled = _currentTrackIndex > 0;
        NextButton.IsEnabled = _currentTrackIndex < _currentPlaylistTracks.Count - 1;
        
        CurrentTrackText.Text = $"Now Playing: {track.Name}\nBy: {track.Artists}\n\nSearching for BPM...";
        SongTitleDisplay.Text = track.Name;
        StatusText.Text = $"Searching songbpm.com for BPM...";
        
        // Initialize BPM scraper if needed
        if (_bpmScraperService == null)
        {
            _bpmScraperService = new BpmScraperService();
        }
        
        // Scrape BPM from songbpm.com
        var bpm = await _bpmScraperService.GetBpmAsync(track.Name, track.Artists);
        
        if (bpm.HasValue)
        {
            // Apply the BPM - always update, not just when flashing
            currentBpm = bpm.Value;
            BpmTextBox.Text = bpm.Value.ToString();
            
            // Always update the BPM display and metronome timing
            UpdateBpm(bpm.Value);
            
            CurrentTrackText.Text = $"Now Playing: {track.Name}\nBy: {track.Artists}\n\nBPM: {bpm.Value}";
            SongTitleDisplay.Text = track.Name;
            StatusText.Text = $"BPM set to {bpm.Value}";
        }
        else
        {
            CurrentTrackText.Text = $"Now Playing: {track.Name}\nBy: {track.Artists}\n\nBPM not found - enter manually above";
            SongTitleDisplay.Text = track.Name;
            StatusText.Text = $"BPM not found on songbpm.com - enter manually";
            
            // Focus the BPM textbox for manual entry
            BpmTextBox.Focus();
            BpmTextBox.SelectAll();
        }
    }
}