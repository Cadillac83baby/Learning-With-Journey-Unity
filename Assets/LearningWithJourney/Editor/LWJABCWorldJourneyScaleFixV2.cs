#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldJourneyScaleFixV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";

        [MenuItem("Learning with Journey/Fix ABC Journey Scale + Placement V2")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "ABCWorld.unity was not found. Build ABC World V1 first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var journey = GameObject.Find("Journey");
            if (journey == null || journey.transform is not RectTransform journeyRect)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Journey was not found in ABC World.", "OK");
                return;
            }

            // The V1 ABC builder added an AspectRatioFitter, which can resize Journey far beyond
            // the intended portrait bounds. Counting World does not use one, so remove it here.
            var fitter = journey.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                Object.DestroyImmediate(fitter);

            // Match the approved Counting World character footprint.
            journeyRect.anchorMin = new Vector2(.015f, .31f);
            journeyRect.anchorMax = new Vector2(.42f, .69f);
            journeyRect.offsetMin = Vector2.zero;
            journeyRect.offsetMax = Vector2.zero;
            journeyRect.localScale = Vector3.one;
            journeyRect.localRotation = Quaternion.identity;

            var raw = journey.GetComponent<RawImage>();
            if (raw != null)
            {
                raw.raycastTarget = false;
                raw.color = Color.white;
            }

            // Keep Journey behind the learning board like Counting World.
            var activityCard = GameObject.Find("ABCActivityCard");
            if (activityCard != null && activityCard.transform.parent == journey.transform.parent)
            {
                int cardIndex = activityCard.transform.GetSiblingIndex();
                journey.transform.SetSiblingIndex(Mathf.Max(0, cardIndex - 1));
            }

            // Preserve the approved backpack cover position over Journey's damaged screen-right leg.
            var bag = GameObject.Find("JourneyBackpack");
            if (bag != null && bag.transform is RectTransform bagRect)
            {
                bagRect.anchorMin = new Vector2(.255f, .335f);
                bagRect.anchorMax = new Vector2(.355f, .425f);
                bagRect.offsetMin = Vector2.zero;
                bagRect.offsetMax = Vector2.zero;
                bagRect.localScale = Vector3.one;
                bagRect.localRotation = Quaternion.identity;

                if (bag.transform.parent == journey.transform.parent)
                {
                    int desired = Mathf.Min(journey.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1);
                    bag.transform.SetSiblingIndex(desired);
                }
            }

            // Keep the speech bubble beside Journey and above the other ABC UI.
            var bubble = GameObject.Find("JourneyABCBubble");
            if (bubble != null && bubble.transform is RectTransform bubbleRect)
            {
                bubbleRect.anchorMin = new Vector2(.045f, .690f);
                bubbleRect.anchorMax = new Vector2(.385f, .770f);
                bubbleRect.offsetMin = Vector2.zero;
                bubbleRect.offsetMax = Vector2.zero;
                bubbleRect.localScale = Vector3.one;
                bubble.transform.SetAsLastSibling();

                var bubbleImage = bubble.GetComponent<Image>();
                if (bubbleImage != null) bubbleImage.raycastTarget = false;

                var speech = bubble.transform.Find("SpeechText")?.GetComponent<TMP_Text>();
                if (speech != null)
                {
                    speech.fontSize = 22f;
                    speech.alignment = TextAlignmentOptions.Center;
                    speech.raycastTarget = false;
                    speech.margin = new Vector4(14f, 8f, 14f, 8f);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V2 fixed Journey's scale and placement. Journey now matches the approved Counting World size, stays behind the activity board, the backpack remains over the damaged shorts area, and the speech bubble stays beside her.",
                "OK");
        }
    }
}
#endif
