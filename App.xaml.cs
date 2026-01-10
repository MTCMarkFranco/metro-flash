using System.Configuration;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;

namespace VisualMetronome;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    private const int ATTACH_PARENT_PROCESS = -1;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Attach to parent console if running from command line
        if (!AttachConsole(ATTACH_PARENT_PROCESS))
        {
            // If no parent console, allocate a new one
            AllocConsole();
        }
        
        Console.WriteLine("Visual Metronome Starting...");
        Console.WriteLine("Console output enabled for debugging");
        Console.WriteLine("");
        
        try
        {
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"STARTUP ERROR: {ex}");
            MessageBox.Show($"Application startup error: {ex.Message}\n\nDetails: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Unhandled exception: {e.Exception.Message}\n\nDetails: {e.Exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
        Shutdown(1);
    }
}

