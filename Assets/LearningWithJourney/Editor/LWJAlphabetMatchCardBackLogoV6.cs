#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    /// <summary>
    /// Applies a Learning with Journey branded card back without importing any external image.
    /// This intentionally avoids TextureImporter/SpriteImporter so it is reliable on every machine.
    /// </summary>
    public static class LWJAlphabetMatchCardBackLogoV6
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string RootName = "LWJCardBackLogoV6";

        [MenuItem("Learning with Journey/Apply Logo Card Backs V6 No Sprite")]
        public static void Apply()
        {
            ApplyInternal(true);
        }

        public static void ApplySilently()
        {
            ApplyInternal(false);
        }

        static void ApplyInternal(bool showDialog)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "AlphabetMatchWorld.unity was not found. Build Alphabet Match World first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int updated = 0;

            for (int i = 1; i <= 8; i++)
            {
                var card = GameObject.Find("MatchCard" + i);
                if (card == null) continue;

                var back = card.transform.Find("Back");
                if (back == null) continue;

                TMP_FontAsset font = FindCardFont(back);
                Image backImage = back.GetComponent<Image>();
                Sprite roundedSprite = backImage != null ? backImage.sprite : null;

                // Remove every older temporary/image-based treatment.
                SetChildActive(back, "Question", false);
                SetChildActive(back, "MatchLabel", false);
                SetChildActive(back, "Gloss", false);
                DestroyChild(back, "LearningWithJourneyCardBackLogo");
                DestroyChild(back, RootName);

                if (backImage != null)
                {
                    backImage.color = Hex("6532B8");
                    backImage.raycastTarget = false;
                }

                var root = CreateRect(back, RootName, Vector2.zero, Vector2.one);

                // Bright inset frame that keeps the commercial glossy-card feel.
                var frame = AddImage(root, "LogoFrame", new Vector2(.035f, .035f), new Vector2(.965f, .965f), roundedSprite, Hex("A33BE7"));
                AddOutline(frame.gameObject, Hex("F6B9FF"), new Vector2(3f, -3f));

                var inner = AddImage(frame.transform, "LogoInner", new Vector2(.045f, .045f), new Vector2(.955f, .955f), roundedSprite, Hex("6E34C2"));
                AddOutline(inner.gameObject, Hex("FFFFFF", .55f), new Vector2(2f, -2f));

                // Soft top shine.
                var shine = AddImage(inner.transform, "TopShine", new Vector2(.06f, .70f), new Vector2(.94f, .93f), roundedSprite, Hex("FFFFFF", .12f));
                shine.raycastTarget = false;

                // A light center badge gives the logo enough contrast at all card sizes.
                var badge = AddImage(inner.transform, "LogoBadge", new Vector2(.08f, .18f), new Vector2(.92f, .78f), roundedSprite, Hex("FFFFFF", .94f));
                AddOutline(badge.gameObject, Hex("E7D6FF"), new Vector2(2f, -2f));

                string rainbowLearning =
                    "<color=#FFD83D>L</color>" +
                    "<color=#FF5CA8>e</color>" +
                    "<color=#49D9F5>a</color>" +
                    "<color=#7FE34A>r</color>" +
                    "<color=#FFD83D>n</color>" +
                    "<color=#FF5CA8>i</color>" +
                    "<color=#49D9F5>n</color>" +
                    "<color=#A963F2>g</color>";

                var learning = AddText(badge.transform, "Learning", rainbowLearning, font,
                    new Vector2(.05f, .56f), new Vector2(.95f, .91f), 34f, FontStyles.Bold, Hex("FFFFFF"));
                learning.enableAutoSizing = true;
                learning.fontSizeMin = 15f;
                learning.fontSizeMax = 38f;
                learning.richText = true;
                learning.outlineColor = Hex("4D237D");
                learning.outlineWidth = .10f;

                var with = AddText(badge.transform, "With", "with", font,
                    new Vector2(.18f, .40f), new Vector2(.82f, .61f), 21f, FontStyles.Bold, Hex("6732B0"));
                with.enableAutoSizing = true;
                with.fontSizeMin = 11f;
                with.fontSizeMax = 23f;

                var journey = AddText(badge.transform, "Journey", "Journey", font,
                    new Vector2(.05f, .08f), new Vector2(.95f, .46f), 37f, FontStyles.Bold, Hex("F23693"));
                journey.enableAutoSizing = true;
                journey.fontSizeMin = 16f;
                journey.fontSizeMax = 42f;
                journey.outlineColor = Hex("5B218E");
                journey.outlineWidth = .08f;

                // Small brand-color accents, built from existing UI graphics only.
                AddAccent(inner.transform, "AccentA", new Vector2(.10f, .81f), new Vector2(.20f, .88f), roundedSprite, Hex("FFD83D"));
                AddAccent(inner.transform, "AccentB", new Vector2(.80f, .80f), new Vector2(.90f, .87f), roundedSprite, Hex("FF5CA8"));
                AddAccent(inner.transform, "AccentC", new Vector2(.45f, .09f), new Vector2(.55f, .15f), roundedSprite, Hex("49D9F5"));

                root.SetAsLastSibling();
                updated++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Done. " + updated + " Alphabet Match cards now use the Learning with Journey branded logo back. This V6 version uses only Unity UI and TextMeshPro, so it does not import or depend on any image/Sprite file.",
                    "OK");
            }
        }

        static TMP_FontAsset FindCardFont(Transform back)
        {
            var question = back.Find("Question");
            if (question != null)
            {
                var text = question.GetComponent<TMP_Text>();
                if (text != null && text.font != null) return text.font;
            }

            var anyText = back.GetComponentInChildren<TMP_Text>(true);
            return anyText != null ? anyText.font : TMP_Settings.defaultFontAsset;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        static Image AddImage(Transform parent, string name, Vector2 min, Vector2 max, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TMP_Text AddText(Transform parent, string name, string value, TMP_FontAsset font,
            Vector2 min, Vector2 max, float size, FontStyles style, Color color)
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
            if (font != null) text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        static void AddAccent(Transform parent, string name, Vector2 min, Vector2 max, Sprite sprite, Color color)
        {
            AddImage(parent, name, min, max, sprite, color);
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void SetChildActive(Transform parent, string childName, bool active)
        {
            var child = parent.Find(childName);
            if (child != null) child.gameObject.SetActive(active);
        }

        static void DestroyChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                color.a = alpha;
                return color;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
#endif
