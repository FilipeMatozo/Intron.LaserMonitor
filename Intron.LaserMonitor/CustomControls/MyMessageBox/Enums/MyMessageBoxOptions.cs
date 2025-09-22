using System.Windows.Media;

namespace Intron.LaserMonitor.CustomControls.MyMessageBox.Enums;
public sealed record MyMessageBoxOptions(
string Title,
string Message,
string? Detail = null,
MyMessageBoxButtons Buttons = MyMessageBoxButtons.Ok,
MyMessageBoxIcon Icon = MyMessageBoxIcon.None,
string? PrimaryText = null,
string? SecondaryText = null,
string? TertiaryText = null,
Brush? AccentBrush = null,
bool ShowCopyButton = true,
bool ShowDoNotAskAgain = false,
string? DoNotAskAgainText = "Não perguntar novamente",
bool IsDoNotAskAgainChecked = false,
MyMessageBoxDefaultButton DefaultButton = MyMessageBoxDefaultButton.Auto,
MyMessageBoxResult CancelResult = MyMessageBoxResult.Cancel
);
