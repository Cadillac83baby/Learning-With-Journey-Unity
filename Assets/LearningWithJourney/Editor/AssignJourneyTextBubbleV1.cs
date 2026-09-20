using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class AssignJourneyTextBubbleV1
{
    [MenuItem("Learning With Journey/Assign Updated Text Bubble")]
    public static void Run()
    {
        string scenePath = "Assets/LearningWithJourney/Scenes/Library.unity";
        string spritePath =
            "Assets/LearningWithJourney/Art/Journey/JourneyTextBubbleUpdated.png";

        var scene = EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single);

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);

        int assigned = 0;

        foreach (Transform item in Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            string name = item.name.ToLowerInvariant();

            if (!name.Contains("bubble") &&
                !name.Contains("speech") &&
                !name.Contains("dialog"))
                continue;

            Image image = item.GetComponent<Image>();
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
                assigned++;
                Debug.Log("Updated Image bubble: " + item.name);
            }

            RawImage rawImage = item.GetComponent<RawImage>();
            if (rawImage != null && texture != null)
            {
                rawImage.texture = texture;
                rawImage.color = Color.white;
                assigned++;
                Debug.Log("Updated RawImage bubble: " + item.name);
            }
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Updated text bubbles: " + assigned);
    }
}
