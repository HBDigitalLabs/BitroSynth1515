using Avalonia.Controls;
using Avalonia.Interactivity;
using RustSynthesizeNative;
using static SharedResources;

namespace BitroSynth1515;

public partial class SettingsWindow : Window
{
	public SettingsWindow()
	{
		InitializeComponent();
		CanResize = false;
		sampleRateTextBox.Text = "11025";
	}


	private void Apply(object? sender, RoutedEventArgs e)
	{
        int sampleRate = 11025;
        if (int.TryParse(sampleRateTextBox.Text, out sampleRate) == false)
		{
			infoText.Text = "Please enter a valid number.";
			return;
		}
		else if (sampleRate < 8000)
		{
			infoText.Text = "Please do not enter a number\nless than 8000";
			return;
		}
		else if (sampleRate > 48000)
		{
			infoText.Text = "Please do not enter a number\ngreater than 48000";
			return;
		}



		if (channel_1.IsChecked == true)
			MainWindow.displayedChannel = NoteChannel.Channel1;
		else if (channel_2.IsChecked == true)
			MainWindow.displayedChannel = NoteChannel.Channel2;
		else if (channel_3.IsChecked == true)
			MainWindow.displayedChannel = NoteChannel.Channel3;
		else
			MainWindow.displayedChannel = NoteChannel.Channel4;

		if (bit8Radio.IsChecked == true)
		{
			RustSynthesizeEngine.bit8Status = 1;
			RustSynthesizeEngine.set_sample_rate(sampleRate);
			Close();
		}
		else
		{
			RustSynthesizeEngine.bit8Status = 0;
			RustSynthesizeEngine.set_sample_rate(sampleRate);
			Close();
		}

	}
}