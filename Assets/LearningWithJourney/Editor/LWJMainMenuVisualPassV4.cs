#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuVisualPassV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Apply Reference Visual Pass V4")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            RemoveOldLogo();
            BuildLogo(canvas.transform);
            ResizeJourney();
            ResizeSupportingElements();
            SharpenAllText(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Reference Visual Pass V4 applied. Journey is larger, the logo has been rebuilt with cleaner spacing, and all TextMesh Pro labels have been sharpened.",
                "OK");
        }

        static void RemoveOldLogo()
        {
            string[] names =
            {
                "LogoShadow", "LogoOutline", "LogoTitle",
                "LearningLogoCrisp", "WithLogoCrisp", "JourneyLogoCrisp",
                "LogoV4LearningShadow", "LogoV4Learning", "LogoV4WithRibbon",
                "LogoV4With", "LogoV4JourneyShadow", "LogoV4Journey", "LogoV4Crown"
            };

            foreach (var name in names)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static void BuildLogo(Transform parent)
        {
            // One clean shadow per major word instead of several overlapping copies.
            CreateText(parent, "LogoV4LearningShadow", "Learning", 108f, FontStyles.Bold,
                Hex("35104F"), new Vector2(.152f, .842f), new Vector2(.952f, .908f), new Vector2(0f, -5f));

            var learning = CreateText(parent, "LogoV4Learning", "Learning", 108f, FontStyles.Bold,
                Color.white, new Vector2(.15f, .847f), new Vector2(.95f, .913f), Vector2.zero);
            learning.outlineColor = Hex("4C126F");
            learning.outlineWidth = .20f;
            learning.characterSpacing = -1.0f;

            var ribbon = CreatePanel(parent, "LogoV4WithRibbon", new Vector2(.36f, .807f), new Vector2(.69f, .845f), Hex("5D168B"));
            AddShadow(ribbon.gameObject, new Vector2(0f, -5f), Hex("2C0A45", .55f));
            AddOutline(ribbon.gameObject, Hex("F3C4FF"), new Vector2(2f, -2f));

            var with = CreateText(ribbon.transform, "LogoV4With", "with", 34f, FontStyles.Bold,
                Color.white, new Vector2(.08f, .08f), new Vector2(.92f, .92f), Vector2.zero);
            with.outlineColor = Hex("4A0E6C");
            with.outlineWidth = .10f;

            CreateText(parent, "LogoV4JourneyShadow", "Journey", 124f, FontStyles.Bold | FontStyles.Italic,
                Hex("541059"), new Vector2(.122f, .745f), new Vector2(.972f, .823f), new Vector2(0f, -6f));

            var journey = CreateText(parent, "LogoV4Journey", "Journey", 124f, FontStyles.Bold | FontStyles.Italic,
                Hex("F13E94"), new Vector2(.12f, .751f), new Vector2(.97f, .829f), Vector2.zero);
            journey.outlineColor = Color.white;
            journey.outlineWidth = .18f;
            journey.characterSpacing = -2f;

            var crown = CreateText(parent, "LogoV4Crown", "♛", 52f, FontStyles.Bold,
                Hex("FFD449"), new Vector2(.78f, .866f), new Vector2(.89f, .924f), Vector2.zero);
            crown.outlineColor = Hex("8D3B16");
            crown.outlineWidth = .10f;

            var tagline = GameObject.Find("TaglineRibbon");
            if (tagline != null && tagline.transform is RectTransform tagRect)
            {
                tagRect.anchorMin = new Vector2(.31f, .705f);
                tagRect.anchorMax = new Vector2(.79f, .747f);
                tagRect.offsetMin = Vector2.zero;
                tagRect.offsetMax = Vector2.zero;
            }
        }

        static void ResizeJourney()
        {
            var journey = GameObject.Find("JourneyCharacter");
            if (journey != null && journey.transform is RectTransform rect)
            {
                rect.anchorMin = new Vector2(.005f, .265f);
                rect.anchorMax = new Vector2(.525f, .735f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;

                var raw = journey.GetComponent<RawImage>();
                if (raw != null)
                {
                    raw.color = Color.white;
                    raw.canvasRenderer.SetAlpha(1f);
                    raw.material = null;
                    if (raw.texture is Texture2D tex)
                    {
                        tex.wrapMode = TextureWrapMode.Clamp;
                        tex.filterMode = FilterMode.Bilinear;
                        tex.anisoLevel = 1;
                    }
                }
            }

            var rug = GameObject.Find("JourneyRug");
            if (rug != null && rug.transform is RectTransform rugRect)
            {
                rugRect.anchorMin = new Vector2(.015f, .255f);
                rugRect.anchorMax = new Vector2(.505f, .545f);
                rugRect.offsetMin = Vector2.zero;
                rugRect.offsetMax = Vector2.zero;
            }

            var bubble = GameObject.Find("JourneySpeechBubble");
            if (bubble != null && bubble.transform is RectTransform bubbleRect)
            {
                bubbleRect.anchorMin = new Vector2(.245f, .565f);
                bubbleRect.anchorMax = new Vector2(.605f, .685f);
                bubbleRect.offsetMin = Vector2.zero;
                bubbleRect.offsetMax = Vector2.zero;
            }
        }

        static void ResizeSupportingElements()
        {
            var gamePanel = GameObject.Find("GamePanel");
            if (gamePanel != null && gamePanel.transform is RectTransform gameRect)
            {
                gameRect.anchorMin = new Vector2(.49f, .275f);
                gameRect.anchorMax = new Vector2(.98f, .69f);
                gameRect.offsetMin = Vector2.zero;
                gameRect.offsetMax = Vector2.zero;
            }

            var start = GameObject.Find("StartBanner");
            if (start != null && start.transform is RectTransform startRect)
            {
                startRect.anchorMin = new Vector2(.17f, .195f);
                startRect.anchorMax = new Vector2(.83f, .265f);
                startRect.offsetMin = Vector2.zero;
                startRect.offsetMax = Vector2.zero;
            }
        }

        static void SharpenAllText(Transform root)
        {
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.enableAutoSizing = false;
                tmp.raycastTarget = false;
                tmp.fontMaterial = tmp.fontMaterial;
                tmp.extraPadding = true;
                tmp.UpdateMeshPadding();
            }
        }

        static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            string value,
            float fontSize,
            FontStyles style,
            Color color,
            Vector2 min,
            Vector2 max,
            Vector2 pixelOffset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = pixelOffset;
            rect.offsetMax = pixelOffset;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            text.extraPadding = true;
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
            text.UpdateMeshPadding();
            return text;
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
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
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
