#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    /// <summary>
    /// Applies the approved Learning with Journey artwork to Unity's device icon slots.
    /// The source texture stays in Assets so builds remain reproducible on every machine.
    /// </summary>
    public static class LWJApplyAppIconV1
    {
        const string IconPath = "Assets/LearningWithJourney/Art/Brand/LearningWithJourney_AppIcon.png";

        [MenuItem("Learning with Journey/Apply Learning with Journey App Icon")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode before applying the app icon.", "OK");
                return;
            }

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The app icon asset was not found at:\n" + IconPath, "OK");
                return;
            }

            int applied = 0;
            ApplyToGroup(BuildTargetGroup.Unknown, icon, ref applied);
            ApplyToGroup(BuildTargetGroup.Standalone, icon, ref applied);
            ApplyToGroup(BuildTargetGroup.Android, icon, ref applied);
            ApplyToGroup(BuildTargetGroup.iOS, icon, ref applied);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "App icon applied to " + applied + " Player Settings target groups (default, Windows/Mac/Linux, Android, and iOS).\n\n" +
                "Build or reinstall the app to see the new launcher icon.",
                "OK");
        }

        static void ApplyToGroup(BuildTargetGroup group, Texture2D icon, ref int applied)
        {
            try
            {
#pragma warning disable 0618 // Unity 6 keeps this API for compatibility; the menu remains usable in 6000.3 LTS.
                int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(group);
                if (sizes == null || sizes.Length == 0)
                    sizes = new[] { 1024 };

                Texture2D[] icons = new Texture2D[sizes.Length];
                for (int i = 0; i < icons.Length; i++)
                    icons[i] = icon;

                PlayerSettings.SetIconsForTargetGroup(group, icons);
#pragma warning restore 0618
                applied++;
                Debug.Log("[LearningWithJourney] App icon applied to " + group + " (" + icons.Length + " slots).");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LearningWithJourney] Could not apply app icon to " + group + ": " + exception.Message);
            }
        }
    }
}
#endif
