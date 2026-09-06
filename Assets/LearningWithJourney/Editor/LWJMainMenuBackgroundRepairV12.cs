#if UNITY_EDITOR
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuBackgroundRepairV12
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";

        static Sprite rounded;

        [MenuItem("Learning with Journey/Repair Background + Restore Progress V12")]
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

            FixV11BackgroundLayering();
            RestoreVisibleBookshelf();
            RemoveOldBackgroundFragments();
            ReplaceUnsupportedDecorCharacters();
            RestoreProgressHud(canvas);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V12 repaired the classroom layering, restored the dimensional bookshelf, and returned the Points and Level counters to the top of the screen.",
                "OK");
        }

        static void FixV11BackgroundLayering()
        {
            var root = GameObject.Find("V11BackgroundRoot");
            if (root == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The V11 classroom root was not found. Apply the V11 classroom background first, then run V12.",
                    "OK");
                return;
            }

            // V11 originally placed the dimensional set as the first Canvas sibling.
            // That put it behind the opaque ClassroomWall/Floor images. Move it directly
            // after the base wall/floor but before Journey, the logo, panels and buttons.
            var wall = GameObject.Find("ClassroomWall");
            var floor = GameObject.Find("Floor");

            int targetIndex = 0;
            if (wall != null) targetIndex = Mathf.Max(targetIndex, wall.transform.GetSiblingIndex() + 1);
            if (floor != null) targetIndex = Mathf.Max(targetIndex, floor.transform.GetSiblingIndex() + 1);

            root.transform.SetSiblingIndex(targetIndex);

            // Keep this entire root non-interactive so it can never block taps.
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        static void RestoreVisibleBookshelf()
        {
            var shelf = GameObject.Find("V11Shelf");
            if (shelf != null)
            {
                shelf.SetActive(true);
                SetRect(shelf, new Vector2(.755f, .665f), new Vector2(.985f, .89f));
                shelf.transform.SetAsLastSibling();
            }

            var shadow = GameObject.Find("V11ShelfShadow");
            if (shadow != null)
            {
                shadow.SetActive(true);
                SetRect(shadow, new Vector2(.758f, .655f), new Vector2(.988f, .88f));
                shadow.transform.SetAsLastSibling();
                shadow.transform.SetSiblingIndex(Mathf.Max(0, shadow.transform.GetSiblingIndex() - 1));
            }
        }

        static void RemoveOldBackgroundFragments()
        {
            // Hiding only the Image on these old containers left their children visible.
            // Disable the entire legacy objects so the V11 window/shelf are the only versions shown.
            string[] legacy = { "Window", "Bookshelf" };
            foreach (string name in legacy)
            {
                var go = GameObject.Find(name);
                if (go != null) go.SetActive(false);
            }
        }

        static void ReplaceUnsupportedDecorCharacters()
        {
            // Liberation Sans SDF in the current project does not contain some decorative glyphs.
            // Use safe ASCII decoration so the Console stays clean.
            string[] starNames = { "V11StarL", "V11StarR" };
            foreach (string name in starNames)
            {
                var go = GameObject.Find(name);
                if (go == null) continue;
                var text = go.GetComponent<TextMeshProUGUI>();
                if (text != null) text.text = "*";
            }

            var heart = GameObject.Find("V11HeartL");
            if (heart != null)
            {
                var text = heart.GetComponent<TextMeshProUGUI>();
                if (text != null) text.text = "+";
            }
        }

        static void RestoreProgressHud(GameObject canvas)
        {
            string[] remove =
            {
                "TopHUD", "AvatarRing", "StarPill", "LevelPill", "CoinPill",
                "V10PointsPill", "V10LevelPill", "V12PointsPill", "V12LevelPill"
            };

            foreach (string name in remove)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }

            var pointsCount = BuildPointsPill(canvas.transform);
            var levelText = BuildLevelPill(canvas.transform);

            // Always render progress above the background and logo artwork.
            pointsCount.transform.parent.parent.SetAsLastSibling();
            levelText.transform.parent.parent.SetAsLastSibling();

            var hud = canvas.GetComponent<MainMenuHud>();
            if (hud == null) hud = canvas.AddComponent<MainMenuHud>();

            var so = new SerializedObject(hud);
            so.FindProperty("starText").objectReferenceValue = pointsCount;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("coinText").objectReferenceValue = null;
            so.FindProperty("playerText").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TMP_Text BuildPointsPill(Transform parent)
        {
            var pill = CreatePanel(parent, "V12PointsPill",
                new Vector2(.035f, .942f), new Vector2(.315f, .988f), Hex("F02C8D"));
            AddShadow(pill.gameObject, new Vector2(0f, -7f), Hex("6E104A", .65f));
            AddOutline(pill.gameObject, Hex("FFD4EE"), new Vector2(3f, -3f));

            var gloss = CreatePanel(pill.transform, "Gloss",
                new Vector2(.03f, .60f), new Vector2(.97f, .92f), new Color(1f, 1f, 1f, .24f));
            gloss.raycastTarget = false;

            var badge = CreatePanel(pill.transform, "Badge",
                new Vector2(.035f, .12f), new Vector2(.29f, .88f), Hex("FFD446"));
            AddShadow(badge.gameObject, new Vector2(0f, -3f), Hex("9C5710", .35f));
            AddOutline(badge.gameObject, Color.white, new Vector2(2f, -2f));

            var badgeText = CreateText(badge.transform, "BadgeText", "PTS", 17f, FontStyles.Bold, Color.white,
                new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.outlineColor = Hex("9C5710");
            badgeText.outlineWidth = .07f;

            var label = CreateText(pill.transform, "Label", "POINTS", 16f, FontStyles.Bold, Color.white,
                new Vector2(.32f, .50f), new Vector2(.68f, .88f));
            label.alignment = TextAlignmentOptions.Left;

            var count = CreateText(pill.transform, "Count", "0", 28f, FontStyles.Bold, Color.white,
                new Vector2(.66f, .12f), new Vector2(.95f, .88f));
            count.alignment = TextAlignmentOptions.Center;
            count.outlineColor = Hex("8C1558");
            count.outlineWidth = .07f;

            return count;
        }

        static TMP_Text BuildLevelPill(Transform parent)
        {
            var pill = CreatePanel(parent, "V12LevelPill",
                new Vector2(.685f, .942f), new Vector2(.965f, .988f), Hex("6F24B8"));
            AddShadow(pill.gameObject, new Vector2(0f, -7f), Hex("2E0B55", .65f));
            AddOutline(pill.gameObject, Hex("E7C5FF"), new Vector2(3f, -3f));

            var gloss = CreatePanel(pill.transform, "Gloss",
                new Vector2(.03f, .60f), new Vector2(.97f, .92f), new Color(1f, 1f, 1f, .22f));
            gloss.raycastTarget = false;

            var badge = CreatePanel(pill.transform, "Badge",
                new Vector2(.04f, .12f), new Vector2(.29f, .88f), Hex("9D4DE1"));
            AddOutline(badge.gameObject, Hex("F4E7FF"), new Vector2(2f, -2f));

            var badgeText = CreateText(badge.transform, "BadgeText", "LVL", 17f, FontStyles.Bold, Color.white,
                new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            badgeText.alignment = TextAlignmentOptions.Center;

            var level = CreateText(pill.transform, "LevelText", "Level 1", 22f, FontStyles.Bold, Color.white,
                new Vector2(.31f, .10f), new Vector2(.95f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            level.outlineColor = Hex("45106F");
            level.outlineWidth = .07f;

            return level;
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

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size,
            FontStyles style, Color color, Vector2 min, Vector2 max)
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

        static void SetRect(GameObject go, Vector2 min, Vector2 max)
        {
            if (go == null || go.transform is not RectTransform rect) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
