using System.Drawing;
using H.NotifyIcon.Core;

namespace Nezumi.Utilities;

public class TrayIcon:IDisposable
{
    private readonly TrayIconWithContextMenu _trayIcon;
    private readonly Stream _iconStream;
    private readonly Icon _icon;
    private readonly CursorTrackerBackgroundTask _task;
    
    /// <summary>
    /// Initializes the tray icon with the context menu.
    /// </summary>
    /// <param name="task">The cursor tracker background task.</param>
    public TrayIcon(CursorTrackerBackgroundTask task)
    {
        _task = task;
        _iconStream = new H.Resource("mouse.ico").AsStream();
        _icon = new Icon(_iconStream);
        
        _trayIcon = new TrayIconWithContextMenu
        {
            Icon = _icon.Handle,
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
        
        _trayIcon.ContextMenu = new PopupMenu()
        {
            Items =
            {
                autostartItem,
                new PopupMenuSeparator(),
                new PopupMenuItem("Exit", (_, _) =>
                {
                    _task.StopAsync();
                    Environment.Exit(0);
                })
            }
        };
    }

    /// <summary>
    /// Creates the tray icon.
    /// </summary>
    public void Create()
    {
        _trayIcon.Create();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _iconStream.Dispose();
        _icon.Dispose();
        _trayIcon.Dispose();
    }
}