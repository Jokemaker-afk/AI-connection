using UnityEngine;

public static class HandheldToolDebug
{
    public static bool Enabled { get; set; } = true;

    public static void Log(string message, Object context = null)
    {
        if (!Enabled)
        {
            return;
        }

        Debug.Log($"[HandheldTool] {message}", context);
    }
}
