#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    /// <summary>
    /// Final visual pass for the V2/V3 Book Reader.
    /// Restores Journey's reader backpack position and gives every page illustration
    /// a richer children's-book presentation without replacing the completed 68-page artwork system.
    /// </summary>
    public static class LWJBookReaderArtPolishV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Fix Bookbag + Upgrade Book Art V4")]
        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "BookReader.unity was not found. Build Complete Book Reader V2 first.",
                    "OK");
                return;
            }

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            FixBackpack();
            UpgradeArtworkPanel();
            UpgradeArtworkGraphic();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Book Reader V4 applied. Journey's backpack is back over the screen-right shorts/leg, and every book illustration now has a richer framed, dimensional children's-book presentation.",
                "OK");
        }

        static void FixBackpack()
        {
            GameObject bag = Find("JourneyBackpack");
            if (bag == null) return;

            RectTransform rect = bag.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.255f, .255f);
                rect.anchorMax = new Vector2(.355f, .345f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            GameObject journey = Find("Journey");
            if (journey != null && bag.transform.parent == journey.transform.parent)
                bag.transform.SetSiblingIndex(Mathf.Min(journey.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));
        }

        static void UpgradeArtworkPanel()
        {
            GameObject panel = Find("ArtworkPanel");
            if (panel == null) return;

            Image panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = Hex("FFF9F1", .98f);
                panelImage.raycastTarget = false;
            }

            Shadow shadow = panel.GetComponent<Shadow>();
            if (shadow == null) shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = Hex("4D236F", .20f);
            shadow.effectDistance = new Vector2(8f, -8f);
            shadow.useGraphicAlpha = true;

            Outline outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.AddComponent<Outline>();
            outline.effectColor = Hex("B984E3", .88f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = true;

            Transform existingInner = panel.transform.Find("ArtInnerCard");
            if (existingInner != null) Object.DestroyImmediate(existingInner.gameObject);
            Transform existingGlow = panel.transform.Find("ArtGlow");
            if (existingGlow != null) Object.DestroyImmediate(existingGlow.gameObject);
            Transform existingGloss = panel.transform.Find("ArtGloss");
            if (existingGloss != null) Object.DestroyImmediate(existingGloss.gameObject);

            Image inner = CreateImage(panel.transform, "ArtInnerCard",
                new Vector2(.035f, .035f), new Vector2(.965f, .965f), Hex("FFFFFF", .72f));
            inner.raycastTarget = false;
            inner.transform.SetAsFirstSibling();

            Image glow = CreateImage(panel.transform, "ArtGlow",
                new Vector2(.15f, .14f), new Vector2(.85f, .86f), Hex("F6C7EB", .22f));
            glow.raycastTarget = false;
            glow.transform.SetSiblingIndex(Mathf.Min(1, panel.transform.childCount - 1));

            Image gloss = CreateImage(panel.transform, "ArtGloss",
                new Vector2(.08f, .78f), new Vector2(.92f, .90f), Hex("FFFFFF", .38f));
            gloss.raycastTarget = false;
            gloss.transform.SetSiblingIndex(Mathf.Min(2, panel.transform.childCount - 1));

            AddSparkle(panel.transform, "SparkleTL", "✦", Hex("F2B933"), new Vector2(.07f, .82f), new Vector2(.16f, .94f));
            AddSparkle(panel.transform, "SparkleBR", "✦", Hex("F04AA4"), new Vector2(.84f, .07f), new Vector2(.93f, .19f));
        }

        static void UpgradeArtworkGraphic()
        {
            GameObject artwork = Find("PageArtwork");
            if (artwork == null) return;

            RectTransform rect = artwork.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.08f, .07f);
                rect.anchorMax = new Vector2(.92f, .93f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            Graphic graphic = artwork.GetComponent<Graphic>();
            if (graphic == null) return;

            Shadow shadow = artwork.GetComponent<Shadow>();
            if (shadow == null) shadow = artwork.AddComponent<Shadow>();
            shadow.effectColor = Hex("4B265F", .22f);
            shadow.effectDistance = new Vector2(7f, -8f);
            shadow.useGraphicAlpha = true;

            Outline outline = artwork.GetComponent<Outline>();
            if (outline == null) outline = artwork.AddComponent<Outline>();
            outline.effectColor = Hex("5D2B8D", .34f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);
            outline.useGraphicAlpha = true;
        }

        static void AddSparkle(Transform parent, string name, string symbol, Color color, Vector2 min, Vector2 max)
        {
            Transform old = parent.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = symbol;
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.outlineColor = Color.white;
            text.outlineWidth = .12f;
        }

        static Image CreateImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
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
            return image;
        }

        static GameObject Find(string name)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (go.name == name) return go;
            return null;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                c.a = alpha;
                return c;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
#endif
