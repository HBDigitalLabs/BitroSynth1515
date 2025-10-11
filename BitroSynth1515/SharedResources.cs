using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

public static class SharedResources
{
    public enum MessageBoxType : byte
	{
		Info,
		Error,
		Warning,
		Success
	}

    public enum NoteChannel : byte
    {
        Channel1,
        Channel2,
        Channel3,
        Channel4
    }
	public enum BaseEngine : byte
	{
		RustSynthesizeEngine,
		RustPlaybackEngine
	}
	
	public static bool EngineFileExists(BaseEngine engine)
    {
        string baseName = engine switch
        {
            BaseEngine.RustSynthesizeEngine => "rust_synthesize_engine",
            BaseEngine.RustPlaybackEngine => "rust_playback_engine",
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, null)
        };

        List<string> candidates = GetPlatformCandidates(baseName);

        string[] searchDirs =
        {
            AppContext.BaseDirectory ?? string.Empty,
            Directory.GetCurrentDirectory()
        };

        foreach (string dir in searchDirs)
        {
            if (string.IsNullOrEmpty(dir)) continue;

            foreach (string name in candidates)
            {
                string path = Path.Combine(dir, name);
                if (File.Exists(path))
                    return true;
            }
        }

        return false;
    }

    private static List<string> GetPlatformCandidates(string baseName)
    {
        List<string> list = new List<string>(capacity: 6);

        if (OperatingSystem.IsWindows())
        {
            list.Add(baseName + ".dll");
            list.Add(baseName); 
        }
        else if (OperatingSystem.IsLinux())
        {
            list.Add("lib" + baseName + ".so");
            list.Add(baseName + ".so");
            list.Add("lib" + baseName);
            list.Add(baseName);
        }
        else if (OperatingSystem.IsMacOS())
        {
            list.Add("lib" + baseName + ".dylib");
            list.Add(baseName + ".dylib");
            list.Add("lib" + baseName);
            list.Add(baseName);
        }
        else
        {
            list.Add(baseName + ".so");
            list.Add(baseName + ".dll");
            list.Add(baseName + ".dylib");
            list.Add("lib" + baseName + ".so");
            list.Add(baseName);
        }

        return list;
    }

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
	
	public static string[] waveForms = { "PINK", "NOISE", "VOID", "SINE", "TRI", "SAW", "SQ" };

    public static async Task ShowMessageAsync(Window owner, MessageBoxType type, string message)
	{

		var box = type switch
		{
			MessageBoxType.Info => MessageBoxManager
				.GetMessageBoxStandard("Info", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info),

			MessageBoxType.Warning => MessageBoxManager
				.GetMessageBoxStandard("Warning", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning),

			MessageBoxType.Error => MessageBoxManager
				.GetMessageBoxStandard("Error", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error),

			MessageBoxType.Success => MessageBoxManager
				.GetMessageBoxStandard("Success", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success),

			_ => MessageBoxManager
				.GetMessageBoxStandard("Info", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info),
		};

		await box.ShowWindowDialogAsync(owner);
	}
}