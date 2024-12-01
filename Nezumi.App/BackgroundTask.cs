using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Nezumi.Utilities;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS;

namespace Nezumi;

public class BackgroundTask(TimeSpan interval)
{
    private Task? _timerTask;
    private readonly PeriodicTimer _timer = new(interval);
    private readonly CancellationTokenSource _cts = new();

    private readonly List<string> _classAllowlist =
    [
        "Chrome_RenderWidgetHostHWND", // gross electron apps
        "Microsoft.UI.Content.DesktopChildSiteBridge", // windows explorer main panel
        "SysTreeView32", // windows explorer side panel
        "TITLE_BAR_SCAFFOLDING_WINDOW_CLASS", // windows explorer side panel
        "DirectUIHWND", // windows explorer after interaction
        "CabinetWClass" // windows explorer
    ];

    private readonly List<string> _classBlocklist =
    [
        "SHELLDLL_DefView", // desktop window
        "Shell_TrayWnd", // tray
        "TrayNotifyWnd", // tray
        "MSTaskSwWClass", // start bar icons
        "Windows.UI.Core.CoreWindow" // start menu
    ];

    private readonly List<string> _classSpecialPause =
    [
        "HwndWrapper[PowerToys.PowerLauncher;;911a26fb-a3ec-45c8-b3bc-60ca27f6b291]"
    ];

    public void Start()
    {
        _timerTask = DoWorkAsync();
    }

    private async Task DoWorkAsync()
    {
        var foregroundClassNameString = string.Empty;

        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                unsafe
                {
                    var success = GetCursorPos(out var point);

                    if (!success)
                    {
                        Console.WriteLine("Failed to get cursor position");
                        Environment.Exit(1);
                    }

                    var hwnd = WindowFromPoint(point);

                    var cursorClassNameString = hwnd.WindowClassName();

                    if (hwnd == GetForegroundWindow())
                    {
                        continue;
                    }

                    if (_classSpecialPause.Contains(foregroundClassNameString))
                    {
                        continue;
                    }

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
                        !WindowUtilities.GetKomorebiManagedWindows().Result.Contains(cursorClassNameString))
                    {
                        continue;
                    }

                    SendInput(new Span<INPUT>([new INPUT() { type = INPUT_TYPE.INPUT_MOUSE }]), sizeof(INPUT));

                    if (!success)
                    {
                        continue;
                    }

                    foregroundClassNameString = GetForegroundWindow().WindowClassName();
                    SetWindowPos(hwnd, new HWND(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    SetForegroundWindow(hwnd);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
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

        Console.WriteLine("Task was cancelled");
    }
}