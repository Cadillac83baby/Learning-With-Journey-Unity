#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldReadabilityPolishV6
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";

        [MenuItem("Learning with Journey/Polish ABC Text Readability V6")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "ABCWorld.unity was not found. Build ABC World first.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Keep the approved ABC layout intact and only improve phone-size readability.
            PolishText("ABCSubtitle", 25f, 21f, 28f, true);
            PolishText("AnswerInstruction", 28f, 24f, 30f, true);
            PolishText("FeedbackText", 27f, 23f, 30f, true);
            PolishText("RoundText", 24f, 21f, 27f, true);

            var wordText = FindText("WordText");
            if (wordText != null)
            {
                wordText.fontStyle = FontStyles.Bold;
                wordText.enableAutoSizing = true;
                wordText.fontSize = 36f;
                wordText.fontSizeMin = 28f;
                wordText.fontSizeMax = 38f;
                wordText.alignment = TextAlignmentOptions.Center;
                wordText.enableWordWrapping = true;
                wordText.color = Hex("4D286F");

                var r = wordText.rectTransform;
                r.anchorMin = new Vector2(.045f, .035f);
                r.anchorMax = new Vector2(.955f, .205f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            var tapLetter = FindText("TapLetterHint");
            if (tapLetter != null)
            {
                tapLetter.text = "TAP LETTER";
                tapLetter.fontStyle = FontStyles.Bold;
                tapLetter.enableAutoSizing = true;
                tapLetter.fontSize = 22f;
                tapLetter.fontSizeMin = 18f;
                tapLetter.fontSizeMax = 24f;
                tapLetter.alignment = TextAlignmentOptions.Center;
                tapLetter.color = Hex("5A2D91");

                var r = tapLetter.rectTransform;
                r.anchorMin = new Vector2(.04f, .005f);
                r.anchorMax = new Vector2(.96f, .19f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            var tapPicture = FindText("TapHint");
            if (tapPicture != null)
            {
                tapPicture.text = "TAP PICTURE TO HEAR";
                tapPicture.fontStyle = FontStyles.Bold;
                tapPicture.enableAutoSizing = true;
                tapPicture.fontSize = 21f;
                tapPicture.fontSizeMin = 17f;
                tapPicture.fontSizeMax = 23f;
                tapPicture.alignment = TextAlignmentOptions.Center;
                tapPicture.color = Hex("5B397F");

                var r = tapPicture.rectTransform;
                r.anchorMin = new Vector2(.025f, .005f);
                r.anchorMax = new Vector2(.975f, .16f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            var feedback = FindText("FeedbackText");
            if (feedback != null)
            {
                feedback.fontStyle = FontStyles.Bold;
                feedback.enableWordWrapping = true;
                feedback.color = Hex("45256E");

                var r = feedback.rectTransform;
                r.anchorMin = new Vector2(.09f, .145f);
                r.anchorMax = new Vector2(.91f, .205f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            var round = FindText("RoundText");
            if (round != null)
            {
                round.color = Hex("553177");
                var r = round.rectTransform;
                r.anchorMin = new Vector2(.29f, .095f);
                r.anchorMax = new Vector2(.71f, .145f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            // Slightly increase Journey's speech bubble copy without changing the bubble placement.
            var speech = FindText("SpeechText");
            if (speech != null)
            {
                speech.enableAutoSizing = true;
                speech.fontSize = 24f;
                speech.fontSizeMin = 20f;
                speech.fontSizeMax = 26f;
                speech.fontStyle = FontStyles.Bold;
                speech.alignment = TextAlignmentOptions.Center;
                speech.color = Hex("4D2D72");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V6 readability polish is applied. The approved layout and pictures were kept. Small instructional text, A-is-for-word text, level practice text, round text, subtitle, and Journey speech text are now larger and easier to read on a phone.",
                "OK");
        }

        static void PolishText(string objectName, float size, float min, float max, bool bold)
        {
            var text = FindText(objectName);
            if (text == null) return;

            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
            if (bold) text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
        }

        static TMP_Text FindText(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                var direct = go.GetComponent<TMP_Text>();
                if (direct != null) return direct;
            }

            var all = Resources.FindObjectsOfTypeAll<TMP_Text>();
            foreach (var text in all)
            {
                if (text == null || text.gameObject == null) continue;
                if (text.gameObject.scene.path != ScenePath) continue;
                if (text.gameObject.name == objectName) return text;
            }

            return null;
        }

        static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out var c)) return c;
            return Color.white;
        }
    }
}
#endif
