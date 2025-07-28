using System.Configuration;
using System.Data;
using System.Windows;

namespace VisualMetronome;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
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

