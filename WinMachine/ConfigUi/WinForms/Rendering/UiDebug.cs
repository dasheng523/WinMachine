using System.Diagnostics;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public static class UiDebug
{
    public static bool Enabled { get; } =
        string.Equals(Environment.GetEnvironmentVariable("WINMACHINE_UI_DEBUG"), "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("WINMACHINE_UI_DEBUG"), "true", StringComparison.OrdinalIgnoreCase);

    public static void Log(string message)
    {
        if (!Enabled) return;
        Trace.WriteLine(message);
        Debug.WriteLine(message);
    }

    public static void Log(Exception ex, string context)
    {
        if (!Enabled) return;
        Trace.WriteLine($"{context}: {ex}");
        Debug.WriteLine($"{context}: {ex}");
    }
}


