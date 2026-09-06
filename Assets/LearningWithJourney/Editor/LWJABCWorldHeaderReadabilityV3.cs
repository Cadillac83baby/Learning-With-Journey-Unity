#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldHeaderReadabilityV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";

        [MenuItem("Learning with Journey/Polish ABC Header + Text V3")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "ABCWorld.unity was not found. Build ABC World V1 first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            MoveRainbowDown();
            FixHeader();
            ImproveSmallText();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V3 applied. The title now sits clearly above the rainbow, the rainbow has been lowered slightly, and the smaller instructional/progress text has been enlarged and given stronger contrast for easier reading. Journey, the backpack, activity board, and answer buttons were not moved.",
                "OK");
        }

        static void MoveRainbowDown()
        {
            SetAnchors("RainbowOuter", new Vector2(.36f, .655f), new Vector2(.78f, .835f));
            SetAnchors("Rainbow2", new Vector2(.385f, .670f), new Vector2(.755f, .820f));
            SetAnchors("Rainbow3", new Vector2(.41f, .685f), new Vector2(.73f, .805f));
            SetAnchors("Rainbow4", new Vector2(.435f, .700f), new Vector2(.705f, .790f));
            SetAnchors("Rainbow5", new Vector2(.46f, .715f), new Vector2(.68f, .775f));
            SetAnchors("RainbowCutout", new Vector2(.485f, .727f), new Vector2(.655f, .765f));
        }

        static void FixHeader()
        {
            var title = GameObject.Find("ABCTitle")?.GetComponent<TMP_Text>();
            if (title != null)
            {
                var rect = title.rectTransform;
                rect.anchorMin = new Vector2(.19f, .885f);
                rect.anchorMax = new Vector2(.81f, .925f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                title.fontSize = 48f;
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                title.color = Color.white;
                title.outlineColor = Hex("3D236F");
                title.outlineWidth = .20f;
                title.enableAutoSizing = false;
            }

            var subtitle = GameObject.Find("ABCSubtitle")?.GetComponent<TMP_Text>();
            if (subtitle != null)
            {
                var rect = subtitle.rectTransform;
                rect.anchorMin = new Vector2(.24f, .846f);
                rect.anchorMax = new Vector2(.76f, .878f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                subtitle.fontSize = 24f;
                subtitle.fontStyle = FontStyles.Bold;
                subtitle.alignment = TextAlignmentOptions.Center;
                subtitle.color = Hex("44206F");
                subtitle.outlineColor = new Color(1f, 1f, 1f, .82f);
                subtitle.outlineWidth = .10f;
                subtitle.enableAutoSizing = false;
            }
        }

        static void ImproveSmallText()
        {
            var feedback = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
            if (feedback != null)
            {
                feedback.fontSize = 24f;
                feedback.fontStyle = FontStyles.Bold;
                feedback.color = Hex("4B236F");
                feedback.outlineColor = new Color(1f, 1f, 1f, .85f);
                feedback.outlineWidth = .08f;
            }

            var round = GameObject.Find("RoundText")?.GetComponent<TMP_Text>();
            if (round != null)
            {
                round.fontSize = 22f;
                round.fontStyle = FontStyles.Bold;
                round.color = Hex("4B236F");
                round.outlineColor = new Color(1f, 1f, 1f, .85f);
                round.outlineWidth = .08f;
            }

            var instruction = GameObject.Find("AnswerInstruction")?.GetComponent<TMP_Text>();
            if (instruction != null)
            {
                instruction.fontSize = 26f;
                instruction.fontStyle = FontStyles.Bold;
                instruction.color = Hex("4B236F");
                instruction.outlineColor = new Color(1f, 1f, 1f, .90f);
                instruction.outlineWidth = .08f;
            }

            var word = GameObject.Find("WordText")?.GetComponent<TMP_Text>();
            if (word != null)
            {
                word.fontSize = 30f;
                word.fontStyle = FontStyles.Bold;
                word.color = Hex("4B236F");
            }

            var level = GameObject.Find("LevelPill")?.transform.Find("Level")?.GetComponent<TMP_Text>();
            if (level != null)
                level.fontSize = 20f;

            var points = GameObject.Find("PointsPill")?.transform.Find("Count")?.GetComponent<TMP_Text>();
            if (points != null)
                points.fontSize = 27f;
        }

        static void SetAnchors(string name, Vector2 min, Vector2 max)
        {
            var go = GameObject.Find(name);
            if (go == null || go.transform is not RectTransform rect) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
