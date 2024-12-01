using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Nezumi.Utilities;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS;

namespace Nezumi;

/// <summary>
/// A background task that tracks the cursor and focuses windows based on the cursor's position.
/// </summary>
/// <param name="interval">The interval at which the task should run.</param>
public class CursorTrackerBackgroundTask(TimeSpan interval)
{
    private Task? _timerTask;
    private readonly PeriodicTimer _timer = new(interval);
    private readonly CancellationTokenSource _cts = new();

    // TODO: read these from a config file

    // alsways allow these apps to be focused
    private readonly List<string> _classAllowlist =
    [
        "Chrome_RenderWidgetHostHWND", // gross electron apps
        "Microsoft.UI.Content.DesktopChildSiteBridge", // windows explorer main panel
        "SysTreeView32", // windows explorer side panel
        "TITLE_BAR_SCAFFOLDING_WINDOW_CLASS", // windows explorer side panel
        "DirectUIHWND", // windows explorer after interaction
        "CabinetWClass" // windows explorer
    ];

    // never allow these apps to be focused
    private readonly List<string> _classBlocklist =
    [
        "SHELLDLL_DefView", // desktop window
        "Shell_TrayWnd", // tray
        "TrayNotifyWnd", // tray
        "MSTaskSwWClass", // start bar icons
        "Windows.UI.Core.CoreWindow" // start menu
    ];

    // special windows that might pop up in the middle of the screen and be focused already - ie. power launcher
    private readonly List<string> _classSpecialPause =
    [
        "HwndWrapper[PowerToys.PowerLauncher;;911a26fb-a3ec-45c8-b3bc-60ca27f6b291]"
    ];

    public void Start()
    {
        _timerTask = TrackCursorAsync();
    }

    public async Task StopAsync()
    {
        if (_timerTask is null)
        {
            return;
        }

        await _cts.CancelAsync();
        await _timerTask;
        _cts.Dispose();
    }

    /// <summary>
    /// Iterates a loop that tracks the cursor and focuses windows based on the cursor's position.
    /// </summary>
    private async Task TrackCursorAsync()
    {
        var foregroundClassNameString = string.Empty;

        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                unsafe
                {
                    if (foregroundClassNameString is null)
                    {
                        // TODO: add logging here
                        continue;
                    }

                    var success = GetCursorPos(out var point);

                    if (!success)
                    {
                        Console.WriteLine("Failed to get cursor position");
                        Environment.Exit(1);
                    }

                    var hwnd = WindowFromPoint(point);

                    var cursorClassNameString = Cache.Get(hwnd);

                    if (cursorClassNameString is null)
                    {
                        // TODO: add logging here
                        continue;
                    }

                    // If the cursor is above the currently focused window, we don't need to do any further processing.
                    if (hwnd == GetForegroundWindow())
                    {
                        continue;
                    }

                    // If the cursor is above a special window, we don't need to do any further processing - skipping
                    // this will immediately unfocus whatever the special window is, possibly closing it.
                    if (_classSpecialPause.Contains(foregroundClassNameString))
                    {
                        continue;
                    }

                    // Special cases for windows explorer (Thanks LGUG2Z for figuring this out: https://github.com/LGUG2Z/masir/blob/master/src/main.rs)
                    switch (cursorClassNameString)
                    {
                        case "DirectUIHWND" when foregroundClassNameString == "CabinetWClass":
                        case "Microsoft.UI.Content.DesktopChildSiteBridge" when
                            foregroundClassNameString == "CabinetWClass":
                            continue;
                    }

                    if (_classBlocklist.Contains(cursorClassNameString))
                    {
                        continue;
                    }

                    if (!_classAllowlist.Contains(cursorClassNameString) &&
                        !WindowUtilities.GetKomorebiManagedWindowsAsync().Result.Contains(cursorClassNameString))
                    {
                        continue;
                    }

                    // Send a random input to this process to allow us to focus subsequent windows.
                    SendInput(new Span<INPUT>([new INPUT() { type = INPUT_TYPE.INPUT_MOUSE }]), sizeof(INPUT));

                    if (!success)
                    {
                        continue;
                    }

                    foregroundClassNameString = Cache.Get(GetForegroundWindow());

                    SetWindowPos(hwnd, new HWND(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    SetForegroundWindow(hwnd);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // do nothing
        }
    }
}