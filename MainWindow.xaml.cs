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

    public MainWindow()
    {
        InitializeComponent();
        InitializeMetronome();
        
        // Set initial focus to BPM textbox and set initial BPM after loading
        this.Loaded += MainWindow_Loaded;
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
        // Don't call UpdateBpm here - wait for Loaded event
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
        
        // Check if UI elements are initialized
        if (BpmDisplay != null)
        {
            BpmDisplay.Text = $"{bpm} BPM";
        }
        
        // Calculate interval: 60 seconds / BPM = seconds per beat
        // Full interval for one beat (no division by 2)
        double intervalMs = (60.0 / bpm) * 1000;
        
        // Ensure timer is initialized
        if (metronomeTimer != null)
        {
            metronomeTimer.Stop();
            metronomeTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
            metronomeTimer.Start();
        }
        
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
}