using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using System.IO;

/// <summary>
/// Ensures a Universal Render Pipeline asset exists and is assigned
/// to Graphics Settings on every Editor load. Fixes the pink/magenta
/// material issue caused by URP being installed but not activated.
/// </summary>
[InitializeOnLoad]
public static class URPSetup
{
    const string AssetPath = "Assets/Settings/URP-Asset.asset";

    static URPSetup()
    {
        EditorApplication.delayCall += EnsureURPActive;
    }

    static void EnsureURPActive()
    {
        // Already active — nothing to do
        if (GraphicsSettings.defaultRenderPipeline != null &&
            GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
            return;

        // Look for an existing URP asset in the project first
        var existing = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        UniversalRenderPipelineAsset urpAsset = null;

        if (existing.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(existing[0]);
            urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            Debug.Log($"Crash & Build: Found existing URP asset at {path}");
        }

        // None found — create one
        if (urpAsset == null)
        {
            Directory.CreateDirectory(Application.dataPath + "/Settings");
            urpAsset = UniversalRenderPipelineAsset.Create();
            AssetDatabase.CreateAsset(urpAsset, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Crash & Build: Created URP asset at {AssetPath}");
        }

        // Assign as the active render pipeline
        GraphicsSettings.defaultRenderPipeline = urpAsset;

        // Also set quality level overrides so every quality level uses URP
        var qualityLevels = QualitySettings.count;
        for (int i = 0; i < qualityLevels; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
        }
        QualitySettings.renderPipeline = urpAsset;

        // Save the change to ProjectSettings
        AssetDatabase.SaveAssets();
        Debug.Log("Crash & Build: URP activated. Pink materials should now render correctly.");
    }
}
