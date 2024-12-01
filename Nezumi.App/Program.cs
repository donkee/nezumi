using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Nezumi;
using H.NotifyIcon.Core;
using Nezumi.Utilities;
using static Windows.Win32.PInvoke;

var task = new BackgroundTask(TimeSpan.FromMilliseconds(1));

await using var iconStream = new H.Resource("mouse.ico").AsStream();
using var icon = new Icon(iconStream);
using var trayIcon = new TrayIconWithContextMenu
{
    Icon = icon.Handle,
    ToolTip = "Nezumi",
};

var autostartItem = new PopupMenuItem
{
    Text = "Autostart",
    Checked = ShortcutUtilities.DoesShortcutExist()
};
autostartItem.Click += (_, _) =>
{
    if (ShortcutUtilities.DoesShortcutExist())
    {
        ShortcutUtilities.RemoveShortcut();
        autostartItem.Checked = false;
    }
    else
    {
        ShortcutUtilities.CreateShortcut();
        autostartItem.Checked = true;
    }
};

trayIcon.ContextMenu = new PopupMenu()
{
    Items =
    {
        autostartItem,
        new PopupMenuSeparator(),
        new PopupMenuItem("Exit", (_, _) =>
        {
            task.StopAsync();
            Environment.Exit(0);
        })
    }
};

trayIcon.Create();

task.Start();

var consoleWindow = GetCurrentProcess();
ShowWindow(new HWND(consoleWindow), SHOW_WINDOW_CMD.SW_HIDE);
new ManualResetEvent(false).WaitOne();

await task.StopAsync();