using System.Windows;

namespace NewDesk.Shell;

/// <summary>
/// Phase 0 placeholder UI: a small always-on-top panel in the corner with the way out.
/// Replaced by the real taskbar in phase 1.
/// </summary>
public partial class ControlWindow : Window
{
    public ControlWindow(bool hotkeyRegistered)
    {
        InitializeComponent();
        Heading.Text = Strings.Running;
        Body.Text = Strings.TaskbarHidden;
        Hotkey.Text = hotkeyRegistered ? Strings.HotkeyHint : Strings.HotkeyMissing;
        RestoreButton.Content = Strings.RestoreButton;
        CrashButton.Content = Strings.CrashButton;

        Loaded += (_, _) =>
        {
            var area = SystemParameters.WorkArea;
            Left = area.Right - ActualWidth - 16;
            Top = area.Bottom - ActualHeight - 16;
        };

        // Closing the panel with its X is treated as "restore and exit", never as "hide and keep going".
        Closing += (_, args) =>
        {
            var app = (App)Application.Current;
            if (app.IsExiting) return;
            args.Cancel = true;
            app.RestoreAndExit();
        };
    }

    private void OnRestore(object sender, RoutedEventArgs e) => ((App)Application.Current).RestoreAndExit();

    private void OnSimulateCrash(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(this, Strings.CrashConfirm, Strings.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer == MessageBoxResult.Yes)
            App.SimulateHardCrash();
    }
}
