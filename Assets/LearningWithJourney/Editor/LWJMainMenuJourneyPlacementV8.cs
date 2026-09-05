#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuJourneyPlacementV8
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Apply Journey Placement V8")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            MoveBackpackToRightLeg();
            MoveSpeechBubbleOverGameHeading();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Journey Placement V8 applied: the backpack now covers only her right leg, and her speech bubble sits higher in front of the Choose a Game heading and starts hidden until she talks.",
                "OK");
        }

        static void MoveBackpackToRightLeg()
        {
            var bag = GameObject.Find("JourneyVoiceButton");
            if (bag == null || bag.transform is not RectTransform rect) return;

            // Narrower and shifted right so it covers only Journey's screen-right leg.
            rect.anchorMin = new Vector2(.255f, .335f);
            rect.anchorMax = new Vector2(.355f, .425f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;

            // Keep it above Journey, but below the temporary speech bubble.
            bag.transform.SetAsLastSibling();
        }

        static void MoveSpeechBubbleOverGameHeading()
        {
            var bubble = GameObject.Find("JourneySpeechBubble");
            if (bubble == null || bubble.transform is not RectTransform rect) return;

            // Small floating bubble positioned over the top-left of the game panel.
            rect.anchorMin = new Vector2(.365f, .585f);
            rect.anchorMax = new Vector2(.635f, .675f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            // Draw in front of the game panel while Journey is speaking.
            bubble.transform.SetAsLastSibling();

            var image = bubble.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
                image.raycastTarget = false;
            }

            var speech = bubble.transform.Find("SpeechText")?.GetComponent<TextMeshProUGUI>();
            if (speech != null)
            {
                speech.text = "Hi Friend!\nLet's learn together!";
                speech.fontSize = 20f;
                speech.fontStyle = FontStyles.Bold;
                speech.alignment = TextAlignmentOptions.Center;
                speech.enableWordWrapping = true;
                speech.color = Hex("532079");
                speech.outlineWidth = 0f;
                speech.rectTransform.anchorMin = new Vector2(.08f, .10f);
                speech.rectTransform.anchorMax = new Vector2(.92f, .90f);
                speech.rectTransform.offsetMin = Vector2.zero;
                speech.rectTransform.offsetMax = Vector2.zero;
            }

            // The Journey character controller turns this on while talking and off afterward.
            bubble.SetActive(false);
        }

        static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
#endif
