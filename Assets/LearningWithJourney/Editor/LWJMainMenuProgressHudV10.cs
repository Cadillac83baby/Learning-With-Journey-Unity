#if UNITY_EDITOR
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuProgressHudV10
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";

        static Sprite rounded;

        [MenuItem("Learning with Journey/Restore Level + Points V10")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);

            RemoveOldProgressHud();
            var pointsCount = BuildPointsPill(canvas.transform);
            var levelText = BuildLevelPill(canvas.transform);
            RebindHud(canvas, pointsCount, levelText);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V10 applied: Levels and the Points counter are back in two compact modern pills. The old J/avatar block stays removed.",
                "OK");
        }

        static void RemoveOldProgressHud()
        {
            string[] names =
            {
                "TopHUD", "AvatarRing", "StarPill", "LevelPill", "CoinPill",
                "V10PointsPill", "V10LevelPill"
            };

            foreach (string name in names)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static TMP_Text BuildPointsPill(Transform parent)
        {
            var pill = CreatePanel(parent, "V10PointsPill",
                new Vector2(.035f, .932f), new Vector2(.31f, .982f), Hex("F02C8D"));
            AddShadow(pill.gameObject, new Vector2(0f, -7f), Hex("6E104A", .65f));
            AddOutline(pill.gameObject, Hex("FFD4EE"), new Vector2(3f, -3f));

            var gloss = CreatePanel(pill.transform, "Gloss",
                new Vector2(.035f, .59f), new Vector2(.965f, .91f), new Color(1f, 1f, 1f, .24f));
            gloss.raycastTarget = false;

            var badge = CreatePanel(pill.transform, "Badge",
                new Vector2(.035f, .12f), new Vector2(.27f, .88f), Hex("FFD446"));
            AddShadow(badge.gameObject, new Vector2(0f, -3f), Hex("9C5710", .35f));
            AddOutline(badge.gameObject, Color.white, new Vector2(2f, -2f));

            var star = CreateText(badge.transform, "Star", "★", 30f, FontStyles.Bold, Color.white,
                new Vector2(.05f, .04f), new Vector2(.95f, .96f));
            star.alignment = TextAlignmentOptions.Center;
            star.outlineColor = Hex("B46B10");
            star.outlineWidth = .08f;

            var label = CreateText(pill.transform, "Label", "POINTS", 17f, FontStyles.Bold, Color.white,
                new Vector2(.30f, .50f), new Vector2(.66f, .88f));
            label.alignment = TextAlignmentOptions.Left;

            var count = CreateText(pill.transform, "Count", "0", 29f, FontStyles.Bold, Color.white,
                new Vector2(.64f, .16f), new Vector2(.94f, .86f));
            count.alignment = TextAlignmentOptions.Center;
            count.outlineColor = Hex("8C1558");
            count.outlineWidth = .07f;

            return count;
        }

        static TMP_Text BuildLevelPill(Transform parent)
        {
            var pill = CreatePanel(parent, "V10LevelPill",
                new Vector2(.69f, .932f), new Vector2(.965f, .982f), Hex("6F24B8"));
            AddShadow(pill.gameObject, new Vector2(0f, -7f), Hex("2E0B55", .65f));
            AddOutline(pill.gameObject, Hex("E7C5FF"), new Vector2(3f, -3f));

            var gloss = CreatePanel(pill.transform, "Gloss",
                new Vector2(.035f, .59f), new Vector2(.965f, .91f), new Color(1f, 1f, 1f, .22f));
            gloss.raycastTarget = false;

            var badge = CreatePanel(pill.transform, "Badge",
                new Vector2(.04f, .12f), new Vector2(.25f, .88f), Hex("9D4DE1"));
            AddOutline(badge.gameObject, Hex("F4E7FF"), new Vector2(2f, -2f));

            var crown = CreateText(badge.transform, "Crown", "1", 25f, FontStyles.Bold, Color.white,
                new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            crown.alignment = TextAlignmentOptions.Center;

            var level = CreateText(pill.transform, "LevelText", "Level 1", 23f, FontStyles.Bold, Color.white,
                new Vector2(.28f, .10f), new Vector2(.94f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            level.outlineColor = Hex("45106F");
            level.outlineWidth = .07f;

            return level;
        }

        static void RebindHud(GameObject canvas, TMP_Text pointsCount, TMP_Text levelText)
        {
            var hud = canvas.GetComponent<MainMenuHud>();
            if (hud == null) hud = canvas.AddComponent<MainMenuHud>();

            var so = new SerializedObject(hud);
            so.FindProperty("starText").objectReferenceValue = pointsCount;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("coinText").objectReferenceValue = null;
            so.FindProperty("playerText").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.extraPadding = true;
            return text;
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            var s = go.AddComponent<Shadow>();
            s.effectDistance = distance;
            s.effectColor = color;
            s.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var o = go.AddComponent<Outline>();
            o.effectColor = color;
            o.effectDistance = distance;
            o.useGraphicAlpha = true;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            color.a = alpha;
            return color;
        }
    }
}
#endif
