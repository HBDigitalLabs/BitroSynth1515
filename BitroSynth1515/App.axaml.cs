using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BitroSynth1515.ViewModels;
namespace BitroSynth1515;

public partial class App : Application
{
    public static MainViewModel SharedVM { get; set; } = new MainViewModel();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = SharedVM
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}