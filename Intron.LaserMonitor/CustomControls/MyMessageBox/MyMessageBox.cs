using Intron.LaserMonitor.CustomControls.MyMessageBox.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Intron.LaserMonitor.CustomControls.MyMessageBox
{
    public static class MyMessageBox
    {
        // Sincronamente (modal)
        public static (MyMessageBoxResult Result, bool DoNotAskAgain) Show(Window? owner, MyMessageBoxOptions options)
        {
            var win = new MyMessageBoxWindow(options)
            {
                Owner = owner,
                WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
            bool? dlg = win.ShowDialog();
            return (win.Result, win.IsDoNotAskAgainChecked);
        }


        public static (MyMessageBoxResult Result, bool DoNotAskAgain) Show(string title, string message, string? detail = null,
        MyMessageBoxButtons buttons = MyMessageBoxButtons.Ok, MyMessageBoxIcon icon = MyMessageBoxIcon.None,
        Window? owner = null)
        {
            var opts = new MyMessageBoxOptions(title, message, detail, buttons, icon);
            return Show(owner, opts);
        }


        // Assíncrono (útil se você quiser não bloquear um fluxo async)
        public static Task<(MyMessageBoxResult Result, bool DoNotAskAgain)> ShowAsync(Window? owner, MyMessageBoxOptions options)
        {
            var tcs = new TaskCompletionSource<(MyMessageBoxResult, bool)>();
            var win = new MyMessageBoxWindow(options)
            {
                Owner = owner,
                WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
            win.Dispatcher.BeginInvoke(() =>
            {
                bool? _ = win.ShowDialog();
                tcs.TrySetResult((win.Result, win.IsDoNotAskAgainChecked));
            });
            return tcs.Task;
        }
    }
}
