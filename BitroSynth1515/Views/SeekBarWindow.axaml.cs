using Avalonia.Controls;
using Avalonia.Interactivity;
using RustPlaybackNative;
using RustSynthesizeNative;

namespace BitroSynth1515;

public partial class SeekBarWindow : Window
{
	public SeekBarWindow()
	{
		InitializeComponent();
		int durationMs = RustSynthesizeEngine.get_duration_of_last_sound();
		infoLabel.Text = $"Enter the duration in milliseconds.\nThe duration of your last synthesized\nsound in milliseconds is {durationMs}";
	}

	private async void Apply(object? sender, RoutedEventArgs e)
	{
		int milliseconds = 0;
		bool error = int.TryParse(SharedResources.RemoveAllWhiteSpace(msTextBox.Text ?? ""), out milliseconds);
		if (error == false)
			await SharedResources.ShowMessageAsync(this, SharedResources.MessageBoxType.Error, $"Error, Please enter a valid value.");
		else if (milliseconds < 0)
			await SharedResources.ShowMessageAsync(this, SharedResources.MessageBoxType.Warning, $"The millisecond value cannot be less than 0.");
		else if (milliseconds > RustSynthesizeEngine.get_duration_of_last_sound())
			await SharedResources.ShowMessageAsync(this, SharedResources.MessageBoxType.Error, $"The millisecond value cannot be greater than the length of the audio file.");
		else
		{
			RustPlaybackEngine.startPositionMs = milliseconds;
			this.Close();
		}
			
	}

}