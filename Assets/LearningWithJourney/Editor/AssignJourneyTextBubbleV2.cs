using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Editor
{
    public static class AssignJourneyTextBubbleV2
    {
        [MenuItem("Learning With Journey/Replace Text Bubble In All Scenes")]
        public static void Run()
        {
            string spritePath =
                "Assets/LearningWithJourney/Art/Journey/JourneyTextBubbleUpdated.png";

            TextureImporter importer =
                AssetImporter.GetAtPath(spritePath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError("Bubble image not found: " + spritePath);
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            Sprite bubbleSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            string[] sceneGuids = AssetDatabase.FindAssets(
                "t:Scene",
                new[] { "Assets/LearningWithJourney/Scenes" });

            int updated = 0;

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

                foreach (Transform item in Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None))
                {
                    string name = item.name.ToLowerInvariant();

                    if (!name.Contains("bubble") &&
                        !name.Contains("speech") &&
                        !name.Contains("dialog") &&
                        !name.Contains("prompt"))
                        continue;

                    Image image = item.GetComponent<Image>();

                    if (image != null)
                    {
                        image.sprite = bubbleSprite;
                        image.color = Color.white;
                        image.preserveAspect = true;
                        updated++;

                        Debug.Log(
                            "Updated bubble: " +
                            item.name +
                            " in " +
                            scenePath);
                    }
                }

                EditorSceneManager.SaveScene(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Total text bubbles updated: " + updated);
        }
    }
}
