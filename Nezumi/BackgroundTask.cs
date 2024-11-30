using Windows.Win32;
using Windows.Win32.Foundation;

namespace Nezumi;

public class BackgroundTask(TimeSpan interval)
{
    private Task? _timerTask;
    private readonly PeriodicTimer _timer = new(interval);
    private readonly CancellationTokenSource _cts = new();

    public void Start()
    {
        _timerTask = DoWorkAsync();
    }

    private async Task DoWorkAsync()
    {
        uint previousThreadId = 0;

        HWND previousHwnd = default;

        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                unsafe
                {
                    var success = PInvoke.GetCursorPos(out var point);

                    if (!success)
                    {
                        Console.WriteLine("Failed to get cursor position");
                        return;
                    }

                    var hwnd = PInvoke.WindowFromPoint(point);

                    if (hwnd == PInvoke.GetForegroundWindow())
                    {
                        continue;
                    }

                    var tid = PInvoke.GetWindowThreadProcessId(hwnd);

                    var currentThreadId = PInvoke.GetCurrentThreadId();

                    if (previousThreadId != 0)
                    {
                        PInvoke.CloseHandle(previousHwnd);
                        previousHwnd = hwnd;
                    }

                    previousThreadId = tid;

                    success = PInvoke.AttachThreadInput(currentThreadId, tid, true);

                    if (success)
                    {
                        PInvoke.SetFocus(hwnd);
                    }
                }
            }
        }
        catch (OperationCanceledException _)
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