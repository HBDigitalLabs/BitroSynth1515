using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BitroSynth1515;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        infoLabel.Text = "By using this application, you agree to the terms of the license.\nThe license can be found in the LICENSES directory located in the root folder of the application.\n\nDeveloper: HBDigitalLabs\nVersion: v2.1.1";
    }

    private void CloseWindow(object? sender, RoutedEventArgs e) => Close();

}
