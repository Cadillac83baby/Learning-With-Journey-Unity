#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.EditorTools
{
    /// <summary>Moves only the existing reader bookbag over the shorts spot.</summary>
    public static class LWJBookReaderBookbagPlacementV6
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";

        [MenuItem("Learning with Journey/Fix Journey Shorts Bookbag V6")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode first.", "OK");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                EditorUtility.DisplayDialog("Learning with Journey",
                    "Open your existing BookReader scene, then run this command. No changes were made.", "OK");
                return;
            }

            RectTransform bag = Find(scene, "JourneyBackpack");
            RectTransform journey = Find(scene, "Journey");
            if (bag == null || journey == null || bag.parent != journey.parent ||
                bag.parent == null || bag.parent.GetComponent<Canvas>() == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey",
                    "Expected Journey and JourneyBackpack objects directly under the reader Canvas. No changes were made.", "OK");
                return;
            }

            Undo.RecordObject(bag, "Move bookbag over Journey shorts");
            // Screenshot reference: 1080x1920 canvas. Shift V4's bag left
            // by 6.5% of the canvas, preserving its size and height.
            // Bag ends at 29%, just before the open book begins at 29.5%.
            bag.anchorMin = new Vector2(.19f, .255f);
            bag.anchorMax = new Vector2(.29f, .345f);
            bag.offsetMin = Vector2.zero;
            bag.offsetMax = Vector2.zero;
            bag.localRotation = Quaternion.identity;
            bag.localScale = Vector3.one;
            // Preserve existing V4 sibling order, which already draws the bag over Journey.
            PrefabUtility.RecordPrefabInstancePropertyModifications(bag);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = bag.gameObject;
            EditorUtility.DisplayDialog("Learning with Journey",
                "The bookbag has moved left over the shorts spot. Check the Game view, then Ctrl+S to save. Undo restores its previous position.", "OK");
        }

        static RectTransform Find(Scene scene, string objectName)
        {
            RectTransform match = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                    if (rect.name == objectName)
                    {
                        if (match != null) return null;
                        match = rect;
                    }
            return match;
        }
    }
}
#endif
