using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SteganographyApp;

public partial class App : Application
{
    // Charge le fichier XAML contenant les ressources globales de l'application.
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Configure la fenêtre principale une fois l'initialisation du framework terminée.
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}