#if UNITY_EDITOR
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuModernControlsV7
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Apply Modern Controls V7")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            RemoveJourneyBackingSquare();
            BuildBackpackControl();
            RebuildModernGameButtons();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Modern Controls V7 applied: Journey's backing square is removed, the greeting control is now a backpack, and all three game buttons have a layered glossy mobile-game treatment.",
                "OK");
        }

        static void RemoveJourneyBackingSquare()
        {
            // Keep the scene objects in place in case we want a rug later, but remove the pink card/square behind Journey.
            MakeTransparent("JourneyRug");
            MakeTransparent("RugInner");
        }

        static void MakeTransparent(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image != null)
            {
                var c = image.color;
                c.a = 0f;
                image.color = c;
                image.raycastTarget = false;
            }

            foreach (var effect in go.GetComponents<Shadow>())
                Object.DestroyImmediate(effect);
            foreach (var effect in go.GetComponents<Outline>())
                Object.DestroyImmediate(effect);
        }

        static void BuildBackpackControl()
        {
            var bag = GameObject.Find("JourneyVoiceButton");
            if (bag == null) return;

            SetRect(bag, new Vector2(.185f, .325f), new Vector2(.345f, .435f));
            Sprite rounded = GetRoundedSprite();

            var bagImage = bag.GetComponent<Image>();
            if (bagImage != null)
            {
                bagImage.sprite = rounded;
                bagImage.type = Image.Type.Sliced;
                bagImage.color = Hex("D839A5");
            }

            string[] generated = { "V7BagHandle", "V7BagStrapL", "V7BagStrapR", "V7BagFlap", "V7BagPocket", "V7BagBadge", "V7BagShine" };
            foreach (var n in generated) RemoveChild(bag.transform, n);

            // Shoulder straps behind the body.
            var leftStrap = CreatePanel(bag.transform, "V7BagStrapL", rounded,
                new Vector2(-.05f, .18f), new Vector2(.18f, .76f), Hex("74228D"));
            leftStrap.transform.SetAsFirstSibling();
            var rightStrap = CreatePanel(bag.transform, "V7BagStrapR", rounded,
                new Vector2(.82f, .18f), new Vector2(1.05f, .76f), Hex("74228D"));
            rightStrap.transform.SetAsFirstSibling();

            // Top handle.
            var handle = CreatePanel(bag.transform, "V7BagHandle", rounded,
                new Vector2(.30f, .82f), new Vector2(.70f, 1.10f), Hex("6D208C"));
            handle.transform.SetAsFirstSibling();

            // Main flap and pocket make the icon read clearly as a backpack instead of a circle.
            var flap = CreatePanel(bag.transform, "V7BagFlap", rounded,
                new Vector2(.08f, .49f), new Vector2(.92f, .86f), Hex("F15BB7"));
            EnsureShadow(flap.gameObject, new Vector2(0f, -4f), Hex("5C155F", .50f));
            EnsureOutline(flap.gameObject, Hex("FFC1EC"), new Vector2(2f, -2f));

            var pocket = CreatePanel(bag.transform, "V7BagPocket", rounded,
                new Vector2(.15f, .08f), new Vector2(.85f, .43f), Hex("B62D9D"));
            EnsureShadow(pocket.gameObject, new Vector2(0f, -3f), Hex("541058", .48f));
            EnsureOutline(pocket.gameObject, Hex("FA8BD4"), new Vector2(2f, -2f));

            var badge = CreateText(bag.transform, "V7BagBadge", "J", 32f, FontStyles.Bold,
                Color.white, new Vector2(.36f, .13f), new Vector2(.64f, .40f));
            badge.outlineColor = Hex("64166C");
            badge.outlineWidth = .12f;

            var shine = CreatePanel(bag.transform, "V7BagShine", rounded,
                new Vector2(.14f, .67f), new Vector2(.86f, .87f), new Color(1f, 1f, 1f, .21f));
            shine.raycastTarget = false;

            var oldLabel = bag.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (oldLabel != null) oldLabel.text = string.Empty;

            ClearEffects(bag);
            EnsureShadow(bag, new Vector2(0f, -8f), Hex("43104F", .68f));
            EnsureOutline(bag, Hex("FFD24A"), new Vector2(4f, -4f));

            if (bag.GetComponent<JuicyUIButton>() == null)
                bag.AddComponent<JuicyUIButton>();
        }

        static void RebuildModernGameButtons()
        {
            SetRect("GamePanel", new Vector2(.50f, .315f), new Vector2(.955f, .655f));
            SetColor("GamePanel", Hex("6822B0", .98f));

            var panel = GameObject.Find("GamePanel");
            if (panel != null)
            {
                ClearEffects(panel);
                EnsureShadow(panel, new Vector2(0f, -13f), Hex("270642", .67f));
                EnsureOutline(panel, Hex("DFA9FF"), new Vector2(4f, -4f));
            }

            var choose = FindTMP("GamePanel/Choose");
            if (choose != null)
            {
                choose.fontSize = 29f;
                choose.fontStyle = FontStyles.Bold;
                choose.color = Color.white;
                choose.outlineColor = Hex("40105F");
                choose.outlineWidth = .10f;
                choose.rectTransform.anchorMin = new Vector2(.06f, .84f);
                choose.rectTransform.anchorMax = new Vector2(.94f, .97f);
                choose.rectTransform.offsetMin = Vector2.zero;
                choose.rectTransform.offsetMax = Vector2.zero;
            }

            StyleModernTile("Counting", "CountingShadow", Hex("FFB52D"), Hex("D97804"), "123", "COUNTING", "Numbers are fun!",
                new Vector2(.07f, .60f), new Vector2(.93f, .80f));
            StyleModernTile("ABC", "ABCShadow", Hex("F43B98"), Hex("B51A67"), "ABC", "LETTERS", "Explore the alphabet!",
                new Vector2(.07f, .36f), new Vector2(.93f, .56f));
            StyleModernTile("Match", "MatchShadow", Hex("1FBEE5"), Hex("007DA9"), "A+", "ALPHABET MATCH", "Match letters & pictures!",
                new Vector2(.07f, .12f), new Vector2(.93f, .32f));
        }

        static void StyleModernTile(
            string tileName,
            string shadowName,
            Color mainColor,
            Color depthColor,
            string icon,
            string title,
            string subtitle,
            Vector2 min,
            Vector2 max)
        {
            var tile = GameObject.Find(tileName);
            if (tile == null) return;
            Sprite rounded = GetRoundedSprite();

            SetRect(tile, min, max);
            var image = tile.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = Image.Type.Sliced;
                image.color = mainColor;
            }

            var shadow = GameObject.Find(shadowName);
            if (shadow != null)
            {
                SetRect(shadow, min + new Vector2(0f, -.018f), max + new Vector2(0f, -.018f));
                var sImg = shadow.GetComponent<Image>();
                if (sImg != null)
                {
                    sImg.sprite = rounded;
                    sImg.type = Image.Type.Sliced;
                    sImg.color = depthColor;
                }
            }

            string[] generated = { "V7InnerRim", "V7TopGloss", "V7BottomShade", "V7ArrowDisc", "V7Arrow", "V7IconGloss" };
            foreach (var n in generated) RemoveChild(tile.transform, n);

            ClearEffects(tile);
            EnsureShadow(tile, new Vector2(0f, -7f), Hex("28073E", .44f));
            EnsureOutline(tile, Color.white, new Vector2(3f, -3f));

            var rim = CreatePanel(tile.transform, "V7InnerRim", rounded,
                new Vector2(.025f, .07f), new Vector2(.975f, .93f), new Color(1f, 1f, 1f, .055f));
            EnsureOutline(rim.gameObject, new Color(1f, 1f, 1f, .45f), new Vector2(2f, -2f));
            rim.raycastTarget = false;

            var gloss = CreatePanel(tile.transform, "V7TopGloss", rounded,
                new Vector2(.045f, .60f), new Vector2(.955f, .91f), new Color(1f, 1f, 1f, .26f));
            gloss.raycastTarget = false;

            var shade = CreatePanel(tile.transform, "V7BottomShade", rounded,
                new Vector2(.04f, .05f), new Vector2(.96f, .28f), new Color(depthColor.r, depthColor.g, depthColor.b, .17f));
            shade.raycastTarget = false;

            var iconBack = tile.transform.Find("IconBack")?.GetComponent<Image>();
            if (iconBack != null)
            {
                var r = iconBack.rectTransform;
                r.anchorMin = new Vector2(.035f, .12f);
                r.anchorMax = new Vector2(.285f, .88f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
                iconBack.sprite = rounded;
                iconBack.type = Image.Type.Sliced;
                iconBack.color = new Color(.995f, .995f, 1f, 1f);
                ClearEffects(iconBack.gameObject);
                EnsureShadow(iconBack.gameObject, new Vector2(0f, -4f), Hex("4B1D52", .25f));
                EnsureOutline(iconBack.gameObject, Color.white, new Vector2(2f, -2f));

                var iconGloss = CreatePanel(iconBack.transform, "V7IconGloss", rounded,
                    new Vector2(.10f, .60f), new Vector2(.90f, .90f), new Color(1f, 1f, 1f, .45f));
                iconGloss.raycastTarget = false;
            }

            var iconText = tile.transform.Find("IconBack/Icon")?.GetComponent<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = icon;
                iconText.fontSize = icon.Length > 2 ? 34f : 39f;
                iconText.fontStyle = FontStyles.Bold;
                iconText.color = mainColor;
                iconText.outlineColor = new Color(depthColor.r, depthColor.g, depthColor.b, .55f);
                iconText.outlineWidth = .05f;
            }

            var titleText = tile.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = title;
                titleText.fontSize = title.Length > 12 ? 22f : 28f;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.white;
                titleText.alignment = TextAlignmentOptions.Left;
                titleText.rectTransform.anchorMin = new Vector2(.32f, .48f);
                titleText.rectTransform.anchorMax = new Vector2(.80f, .88f);
                titleText.rectTransform.offsetMin = Vector2.zero;
                titleText.rectTransform.offsetMax = Vector2.zero;
                titleText.outlineColor = new Color(depthColor.r, depthColor.g, depthColor.b, .52f);
                titleText.outlineWidth = .06f;
            }

            var sub = tile.transform.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            if (sub != null)
            {
                sub.text = subtitle;
                sub.fontSize = 15f;
                sub.fontStyle = FontStyles.Normal;
                sub.color = new Color(1f, 1f, 1f, .96f);
                sub.alignment = TextAlignmentOptions.Left;
                sub.rectTransform.anchorMin = new Vector2(.32f, .16f);
                sub.rectTransform.anchorMax = new Vector2(.80f, .52f);
                sub.rectTransform.offsetMin = Vector2.zero;
                sub.rectTransform.offsetMax = Vector2.zero;
            }

            var arrowDisc = CreatePanel(tile.transform, "V7ArrowDisc", rounded,
                new Vector2(.84f, .23f), new Vector2(.965f, .77f), new Color(1f, 1f, 1f, .18f));
            EnsureOutline(arrowDisc.gameObject, new Color(1f, 1f, 1f, .58f), new Vector2(2f, -2f));
            arrowDisc.raycastTarget = false;

            var arrow = CreateText(tile.transform, "V7Arrow", ">", 43f, FontStyles.Bold,
                Color.white, new Vector2(.845f, .23f), new Vector2(.96f, .77f));
            arrow.outlineColor = new Color(depthColor.r, depthColor.g, depthColor.b, .58f);
            arrow.outlineWidth = .08f;

            if (tile.GetComponent<JuicyUIButton>() == null)
                tile.AddComponent<JuicyUIButton>();
        }

        static Sprite GetRoundedSprite()
        {
            string[] candidates =
            {
                "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png",
                "Assets/LearningWithJourney/Generated/MainMenu/Circle.png"
            };

            foreach (string path in candidates)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) return sprite;
            }
            return null;
        }

        static Image CreatePanel(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, Color color)
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
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            return text;
        }

        static void RemoveChild(Transform parent, string name)
        {
            if (parent == null) return;
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        static void SetRect(string name, Vector2 min, Vector2 max)
        {
            var go = GameObject.Find(name);
            if (go != null) SetRect(go, min, max);
        }

        static void SetRect(GameObject go, Vector2 min, Vector2 max)
        {
            if (go == null || !(go.transform is RectTransform rect)) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetColor(string name, Color color)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        static TextMeshProUGUI FindTMP(string path)
        {
            var go = GameObject.Find(path);
            return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
        }

        static void ClearEffects(GameObject go)
        {
            if (go == null) return;
            foreach (var s in go.GetComponents<Shadow>()) Object.DestroyImmediate(s);
            foreach (var o in go.GetComponents<Outline>()) Object.DestroyImmediate(o);
        }

        static void EnsureShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void EnsureOutline(GameObject go, Color color, Vector2 distance)
        {
            if (go == null) return;
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
