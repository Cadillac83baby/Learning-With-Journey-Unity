#if UNITY_EDITOR
using LearningWithJourney.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJJourneyFixedArtInstaller
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string FixedTexturePath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";

        [MenuItem("Learning with Journey/Apply Repaired Journey Character")]
        public static void Apply()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(FixedTexturePath);
            if (texture == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "JourneyMenuCleanFixed.png is not in the project yet. Put it here:\n\n" + FixedTexturePath,
                    "OK");
                return;
            }

            ConfigureTextureImporter(FixedTexturePath);
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(FixedTexturePath);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var journey = GameObject.Find("JourneyCharacter");
            if (journey == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "JourneyCharacter was not found. Run Build Polished Main Menu first.",
                    "OK");
                return;
            }

            var raw = journey.GetComponent<RawImage>();
            if (raw == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "JourneyCharacter has no RawImage component.", "OK");
                return;
            }

            var guard = journey.GetComponent<JourneyCleanTextureGuard>();
            if (guard == null)
                guard = journey.AddComponent<JourneyCleanTextureGuard>();

            guard.Configure(raw, texture);

            var so = new SerializedObject(guard);
            so.FindProperty("characterImage").objectReferenceValue = raw;
            so.FindProperty("cleanTexture").objectReferenceValue = texture;
            so.ApplyModifiedPropertiesWithoutUndo();

            raw.texture = texture;
            raw.uvRect = new Rect(0f, 0f, 1f, 1f);
            raw.color = Color.white;
            raw.material = null;
            raw.canvasRenderer.SetAlpha(1f);

            EditorUtility.SetDirty(journey);
            EditorUtility.SetDirty(guard);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Repaired Journey is installed. This keeps the same character, restores the damaged shorts, and preserves her gentle menu movement without using the damaged atlas pixels.",
                "OK");
        }

        static void ConfigureTextureImporter(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;

            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
}
#endif
