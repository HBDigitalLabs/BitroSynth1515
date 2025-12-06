using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RustSynthesizeNative;
using RustPlaybackNative;
using System.Text;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia;
using static SharedResources;
using System.Linq;

namespace BitroSynth1515;

public partial class CreateNoteWithGUIWindow : Window
{
	public CreateNoteWithGUIWindow()
	{

		InitializeComponent();
		Closing += async (_, _) =>
		{
			if (RustPlaybackEngine.stop() != (int)RustPlaybackEngine.EngineStatus.Success)
				await ShowMessageAsync(this, MessageBoxType.Error, "Playback could not be stopped");
		};



		SizeChanged += async (_, _) => await Draw();

		notes.ItemsSource = RustSynthesizeEngine.noteFrequency.Keys;
		notes.SelectedIndex = 0;
		this.DataContext = App.SharedVM;
	}


	private float[] samples = new float[1000];
	private const float sampleRate = 11025f;


	private async Task<int> GenerateSound()
	{

		string channel_1_value = await FilterAndClearText(false);

		if (channel_1_value != "E")
			return await Task.Run(() =>
				RustSynthesizeEngine.synthesize(
					channel_1_value,
					"",
					"",
					"",
					RustSynthesizeEngine.bit8Status,
					RustSynthesizeEngine.cachePath)
			);
		else
			return (int)RustSynthesizeEngine.EngineStatus.SilentError;

	}



	private void GenerateVoid()
	{

		for (short i = 0; i < samples.Length; i++)
			samples[i] = 0;
	}

	private void GenerateTriangle()
	{
		float frequency = GetFrequency();

		if (frequency == -1) return;

		for (short n = 0; n < samples.Length; n++)
		{
			float t = n / sampleRate;
			double x = frequency * t - Math.Floor(frequency * t);
			float sample = (float)(4.0 * Math.Abs(x - 0.5) - 1.0);
			samples[n] = sample;
		}
	}

	private void GeneratePinkNoise()
	{
		GenerateNoise();

		float b0 = 0f;
		float b1 = 0f;
		float b2 = 0f;
		float b3 = 0f;
		float b4 = 0f;
		float b5 = 0f;
		float b6 = 0f;


		for (short i = 0; i < samples.Length; i++)
		{
			float x = samples[i];
			b0 = 0.99886f * b0 + x * 0.0555179f;
			b1 = 0.99332f * b1 + x * 0.0750759f;
			b2 = 0.96900f * b2 + x * 0.1538520f;
			b3 = 0.86650f * b3 + x * 0.3104856f;
			b4 = 0.55000f * b4 + x * 0.5329522f;
			b5 = -0.7616f * b5 - x * 0.0168980f;
			float y = b0 + b1 + b2 + b3 + b4 + b5 + b6 + x * 0.5362f;
			b6 = x * 0.115926f;
			samples[i] = y;
		}

		float max_amp = 0f;

		for (short i = 0; i < samples.Length; i++)
		{
			float abs_value = Math.Abs(samples[i]);
			if (abs_value > max_amp)
				max_amp = abs_value;
		}

		if (max_amp > 1.0)
			for (short i = 0; i < samples.Length; i++)
				samples[i] /= max_amp; 
	}

	private void GenerateSawtooth()
	{
		float frequency = GetFrequency();

		if (frequency == -1) return;

		for (short n = 0; n < samples.Length; n++)
		{
			float t = n / sampleRate;
			float value = 2f * (float)(frequency * t - Math.Floor(frequency * t)) - 1f;
			samples[n] = value;
		}
	}
	private void GenerateSine()
	{
		float frequency = GetFrequency();

		if (frequency == -1) return;

		for (short n = 0; n < samples.Length; n++)
		{
			float t = n / sampleRate;
			float value = (float)Math.Sin(2.0 * Math.PI * frequency * t);
			samples[n] = value;
		}
	}

	private void GenerateSquare()
	{
		float frequency = GetFrequency();

		if (frequency == -1) return;

		for (short n = 0; n < samples.Length; n++)
		{
			float t = n / sampleRate;
			float value = 0.0f;

			if (Math.Sin(2.0 * Math.PI * frequency * t) >= 0.0) value = 1.0f;
			else value = -1.0f;

			samples[n] = value;
		}
	}

	private void GenerateNoise()
	{
		Random rnd = new Random();
		for (short i = 0; i < samples.Length; i++)
			samples[i] = (float)rnd.NextDouble() * 2 - 1;
	}

	private float GetFrequency()
	{
		string noteType = "";

		var noteChoose = notes.SelectedItem;

		float frequency = 0.0f;

		if (noteChoose is string noteItem)
			noteType = noteItem.ToString();

		if (RustSynthesizeEngine.noteFrequency.TryGetValue(noteType, out frequency) == false)
			return -1;

		return frequency;

	}

	private double GetGain()
	{
		double gain = -1;
		if (
			(
				double.TryParse(gainTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out gain)
				||
				double.TryParse(gainTextBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out gain)
			) == false
		)
		{
			return -1;
		}
		else if (gain > 1.0 || gain < 0.0)
		{
			return -1;
		}

		return gain;
	}

	private async void ReDraw(object? sender, RoutedEventArgs e)
		=> await Draw();

	private async Task Draw()
	{


		string waveFormType = "";

		var waveChoose = waves.SelectedItem;

		if (waveChoose is ComboBoxItem waveItem)
			waveFormType = waveItem.Content?.ToString() ?? "";

		if (!Array.Exists(waveForms, w => w == waveFormType))
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"Unknown wave form type: \"{waveFormType}\"");
			return;
		}

		switch (waveFormType)
		{
			case "TRI":
				GenerateTriangle();
				break;
			case "SAW":
				GenerateSawtooth();
				break;
			case "SQ":
				GenerateSquare();
				break;
			case "PINK":
				GeneratePinkNoise();
				break;
			case "NOISE":
				GenerateNoise();
				break;
			case "SINE":
				GenerateSine();
				break;
			default:
				GenerateVoid();
				break;
		}


		double canvasWidth = waveCanvas.Bounds.Width;
		double canvasHeight = waveCanvas.Bounds.Height;
		double centerY = canvasHeight / 2;
		double gain = GetGain();

		if (gain == -1) return;

		gain = centerY * gain;

		StreamGeometry waveGeometry = new StreamGeometry();
		using (StreamGeometryContext context = waveGeometry.Open())
		{
			context.BeginFigure(new Point(0, centerY), false);

			int step = Math.Max(1, samples.Length / (int)canvasWidth);
			for (int i = 0; i < samples.Length; i += step)
			{
				double x = i * (canvasWidth / samples.Length);
				double y = centerY - (samples[i] * gain);
				context.LineTo(new Point(x, y));
			}
		}

		wavePath.Data = waveGeometry;
	}

	private async void AddNote(object? sender, RoutedEventArgs e)
	{


		string text = await FilterAndClearText(true);
		if (channel_1.IsChecked == true && text != "E")
			App.SharedVM.Channel1Text += text;
		else if (channel_2.IsChecked == true && text != "E")
			App.SharedVM.Channel2Text += text;
		else if (channel_3.IsChecked == true && text != "E")
			App.SharedVM.Channel3Text += text;
		else if (channel_4.IsChecked == true && text != "E")
			App.SharedVM.Channel4Text += text;
	}

	private void DeleteNote(object? sender, RoutedEventArgs e)
	{
		StringBuilder result = new StringBuilder();
		List<string> stringList = new List<string>();

		if (channel_1.IsChecked == true)
			stringList = App.SharedVM.Channel1Text.Split('>').ToList();
		else if (channel_2.IsChecked == true)
			stringList = App.SharedVM.Channel2Text.Split('>').ToList();
		else if (channel_3.IsChecked == true)
			stringList = App.SharedVM.Channel3Text.Split('>').ToList();
		else if (channel_4.IsChecked == true)
			stringList = App.SharedVM.Channel4Text.Split('>').ToList();

		for (int i = 0; i < (stringList.Count - 2); i++)
		{
			result.Append($"{stringList[i]}>");
		}

		if (channel_1.IsChecked == true)
			App.SharedVM.Channel1Text = result.ToString();
		else if (channel_2.IsChecked == true)
			App.SharedVM.Channel2Text = result.ToString();
		else if (channel_3.IsChecked == true)
			App.SharedVM.Channel3Text = result.ToString();
		else if (channel_4.IsChecked == true)
			App.SharedVM.Channel4Text = result.ToString();
	}

	private async Task<string> FilterAndClearText(bool forAdd = false)
	{

		float note = 0.0f;
		int milliseconds = 0;
		float gain = 0.0f;
		string waveFormType = "";
		string noteType = "";

		var waveChoose = waves.SelectedItem;
		var noteChoose = notes.SelectedItem;

		if (waveChoose is ComboBoxItem waveItem)
			waveFormType = waveItem.Content?.ToString() ?? "";
		if (noteChoose is string noteItem)
			noteType = noteItem.ToString();

		// Note to frequency
		if (RustSynthesizeEngine.noteFrequency.TryGetValue(noteType, out note) == false)
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"Unknown note: \"{noteType}\"");
			return "E";
		}
		// string to integer
		else if (int.TryParse(millisecondsTextBox.Text, out milliseconds) == false)
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"Unknown milliseconds value: \"{millisecondsTextBox.Text}\"");
			return "E";
		}
		else if (milliseconds <= 0)
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"The millisecond value cannot be less than or equal to 0: \"{millisecondsTextBox.Text}\"");
			return "E";
		}

		// string to gain(float)
		else if (
			(float.TryParse(gainTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out gain)
			|| float.TryParse(gainTextBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out gain)) == false)
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"Unknown gain value: \"{gainTextBox.Text}\"");
			return "E";
		}
		else if (gain > 1.0 || gain < 0.0)
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"The gain value can only be between 0.0 and 1.0: \"{gainTextBox.Text}\"");
			return "E";
		}

		// wave form type
		else if (!Array.Exists(waveForms, w => w == waveFormType))
		{
			await ShowMessageAsync(this, MessageBoxType.Warning, $"Unknown wave form type: \"{waveFormType}\"");
			return "E";
		}


		if (forAdd)
			return $"{noteType}_{milliseconds}_{gain}_{waveFormType}>";
		else
			return $"{note}_{milliseconds}_{gain}_{waveFormType}";



	}

	private async void StopSound(object? sender, RoutedEventArgs e)
	{
		if (RustPlaybackEngine.stop() != (int)RustPlaybackEngine.EngineStatus.Success)
			await ShowMessageAsync(this, MessageBoxType.Warning, "Playback could not be stopped");
	}

	private async void PlaySound(object? sender, RoutedEventArgs e)
	{

		int status = await GenerateSound();

		if (status == (int)RustSynthesizeEngine.EngineStatus.SilentError)
			return;
		else if (status == (int)RustSynthesizeEngine.EngineStatus.Error)
		{
			await ShowMessageAsync(this, MessageBoxType.Error, "Could not be synthesized");
			return;
		}

		RustPlaybackEngine.set_file_path(RustSynthesizeEngine.cachePath);

		if (RustPlaybackEngine.play(0) != (int)RustPlaybackEngine.EngineStatus.Success)
		{
			
			await ShowMessageAsync(this, MessageBoxType.Warning, "Playback failed to start");
		}


	}

}