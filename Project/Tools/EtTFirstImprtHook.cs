#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EtTFirstImportHook
{
    private const string KEY = "EtT_DefaultAssetsGenerated";

    static EtTFirstImportHook()
    {
        if (!SessionState.GetBool(KEY, false))
        {
            // spróbuj wygenerować — jeśli pliki już są, funkcja nic nie zmieni
            EtTDefaultAssetsGenerator.GenerateAll();
            SessionState.SetBool(KEY, true);
            Debug.Log("[EtT] Default assets ensured.");
        }
    }
}
#endif