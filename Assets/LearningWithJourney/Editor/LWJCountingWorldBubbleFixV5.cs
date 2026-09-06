#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldBubbleFixV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";

        [MenuItem("Learning with Journey/Fix Counting Speech Bubble V5")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "CountingWorld.unity was not found.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var bubble = GameObject.Find("JourneyCountingBubble");
            var activityCard = GameObject.Find("CountingActivityCard");

            if (bubble == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "JourneyCountingBubble was not found. Run the Counting World V2/V3 setup first.",
                    "OK");
                return;
            }

            var bubbleRect = bubble.GetComponent<RectTransform>();
            if (bubbleRect != null)
            {
                // Keep the bubble close to Journey, but lift it slightly so it does not cover her face.
                bubbleRect.anchorMin = new Vector2(.075f, .685f);
                bubbleRect.anchorMax = new Vector2(.505f, .775f);
                bubbleRect.offsetMin = Vector2.zero;
                bubbleRect.offsetMax = Vector2.zero;
            }

            // Unity UI renders later siblings on top. Move the bubble directly after the
            // activity card so it always appears in front of the game board.
            if (activityCard != null && activityCard.transform.parent == bubble.transform.parent)
            {
                int targetIndex = Mathf.Min(
                    activityCard.transform.GetSiblingIndex() + 1,
                    bubble.transform.parent.childCount - 1);
                bubble.transform.SetSiblingIndex(targetIndex);
            }
            else
            {
                bubble.transform.SetAsLastSibling();
            }

            var bubbleImage = bubble.GetComponent<Image>();
            if (bubbleImage != null)
                bubbleImage.raycastTarget = false;

            var speechText = bubble.transform.Find("SpeechText")?.GetComponent<TMP_Text>();
            if (speechText != null)
            {
                speechText.raycastTarget = false;
                speechText.fontSize = 24f;
                speechText.enableWordWrapping = true;
                speechText.alignment = TextAlignmentOptions.Center;
            }

            // Do not alter Journey or the backpack. The backpack remains in its approved
            // position over the damaged shorts/leg area.

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World speech bubble fixed. It now renders in front of the game board and Journey/backpack placement was left unchanged.",
                "OK");
        }
    }
}
#endif
