using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Win32.Foundation;
using Nezumi.Models;
using static Windows.Win32.PInvoke;

namespace Nezumi.Utilities;

public static class WindowUtilities
{
    /// <summary>
    /// Gets the class name of a window by its handle.
    /// </summary>
    /// <param name="hwnd">The handle of the window.</param>
    /// <returns>The class name of the window.</returns>
    public static string WindowClassName(this HWND hwnd)
    {
        unsafe
        {
            PWSTR className;
            fixed (char* p = new char[260])
            {
                className = new PWSTR(p);
            }

            var size = GetClassName(hwnd, className, 260);

            var sb = new StringBuilder(size);

            for (var i = 0; i < size; i++)
            {
                sb.Append(className.Value[i]);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Gets the list of Komorebi managed windows.
    /// </summary>
    /// <returns>The list of Komorebi managed windows.</returns>
    public static async Task<List<string>> GetKomorebiManagedWindowsAsync()
    {
        // TODO: use named pipes to communicate with komorebi? read the hwnd file instead? (probably better since it includes hwnds)
        ProcessStartInfo startInfo = new()
        {
            FileName = "komorebic",
            Arguments = "visible-windows",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        var proc = Process.Start(startInfo);

        ArgumentNullException.ThrowIfNull(proc);

        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        var json = JsonNode.Parse(output)!;
        var windows = json[0]!.Deserialize<Window[]>();

        return windows!.Select(w => w.Class).ToList();
    }
}