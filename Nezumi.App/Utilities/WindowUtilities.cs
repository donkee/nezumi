using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Nezumi.Utilities;

public static class WindowUtilities
{
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

    public static async Task<List<string>> GetKomorebiManagedWindows()
    {
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