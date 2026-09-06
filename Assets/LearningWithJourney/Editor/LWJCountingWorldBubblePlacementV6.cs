#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldBubblePlacementV6
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";

        [MenuItem("Learning with Journey/Position Counting Speech Bubble V6")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "CountingWorld.unity was not found.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var bubble = GameObject.Find("JourneyCountingBubble");

            if (bubble == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "JourneyCountingBubble was not found.", "OK");
                return;
            }

            var rect = bubble.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Keep the bubble completely on Journey's side of the screen.
                // The activity card begins near x=.40, so this max x=.385 leaves a clean gap.
                rect.anchorMin = new Vector2(.045f, .690f);
                rect.anchorMax = new Vector2(.385f, .770f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            // Render the bubble above all other Counting World UI without moving Journey
            // or her backpack.
            bubble.transform.SetAsLastSibling();

            var image = bubble.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;

            var text = bubble.transform.Find("SpeechText")?.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.raycastTarget = false;
                text.fontSize = 22f;
                text.alignment = TextAlignmentOptions.Center;
                text.margin = new Vector4(14f, 8f, 14f, 8f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting speech bubble repositioned. It is now fully beside Journey, clear of the game board, and still renders on top. Journey and the backpack were not changed.",
                "OK");
        }
    }
}
#endif
