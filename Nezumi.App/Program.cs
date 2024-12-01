using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Nezumi;
using Microsoft.Extensions.Caching.Memory;
using Nezumi.Utilities;
using static Windows.Win32.PInvoke;

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

var task = new CursorTrackerBackgroundTask(TimeSpan.FromMilliseconds(1));

using var trayIcon = new TrayIcon(task);
trayIcon.Create();

task.Start();

// Hide the console window at startup.
var consoleWindow = GetCurrentProcess();
ShowWindow(new HWND(consoleWindow), SHOW_WINDOW_CMD.SW_HIDE);

// This is a hack to keep the program running.
new ManualResetEvent(false).WaitOne();

await task.StopAsync();