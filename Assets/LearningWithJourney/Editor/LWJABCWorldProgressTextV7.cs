#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldProgressTextV7
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";

        [MenuItem("Learning with Journey/Enlarge ABC Level + Round Text V7")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "ABCWorld.unity was not found.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var feedback = FindText("FeedbackText");
            if (feedback != null)
            {
                feedback.fontStyle = FontStyles.Bold;
                feedback.enableAutoSizing = true;
                feedback.fontSize = 31f;
                feedback.fontSizeMin = 27f;
                feedback.fontSizeMax = 34f;
                feedback.alignment = TextAlignmentOptions.Center;
                feedback.textWrappingMode = TextWrappingModes.Normal;
                feedback.color = Hex("45256E");
                feedback.outlineColor = new Color(1f, 1f, 1f, .88f);
                feedback.outlineWidth = .08f;

                var r = feedback.rectTransform;
                r.anchorMin = new Vector2(.075f, .135f);
                r.anchorMax = new Vector2(.925f, .210f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            var round = FindText("RoundText");
            if (round != null)
            {
                round.fontStyle = FontStyles.Bold;
                round.enableAutoSizing = true;
                round.fontSize = 29f;
                round.fontSizeMin = 25f;
                round.fontSizeMax = 31f;
                round.alignment = TextAlignmentOptions.Center;
                round.color = Hex("553177");
                round.outlineColor = new Color(1f, 1f, 1f, .88f);
                round.outlineWidth = .08f;

                var r = round.rectTransform;
                r.anchorMin = new Vector2(.25f, .080f);
                r.anchorMax = new Vector2(.75f, .145f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V7 is applied. The level practice text and round counter are larger, bolder, and easier to read on a phone. The approved ABC layout, Journey, backpack, pictures, and answer buttons were not changed.",
                "OK");
        }

        static TMP_Text FindText(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                var direct = go.GetComponent<TMP_Text>();
                if (direct != null) return direct;
            }

            foreach (var text in Resources.FindObjectsOfTypeAll<TMP_Text>())
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
