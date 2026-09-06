#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuGameButtonsV14
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/MainMenu/Circle.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Upgrade Game Buttons V14")]
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
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            StyleGamePanel();
            StyleTile("Counting", "CountingShadow", Hex("FFB52C"), Hex("D56A06"), TileIcon.Counting,
                "COUNTING", "Numbers 1-20", new Vector2(.07f, .60f), new Vector2(.93f, .80f));
            StyleTile("ABC", "ABCShadow", Hex("F33D98"), Hex("B31565"), TileIcon.Letters,
                "LETTERS", "Explore the alphabet", new Vector2(.07f, .36f), new Vector2(.93f, .56f));
            StyleTile("Match", "MatchShadow", Hex("20BFE7"), Hex("087C9D"), TileIcon.Match,
                "ALPHABET MATCH", "Match letters & pictures", new Vector2(.07f, .12f), new Vector2(.93f, .32f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V14 applied. The three game choices now use layered modern mobile-game buttons with raised picture badges, deeper shadows, gloss, cleaner text, and clear tap arrows.",
                "OK");
        }

        static void StyleGamePanel()
        {
            var panel = GameObject.Find("GamePanel");
            if (panel == null) return;

            SetRect(panel, new Vector2(.49f, .305f), new Vector2(.965f, .655f));
            var image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = Hex("5F1AA2", .94f);
            }

            ClearEffects(panel);
            AddShadow(panel, new Vector2(0f, -12f), Hex("26053F", .62f));
            AddOutline(panel, Hex("DDAEFF", .90f), new Vector2(4f, -4f));

            RemoveChild(panel.transform, "V14PanelGloss");
            var gloss = CreatePanel(panel.transform, "V14PanelGloss", rounded,
                new Vector2(.025f, .83f), new Vector2(.975f, .975f), new Color(1f, 1f, 1f, .08f));
            gloss.raycastTarget = false;
            gloss.transform.SetAsFirstSibling();

            var choose = panel.transform.Find("Choose")?.GetComponent<TextMeshProUGUI>();
            if (choose != null)
            {
                choose.text = "CHOOSE A GAME";
                choose.fontSize = 28f;
                choose.fontStyle = FontStyles.Bold;
                choose.color = Color.white;
                choose.alignment = TextAlignmentOptions.Center;
                choose.rectTransform.anchorMin = new Vector2(.08f, .84f);
                choose.rectTransform.anchorMax = new Vector2(.92f, .965f);
                choose.rectTransform.offsetMin = Vector2.zero;
                choose.rectTransform.offsetMax = Vector2.zero;
                choose.outlineColor = Hex("3E0B69");
                choose.outlineWidth = .08f;
            }
        }

        enum TileIcon { Counting, Letters, Match }

        static void StyleTile(string tileName, string shadowName, Color main, Color depth, TileIcon icon,
            string titleValue, string subtitleValue, Vector2 min, Vector2 max)
        {
            var tile = GameObject.Find(tileName);
            if (tile == null) return;

            SetRect(tile, min, max);
            var image = tile.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = main;
            }

            var shadow = GameObject.Find(shadowName);
            if (shadow != null)
            {
                SetRect(shadow, min + new Vector2(0f, -.022f), max + new Vector2(0f, -.022f));
                var shadowImage = shadow.GetComponent<Image>();
                if (shadowImage != null)
                {
                    shadowImage.sprite = rounded;
                    shadowImage.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
                    shadowImage.color = depth;
                }
            }

            ClearEffects(tile);
            AddShadow(tile, new Vector2(0f, -8f), new Color(depth.r, depth.g, depth.b, .52f));
            AddOutline(tile, Color.white, new Vector2(3f, -3f));

            RemoveGenerated(tile.transform);

            var bottomDepth = CreatePanel(tile.transform, "V14BottomDepth", rounded,
                new Vector2(.025f, .02f), new Vector2(.975f, .24f), new Color(depth.r, depth.g, depth.b, .32f));
            bottomDepth.raycastTarget = false;

            var topGloss = CreatePanel(tile.transform, "V14TopGloss", rounded,
                new Vector2(.035f, .62f), new Vector2(.965f, .93f), new Color(1f, 1f, 1f, .28f));
            topGloss.raycastTarget = false;

            var inner = CreatePanel(tile.transform, "V14InnerRim", rounded,
                new Vector2(.025f, .07f), new Vector2(.975f, .93f), new Color(1f, 1f, 1f, .035f));
            AddOutline(inner.gameObject, new Color(1f, 1f, 1f, .40f), new Vector2(2f, -2f));
            inner.raycastTarget = false;

            var badge = CreatePanel(tile.transform, "V14PictureBadge", rounded,
                new Vector2(.035f, .12f), new Vector2(.285f, .88f), Hex("FFFDFE"));
            AddShadow(badge.gameObject, new Vector2(0f, -4f), new Color(depth.r, depth.g, depth.b, .28f));
            AddOutline(badge.gameObject, Color.white, new Vector2(2f, -2f));
            badge.raycastTarget = false;
            BuildPicture(badge.transform, icon, main, depth);

            var title = tile.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = titleValue;
                title.fontSize = titleValue.Length > 12 ? 20f : 27f;
                title.fontStyle = FontStyles.Bold;
                title.color = Color.white;
                title.alignment = TextAlignmentOptions.Left;
                title.rectTransform.anchorMin = new Vector2(.32f, .49f);
                title.rectTransform.anchorMax = new Vector2(.80f, .88f);
                title.rectTransform.offsetMin = Vector2.zero;
                title.rectTransform.offsetMax = Vector2.zero;
                title.outlineColor = new Color(depth.r, depth.g, depth.b, .58f);
                title.outlineWidth = .06f;
            }

            var sub = tile.transform.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            if (sub != null)
            {
                sub.text = subtitleValue;
                sub.fontSize = 14f;
                sub.fontStyle = FontStyles.Normal;
                sub.color = new Color(1f, 1f, 1f, .97f);
                sub.alignment = TextAlignmentOptions.Left;
                sub.rectTransform.anchorMin = new Vector2(.32f, .16f);
                sub.rectTransform.anchorMax = new Vector2(.80f, .50f);
                sub.rectTransform.offsetMin = Vector2.zero;
                sub.rectTransform.offsetMax = Vector2.zero;
            }

            var arrowDisc = CreatePanel(tile.transform, "V14ArrowDisc", circle,
                new Vector2(.835f, .24f), new Vector2(.955f, .76f), new Color(1f, 1f, 1f, .24f));
            AddOutline(arrowDisc.gameObject, new Color(1f, 1f, 1f, .65f), new Vector2(2f, -2f));
            arrowDisc.raycastTarget = false;

            var arrow = CreateText(tile.transform, "V14Arrow", ">", 40f, FontStyles.Bold, Color.white,
                new Vector2(.84f, .24f), new Vector2(.95f, .76f));
            arrow.outlineColor = new Color(depth.r, depth.g, depth.b, .55f);
            arrow.outlineWidth = .07f;
        }

        static void BuildPicture(Transform parent, TileIcon icon, Color main, Color depth)
        {
            switch (icon)
            {
                case TileIcon.Counting:
                    BuildCountingPicture(parent, main, depth);
                    break;
                case TileIcon.Letters:
                    BuildLettersPicture(parent, main, depth);
                    break;
                case TileIcon.Match:
                    BuildMatchPicture(parent, main, depth);
                    break;
            }
        }

        static void BuildCountingPicture(Transform parent, Color main, Color depth)
        {
            CreateNumberBlock(parent, "One", "1", new Vector2(.10f, .22f), new Vector2(.38f, .55f), main, depth);
            CreateNumberBlock(parent, "Two", "2", new Vector2(.36f, .38f), new Vector2(.64f, .71f), Hex("F45A9E"), Hex("B91E66"));
            CreateNumberBlock(parent, "Three", "3", new Vector2(.62f, .22f), new Vector2(.90f, .55f), Hex("25BDE5"), Hex("087C9D"));
        }

        static void CreateNumberBlock(Transform parent, string name, string number, Vector2 min, Vector2 max, Color color, Color depth)
        {
            var block = CreatePanel(parent, name, rounded, min, max, color);
            AddShadow(block.gameObject, new Vector2(0f, -3f), new Color(depth.r, depth.g, depth.b, .35f));
            var text = CreateText(block.transform, "Number", number, 23f, FontStyles.Bold, Color.white,
                new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            text.alignment = TextAlignmentOptions.Center;
        }

        static void BuildLettersPicture(Transform parent, Color main, Color depth)
        {
            var a = CreatePanel(parent, "CardA", rounded, new Vector2(.10f, .30f), new Vector2(.42f, .72f), main);
            var b = CreatePanel(parent, "CardB", rounded, new Vector2(.34f, .20f), new Vector2(.66f, .62f), Hex("8F46D3"));
            var c = CreatePanel(parent, "CardC", rounded, new Vector2(.58f, .30f), new Vector2(.90f, .72f), Hex("FFAA28"));
            AddShadow(a.gameObject, new Vector2(0f, -3f), new Color(depth.r, depth.g, depth.b, .30f));
            AddShadow(b.gameObject, new Vector2(0f, -3f), Hex("4C1779", .30f));
            AddShadow(c.gameObject, new Vector2(0f, -3f), Hex("A55A08", .30f));
            AddCenteredLetter(a.transform, "A");
            AddCenteredLetter(b.transform, "B");
            AddCenteredLetter(c.transform, "C");
        }

        static void AddCenteredLetter(Transform parent, string letter)
        {
            var t = CreateText(parent, "Letter", letter, 22f, FontStyles.Bold, Color.white,
                new Vector2(.04f, .04f), new Vector2(.96f, .96f));
            t.alignment = TextAlignmentOptions.Center;
        }

        static void BuildMatchPicture(Transform parent, Color main, Color depth)
        {
            var cardA = CreatePanel(parent, "LetterCard", rounded, new Vector2(.08f, .23f), new Vector2(.44f, .77f), main);
            AddShadow(cardA.gameObject, new Vector2(0f, -3f), new Color(depth.r, depth.g, depth.b, .32f));
            AddCenteredLetter(cardA.transform, "A");

            var cardB = CreatePanel(parent, "PictureCard", rounded, new Vector2(.56f, .23f), new Vector2(.92f, .77f), Hex("FFF0F6"));
            AddShadow(cardB.gameObject, new Vector2(0f, -3f), new Color(depth.r, depth.g, depth.b, .22f));

            var apple = CreatePanel(cardB.transform, "Apple", circle, new Vector2(.23f, .24f), new Vector2(.77f, .71f), Hex("F04477"));
            apple.raycastTarget = false;
            var leaf = CreatePanel(cardB.transform, "Leaf", rounded, new Vector2(.57f, .65f), new Vector2(.78f, .82f), Hex("65B94A"));
            leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            leaf.raycastTarget = false;

            var line = CreatePanel(parent, "MatchLine", rounded, new Vector2(.43f, .46f), new Vector2(.57f, .54f), new Color(depth.r, depth.g, depth.b, .55f));
            line.raycastTarget = false;
        }

        static void RemoveGenerated(Transform tile)
        {
            string[] names =
            {
                "Gloss", "IconBack",
                "V7InnerRim", "V7TopGloss", "V7BottomShade", "V7ArrowDisc", "V7Arrow", "V7IconGloss",
                "V14BottomDepth", "V14TopGloss", "V14InnerRim", "V14PictureBadge", "V14ArrowDisc", "V14Arrow"
            };
            foreach (string name in names) RemoveChild(tile, name);
        }

        static void RemoveChild(Transform parent, string name)
        {
            if (parent == null) return;
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
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
            image.type = sprite == rounded && rounded != null ? Image.Type.Sliced : Image.Type.Simple;
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
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.extraPadding = true;
            text.raycastTarget = false;
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

        static void ClearEffects(GameObject go)
        {
            if (go == null) return;
            foreach (var s in go.GetComponents<Shadow>()) Object.DestroyImmediate(s);
            foreach (var o in go.GetComponents<Outline>()) Object.DestroyImmediate(o);
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var s = go.AddComponent<Shadow>();
            s.effectDistance = distance;
            s.effectColor = color;
            s.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            if (go == null) return;
            var o = go.AddComponent<Outline>();
            o.effectDistance = distance;
            o.effectColor = color;
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
