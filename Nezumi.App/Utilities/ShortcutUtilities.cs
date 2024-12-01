using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Nezumi.Utilities;

public static class ShortcutUtilities
{
    private static readonly string StartupFolder = $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
    
    public static void CreateShortcut()
    {
        var link = (IShellLink)new ShellLink();

        // setup shortcut information
        link.SetDescription("Nezumi, the focus-follows-mouse solution.");
        link.SetPath($@"{System.AppContext.BaseDirectory}\Nezumi.exe");

        // save it
        var file = (IPersistFile)link;
        file.Save(Path.Combine(StartupFolder, "Nezumi.lnk"), false);
    }
    
    public static void RemoveShortcut()
    {
        File.Delete(Path.Combine(StartupFolder, "Nezumi.lnk"));
    }
    
    public static bool DoesShortcutExist()
    {
        return File.Exists(Path.Combine(StartupFolder, "Nezumi.lnk"));
    }
}

[ComImport]
[Guid("00021401-0000-0000-C000-000000000046")]
internal class ShellLink
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F9-0000-0000-C000-000000000046")]
internal interface IShellLink
{
    void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
    void GetIDList(out IntPtr ppidl);
    void SetIDList(IntPtr pidl);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
    void GetHotkey(out short pwHotkey);
    void SetHotkey(short wHotkey);
    void GetShowCmd(out int piShowCmd);
    void SetShowCmd(int iShowCmd);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
    void Resolve(IntPtr hwnd, int fFlags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
}