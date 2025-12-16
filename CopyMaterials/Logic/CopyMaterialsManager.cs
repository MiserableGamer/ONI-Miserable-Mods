public static class CopyMaterialsManager
{
    public static bool DebugMode = true;

    public static void Log(string message)
    {
        if (DebugMode) Debug.Log($"[CopyMaterials] {message}");
    }

    public static void Warn(string message)
    {
        if (DebugMode) Debug.LogWarning($"[CopyMaterials] {message}");
    }

    public static void Error(string message)
    {
        Debug.LogError($"[CopyMaterials] {message}");
    }

    // … rest of your manager code …
}