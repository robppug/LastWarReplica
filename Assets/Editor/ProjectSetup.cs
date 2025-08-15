#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWR.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        static ProjectSetup()
        {
            EditorApplication.update += RunOnce;
        }

        private static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            try
            {
                EnsureScenes();
                EnsurePlayerSettings();
                TryCreateAndAssignURPAsset(); // retried safely if URP not yet imported
                Debug.Log("[LWR] Project setup complete.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LWR] Setup deferred: " + e.Message);
            }
        }

        private static void EnsureScenes()
        {
            string[] desired = { "Boot","HomeBase","WorldMap","Combat","UINav" };
            string scenesPath = "Assets/Scenes";
            Directory.CreateDirectory(scenesPath);

            foreach (var name in desired)
            {
                string p = $"{scenesPath}/{name}.unity";
                if (!File.Exists(p))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, p);
                }
            }

            var buildScenes = desired
                .Select(n => AssetDatabase.FindAssets($"{n} t:scene").FirstOrDefault())
                .Where(g => !string.IsNullOrEmpty(g))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();

            if (buildScenes.Length > 0)
                EditorBuildSettings.scenes = buildScenes;
        }

        private static void EnsurePlayerSettings()
        {
            // Portrait
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Reference resolution
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;

#if UNITY_ANDROID
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 2); // ARM64
            PlayerSettings.stripEngineCode = true;
#endif

#if UNITY_IOS
            // IL2CPP is default on iOS in 2022.3
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.stripEngineCode = true;
#endif
        }

        // Create a URP asset and assign it once URP is available.
        private static void TryCreateAndAssignURPAsset()
        {
            var urpAssetType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");

            if (urpAssetType == null)
            {
                // URP not imported yet; try again on next editor idle.
                EditorApplication.delayCall += TryCreateAndAssignURPAsset;
                return;
            }

            string settingsDir = "Assets/Settings";
            Directory.CreateDirectory(settingsDir);
            string assetPath = $"{settingsDir}/URP_Asset.asset";

            var existing = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(assetPath);
            if (existing == null)
            {
                var created = ScriptableObject.CreateInstance(urpAssetType) as RenderPipelineAsset;
                AssetDatabase.CreateAsset(created, assetPath);
                AssetDatabase.SaveAssets();
                existing = created;
            }

            // Assign to Graphics and Quality
            if (GraphicsSettings.defaultRenderPipeline != existing)
            {
                GraphicsSettings.defaultRenderPipeline = existing;
            }
            if (QualitySettings.renderPipeline != existing)
            {
                QualitySettings.renderPipeline = existing;
            }

            // Persist changes
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("LWR/Force Setup Now")]
        private static void ForceSetup()
        {
            EnsureScenes();
            EnsurePlayerSettings();
            TryCreateAndAssignURPAsset();
            Debug.Log("[LWR] Forced setup completed.");
        }
    }
}
#endif
