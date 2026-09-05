#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    [InitializeOnLoad]
    public static class LWJMainMenuPolishFix
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        static LWJMainMenuPolishFix()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.path != ScenePath) return;
            EditorApplication.delayCall += AutoPolishActiveMainMenu;
        }

        [MenuItem("Learning with Journey/Apply Main Menu Polish Fix")]
        public static void ApplyPolish()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyInternal(scene, true);
        }

        static void AutoPolishActiveMainMenu()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath) return;

            // Only auto-repair when the old stacked title exists.
            if (GameObject.Find("LogoTitle") == null) return;
            ApplyInternal(scene, false);
        }

        static void ApplyInternal(Scene scene, bool showDialog)
        {
            var canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "Canvas was not found in MainMenu.", "OK");
                return;
            }

            RemoveOldTitle();
            BuildCrispBrandTitle(canvasGO.transform);
            RepositionTagline();
            HardenJourneyPreview();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Main Menu polish applied: sharper brand title, cleaner spacing, and Journey image rendering fixes.",
                    "OK");
            }
        }

        static void RemoveOldTitle()
        {
            DestroyNamed("LogoShadow");
            DestroyNamed("LogoOutline");
            DestroyNamed("LogoTitle");
            DestroyNamed("LearningLogoCrisp");
            DestroyNamed("WithLogoCrisp");
            DestroyNamed("JourneyLogoCrisp");
        }

        static void BuildCrispBrandTitle(Transform parent)
        {
            var learning = CreateCrispText(
                parent,
                "LearningLogoCrisp",
                "Learning",
                86f,
                FontStyles.Bold,
                Color.white,
                new Vector2(.20f, .842f),
                new Vector2(.90f, .905f));
            learning.outlineColor = Hex("4C126F");
            learning.outlineWidth = 0.18f;
            learning.characterSpacing = -1.5f;

            var with = CreateCrispText(
                parent,
                "WithLogoCrisp",
                "with",
                31f,
                FontStyles.Bold,
                Hex("FFF2A3"),
                new Vector2(.39f, .813f),
                new Vector2(.67f, .844f));
            with.outlineColor = Hex("5B177D");
            with.outlineWidth = 0.14f;

            var journey = CreateCrispText(
                parent,
                "JourneyLogoCrisp",
                "Journey",
                96f,
                FontStyles.Bold | FontStyles.Italic,
                Hex("F13D93"),
                new Vector2(.17f, .765f),
                new Vector2(.92f, .833f));
            journey.outlineColor = Color.white;
            journey.outlineWidth = 0.16f;
            journey.characterSpacing = -2f;
        }

        static TMP_Text CreateCrispText(
            Transform parent,
            string name,
            string value,
            float fontSize,
            FontStyles style,
            Color color,
            Vector2 min,
            Vector2 max)
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
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;

            return text;
        }

        static void RepositionTagline()
        {
            var tagline = GameObject.Find("TaglineRibbon");
            if (tagline == null) return;
            if (tagline.transform is not RectTransform rect) return;

            rect.anchorMin = new Vector2(.31f, .715f);
            rect.anchorMax = new Vector2(.78f, .758f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void HardenJourneyPreview()
        {
            var journey = GameObject.Find("JourneyCharacter");
            if (journey == null) return;

            var raw = journey.GetComponent<RawImage>();
            if (raw != null)
            {
                raw.color = Color.white;
                raw.canvasRenderer.SetAlpha(1f);
                raw.material = null;

                if (raw.texture is Texture2D texture)
                {
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.filterMode = FilterMode.Bilinear;
                    texture.anisoLevel = 1;
                }
            }
        }

        static void DestroyNamed(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Object.DestroyImmediate(go);
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
