using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Text;
using RustSynthesizeNative;
using RustPlaybackNative;

using System;
using System.Threading;
using Avalonia.Threading;
using System.Linq;
using System.Globalization;
using static SharedResources;

namespace BitroSynth1515;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += (s, e) => DeInit();
        Loaded += async (_, __) => await InitAsync();
        App.SharedVM.Channel1Text = "By using this application, you agree to the terms of the license.\nThe license can be found in the LICENSES directory located in the root folder of the application.";

        this.DataContext = App.SharedVM;
    }



    private void DeInit()
    {
        RustPlaybackEngine.audio_engine_deinit();
        timer.Dispose();
        cts.Dispose();
    }
    private async Task InitAsync()
    {
        if (RustPlaybackEngine.audio_engine_init() != 0)
        {
            await ShowMessageAsync(this,MessageBoxType.Error, "RustPlaybackEngine failed to start");
            Close();
        }
        timer.Elapsed += (sender, e) =>
        {
            TimeSpan elapsed = DateTime.Now - startTime;

            byte status = RustPlaybackEngine.get_playback_status();

            if (status == 0)
            {
                timer.Stop();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    timerLabel.Content = MillisecondsToFormat(elapsed);
                });
                return;
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                timerLabel.Content = MillisecondsToFormat(elapsed);
            });
        };
        timer.AutoReset = true;

    }

    private static List<string> noteList = new List<string>();

    readonly System.Timers.Timer timer = new System.Timers.Timer(50);
    DateTime startTime = DateTime.Now;
    CancellationTokenSource cts = new CancellationTokenSource();

    private bool firstSynthesisStatus = false;
    public static NoteChannel displayedChannel = NoteChannel.Channel1;

    public static string RemoveAllWhiteSpace(string input)
    {
        StringBuilder result = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (!char.IsWhiteSpace(c))
                result.Append(c);
        }
        return result.ToString();
    }

    private async Task<string> FilterAndClearText(string text)
    {
        string[] parts = Array.FindAll(RemoveAllWhiteSpace(text).Split('>'), s => !string.IsNullOrWhiteSpace(s));


        if (parts.Length == 0)
        {
            return "";
        }

        StringBuilder output_string = new StringBuilder();

        foreach (string part in parts)
        {
            string[] token = Array.FindAll(part.Split('_'), s => !string.IsNullOrWhiteSpace(s));
            if (token.Length < 4)
            {
                if (part.Length <= 20)
                    await ShowMessageAsync(this,MessageBoxType.Warning, $"Missing parameter: \"{part}\"");
                else if (part.Length > 20)
                    await ShowMessageAsync(this,MessageBoxType.Warning, $"Missing parameter: \"{part[0..15]}...\"");
                return "E";
            }


            float note = 0.0f;
            int milliseconds = 0;
            float gain = 0.0f;
            string waveFormType = token[3].ToUpper();

            // Note to frequency
            if (RustSynthesizeEngine.noteFrequency.TryGetValue(token[0].ToUpper(), out note) == false)
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"Unknown note: \"{token[0]}\"");
                return "E";
            }
            // string to integer
            else if (int.TryParse(token[1], out milliseconds) == false)
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"Unknown milliseconds value: \"{token[1]}\"");
                return "E";
            }
            else if (milliseconds <= 0)
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"The millisecond value cannot be less than or equal to 0: \"{token[1]}\"");
                return "E";
            }

            // string to gain(float)
            else if (
                (float.TryParse(token[2], NumberStyles.Float, CultureInfo.InvariantCulture, out gain)
                || float.TryParse(token[2], NumberStyles.Float, CultureInfo.CurrentCulture, out gain)) == false)
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"Unknown gain value: \"{token[2]}\"");
                return "E";
            }
            else if (gain > 1.0 || gain < 0.0)
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"The gain value can only be between 0.0 and 1.0: \"{token[2]}\"");
                return "E";
            }

            // wave form type
            else if (!Array.Exists(waveForms, w => w == waveFormType))
            {
                await ShowMessageAsync(this,MessageBoxType.Warning, $"Unknown wave form type: \"{waveFormType}\"");
                return "E";
            }

            string result = $"{note}_{milliseconds}_{gain}_{waveFormType}>";

            output_string.Append(result);


        }
        return output_string.ToString().TrimEnd('>');
    }

    private int GainToWidth(float gain)
    {

        switch (gain)
        {
            case <= 0.1f:
                return 28;
            case <= 0.2f:
                return 56;
            case <= 0.3f:
                return 84;
            case <= 0.4f:
                return 112;
            case <= 0.5f:
                return 140;
            case <= 0.6f:
                return 168;
            case <= 0.7f:
                return 196;
            case <= 0.8f:
                return 224;
            case <= 0.9f:
                return 252;
            default:
                return 285;
        }
    }

    private string MillisecondsToFormat(TimeSpan ts)
    {
        int hours = ts.Hours;
        int minutes = ts.Minutes;
        int seconds = ts.Seconds;
        int milliseconds = ts.Milliseconds;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }

    private string GetChannelName(NoteChannel ch)
    {
        switch (ch)
        {
            case NoteChannel.Channel1: return "Channel-1";
            case NoteChannel.Channel2: return "Channel-2";
            case NoteChannel.Channel3: return "Channel-3";
            default: return "Channel-4";
        }
    }

   
    public async Task ShowNote()
    {
        if (cts.IsCancellationRequested)
            cts = new CancellationTokenSource();

        CancellationToken ctsToken = cts.Token;

        foreach (string note in noteList)
        {
            float gain = 0.0f;
            int milliseconds = 20;

            string[] parts = note.Split('_');

            if (parts.Length < 3 ||
                !int.TryParse(parts[1], out milliseconds))
                continue;

            if (!(float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out gain)
                  || float.TryParse(parts[2], NumberStyles.Float, CultureInfo.CurrentCulture, out gain)))
            {
                continue;
            }

            milliseconds = Math.Max(1, milliseconds);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SetStatusBarText(note);
                gainBar.Width = GainToWidth(gain);
            });

            await Task.Delay(milliseconds, ctsToken);
        }

        string completedChannel = GetChannelName(displayedChannel);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SetStatusBarText($"{completedChannel} is Completed");
            gainBar.Width = GainToWidth(0.0f);
        });



    }


    private async Task<int> GenerateSound()
    {

        string channel_1_value = await FilterAndClearText(App.SharedVM.Channel1Text);
        string channel_2_value = await FilterAndClearText(App.SharedVM.Channel2Text);
        string channel_3_value = await FilterAndClearText(App.SharedVM.Channel3Text);
        string channel_4_value = await FilterAndClearText(App.SharedVM.Channel4Text);


        switch (displayedChannel)
        {
            case NoteChannel.Channel1:
                noteList = channel_1_value.Split('>').ToList();
                break;
            case NoteChannel.Channel2:
                noteList = channel_2_value.Split('>').ToList();
                break;
            case NoteChannel.Channel3:
                noteList = channel_3_value.Split('>').ToList();
                break;
            case NoteChannel.Channel4:
                noteList = channel_4_value.Split('>').ToList();
                break;
        }

        if (channel_1_value != "E" && channel_2_value != "E" && channel_3_value != "E" && channel_4_value != "E")
            return await Task.Run(() =>
                RustSynthesizeEngine.synthesize(
                    channel_1_value,
                    channel_2_value,
                    channel_3_value,
                    channel_4_value,
                    RustSynthesizeEngine.bit8Status,
                    RustSynthesizeEngine.cachePath)
            );
        else
            return -2;

    }

    

    private async void ShowCreateNoteWithGUIWindow(object? sender, RoutedEventArgs e)
    {
        if (StopSound() != 0)
        {
            await ShowMessageAsync(this,MessageBoxType.Warning, "Playback could not be stopped");
            return;
        }
            
        CreateNoteWithGUIWindow settingsWindow = new CreateNoteWithGUIWindow(){
            DataContext = App.SharedVM
        };
        await settingsWindow.ShowDialog(this);
    }

    private async void ShowSettingsWindow(object? sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new SettingsWindow();
        await settingsWindow.ShowDialog(this);
    }

    private void SetStatusBarText(string text)
    {
        statusBar.Text = text;
    }

    private async void PlaySound(object? sender, RoutedEventArgs e)
    {
        byte playBackStatus = RustPlaybackEngine.get_playback_status();
        if (playBackStatus == 1)
        {
            await ShowMessageAsync(this,MessageBoxType.Info, "Audio is already playing, stop the audio first.");
            return;
        }
        SetStatusBarText("Synthesizing");
        int status = await GenerateSound();
        RustPlaybackEngine.set_file_path(RustSynthesizeEngine.cachePath);

        if (status == -2)
        {
            SetStatusBarText("Could not be synthesized");
            return;
        }
        else if (status == -1)
        {
            await ShowMessageAsync(this,MessageBoxType.Error, "Could not be synthesized");
            SetStatusBarText("Could not be synthesized");
            return;
        }


        if (RustPlaybackEngine.play() != 0)
            await ShowMessageAsync(this,MessageBoxType.Warning, "Playback failed to start");
        else
        {
            firstSynthesisStatus = true;
            try
            {

                startTime = DateTime.Now;
                timer.Start();
                await ShowNote();
            }
            catch (OperationCanceledException)
            {
                // cancelled
            }
        }

    }

    private async void StopSoundForButton(object? sender, RoutedEventArgs e)
    {
        if (StopSound() != 0) 
            await ShowMessageAsync(this,MessageBoxType.Warning, "Playback could not be stopped");

    }

    private int StopSound()
    {
        if (firstSynthesisStatus)
        {
           if (RustPlaybackEngine.stop() != 0)
            {
             return -1;
            }
            else
            {
                SetStatusBarText("Audio is stopped.");
                cts.Cancel();
                timer.Stop();
                return 0;
            } 
        }
        return 0;

    }


    private async void OpenJSONFile(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Where is your JSON File?",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
            }
        };


        IReadOnlyList<IStorageFile>? files = await this.StorageProvider.OpenFilePickerAsync(options);
        if (files != null && files.Count > 0)
        {
            IStorageFile file = files[0];

            try
            {
                await using Stream stream = await file.OpenReadAsync();
                using StreamReader reader = new StreamReader(stream);
                string jsonData = await reader.ReadToEndAsync();

                Dictionary<string, string>? channels = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

                if (channels != null)
                {
                    App.SharedVM.Channel1Text = channels.GetValueOrDefault("Channel-1", "");
                    App.SharedVM.Channel2Text = channels.GetValueOrDefault("Channel-2", "");
                    App.SharedVM.Channel3Text = channels.GetValueOrDefault("Channel-3", "");
                    App.SharedVM.Channel4Text = channels.GetValueOrDefault("Channel-4", "");
                }
            }
            catch (JsonException)
            {
                await ShowMessageAsync(this,MessageBoxType.Error, "Invalid JSON format!");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync(this,MessageBoxType.Error, $"Could not open file: {ex.Message}");
            }

        }
    }
    private async void SaveAsJSONFile(object? sender, RoutedEventArgs e)
    {
        FilePickerSaveOptions options = new FilePickerSaveOptions
        {
            Title = "Save As JSON File",
            SuggestedFileName = "channels.json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON File") { Patterns = new[] { "*.json" } }
            }
        };
        IStorageFile? path = await this.StorageProvider.SaveFilePickerAsync(options);
        if (path != null)
        {
            Dictionary<string, string> channels = new Dictionary<string, string>
            {
                ["Channel-1"] = App.SharedVM.Channel1Text,
                ["Channel-2"] = App.SharedVM.Channel2Text,
                ["Channel-3"] = App.SharedVM.Channel3Text,
                ["Channel-4"] = App.SharedVM.Channel4Text
            };

            string jsonData = JsonSerializer.Serialize(channels, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await using Stream stream = await path.OpenWriteAsync();
                stream.SetLength(0);
                await using StreamWriter writer = new StreamWriter(stream);
                await writer.WriteAsync(jsonData);

                await ShowMessageAsync(this,MessageBoxType.Success, "Successful");



            }
            catch (Exception ex)
            {
                await ShowMessageAsync(this,MessageBoxType.Error, $"Save failed: {ex.Message}");
            }
        }
    }

    private async void ExportAudio(object? sender, RoutedEventArgs e)
    {

        if (StopSound() != 0)
        {
            await ShowMessageAsync(this,MessageBoxType.Warning, "Playback could not be stopped");
            return;
        }
            

        FilePickerSaveOptions options = new FilePickerSaveOptions
        {
            Title = "Export Audio",
            SuggestedFileName = "output.wav",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("WAV File") { Patterns = new[] { "*.wav" } }
            }
        };

        IStorageFile? path = await this.StorageProvider.SaveFilePickerAsync(options);

        if (path != null)
        {
            int status = await GenerateSound();

            if (status == -2)
            {
                SetStatusBarText("Could not export audio.");
                return;
            }
            else if (status == -1)
            {
                await ShowMessageAsync(this,MessageBoxType.Error, "Could not be synthesized");
                SetStatusBarText("Could not export audio.");
                return;
            }

            string filePath = path.Path.LocalPath;

            try
            {
                if (File.Exists(RustSynthesizeEngine.cachePath))
                {
                    File.Copy(RustSynthesizeEngine.cachePath, filePath, true);
                    await ShowMessageAsync(this,MessageBoxType.Success, "Successful");
                    SetStatusBarText("Audio exported.");
                }
                else
                {
                    await ShowMessageAsync(this,MessageBoxType.Error, "Temporary cache file not found!");
                    SetStatusBarText("Could not export audio.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync(this,MessageBoxType.Error, $"Export failed: {ex.Message}");
                SetStatusBarText("Could not export audio.");
            }

        }
        else
            SetStatusBarText("Cancelled.");
    }


    private async void ShowAboutWindow(object? sender, RoutedEventArgs e)
    {
        AboutWindow aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog(this);
    }
}