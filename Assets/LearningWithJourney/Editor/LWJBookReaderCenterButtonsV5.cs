#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.EditorTools
{
    /// <summary>Centers only the existing reader controls; preserves all content and listeners.</summary>
    public static class LWJBookReaderCenterButtonsV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";

        [MenuItem("Learning with Journey/Center Book Reader Bottom Buttons V5")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode before centering the buttons.", "OK");
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                {
                    EditorUtility.DisplayDialog("Learning with Journey", "Open your completed BookReader scene first. No changes were made.", "OK");
                    return;
                }
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            RectTransform previous = Find(scene, "PreviousPage");
            RectTransform readAgain = Find(scene, "ReadAgain");
            RectTransform next = Find(scene, "NextPage");
            if (previous == null || readAgain == null || next == null ||
                previous.parent != readAgain.parent || next.parent != readAgain.parent)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Expected reader buttons were not found under the same parent. No changes were made.", "OK");
                return;
            }

            Undo.RecordObjects(new Object[] { previous, readAgain, next }, "Center Book Reader Bottom Buttons");
            // Keep V3 button widths, spacing and vertical placement.
            // Equal 17% side margins center the complete row on the canvas.
            SetHorizontal(previous, .17f, .385f);
            SetHorizontal(readAgain, .395f, .605f);
            SetHorizontal(next, .615f, .83f);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = readAgain.gameObject;
            EditorUtility.DisplayDialog("Learning with Journey",
                "Bottom buttons are centered. Everything else is unchanged. Save the scene with Ctrl+S when ready; Undo restores the previous positions.", "OK");
        }

        static void SetHorizontal(RectTransform rect, float min, float max)
        {
            rect.anchorMin = new Vector2(min, rect.anchorMin.y);
            rect.anchorMax = new Vector2(max, rect.anchorMax.y);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
            PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
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
