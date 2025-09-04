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