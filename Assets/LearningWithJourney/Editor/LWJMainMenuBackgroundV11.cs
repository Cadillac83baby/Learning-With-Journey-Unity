#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuBackgroundV11
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/MainMenu/Circle.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Apply 2.5D Classroom Background V11")]
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

            RemoveOldV11();
            PolishExistingBase();
            BuildDimensionalClassroom(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V11 applied. The classroom now has layered walls, window depth, shelving, toys, lighting, baseboards, and floor perspective while keeping Journey and the approved menu layout unchanged.",
                "OK");
        }

        static void RemoveOldV11()
        {
            var old = GameObject.Find("V11BackgroundRoot");
            if (old != null) Object.DestroyImmediate(old);
        }

        static void PolishExistingBase()
        {
            Tint("ClassroomWall", Hex("F7A1C8"));
            Tint("Floor", Hex("C97949"));

            // Old window/shelf remain behind the new dimensional set, so tone them down.
            SetAlpha("Window", 0f);
            SetAlpha("Bookshelf", 0f);

            // Keep the title area clean: no rainbow/fish-bowl circles.
            string[] oldRainbow = { "RainbowPurple", "RainbowBlue", "RainbowYellow", "RainbowPink" };
            foreach (string name in oldRainbow)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static void BuildDimensionalClassroom(Transform canvas)
        {
            var root = new GameObject("V11BackgroundRoot", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.transform.SetAsFirstSibling();

            // WALL DEPTH + BASEBOARD
            var upperGlow = CreatePanel(root.transform, "V11UpperGlow", null,
                new Vector2(0f, .58f), new Vector2(1f, 1f), new Color(1f, .86f, .95f, .18f));
            upperGlow.raycastTarget = false;

            var baseboardShadow = CreatePanel(root.transform, "V11BaseboardShadow", rounded,
                new Vector2(0f, .355f), new Vector2(1f, .378f), Hex("8F3F58", .28f));
            baseboardShadow.raycastTarget = false;

            var baseboard = CreatePanel(root.transform, "V11Baseboard", rounded,
                new Vector2(0f, .365f), new Vector2(1f, .389f), Hex("FBE4F2"));
            AddShadow(baseboard.gameObject, new Vector2(0f, -5f), Hex("743047", .24f));

            // FLOOR PERSPECTIVE: alternating planks + angled highlights add visual depth.
            for (int i = 0; i < 8; i++)
            {
                float y = .02f + i * .043f;
                var plank = CreatePanel(root.transform, "V11FloorPlank" + i, rounded,
                    new Vector2(.015f, y), new Vector2(.985f, y + .012f),
                    i % 2 == 0 ? Hex("8E4B31", .26f) : Hex("F4B078", .14f));
                plank.raycastTarget = false;
            }

            for (int i = 0; i < 7; i++)
            {
                float x = .08f + i * .145f;
                var seam = CreatePanel(root.transform, "V11FloorSeam" + i, rounded,
                    new Vector2(x, .015f), new Vector2(x + .009f, .36f), Hex("79402F", .22f));
                seam.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (x - .5f) * -6f);
                seam.raycastTarget = false;
            }

            // WINDOW UNIT — lowered so it does not sit behind the title.
            var windowShadow = CreatePanel(root.transform, "V11WindowShadow", rounded,
                new Vector2(.025f, .47f), new Vector2(.355f, .735f), Hex("7A3156", .26f));
            windowShadow.rectTransform.anchoredPosition += new Vector2(10f, -12f);

            var windowFrame = CreatePanel(root.transform, "V11WindowFrame", rounded,
                new Vector2(.018f, .482f), new Vector2(.345f, .75f), Hex("FFF0FA"));
            AddShadow(windowFrame.gameObject, new Vector2(8f, -8f), Hex("7D3159", .30f));
            AddOutline(windowFrame.gameObject, Hex("F9C9E7"), new Vector2(3f, -3f));

            var sky = CreatePanel(windowFrame.transform, "V11Sky", rounded,
                new Vector2(.07f, .08f), new Vector2(.93f, .92f), Hex("62C8F4"));
            sky.raycastTarget = false;

            var skyGlow = CreatePanel(sky.transform, "V11SkyGlow", rounded,
                new Vector2(.04f, .58f), new Vector2(.96f, .94f), new Color(1f, 1f, 1f, .22f));
            skyGlow.raycastTarget = false;

            CreateCloud(sky.transform, "V11CloudA", new Vector2(.10f, .64f), new Vector2(.45f, .81f));
            CreateCloud(sky.transform, "V11CloudB", new Vector2(.55f, .43f), new Vector2(.86f, .58f));

            CreatePanel(windowFrame.transform, "V11WindowBarV", rounded,
                new Vector2(.485f, .07f), new Vector2(.515f, .93f), Hex("FFF8FC"));
            CreatePanel(windowFrame.transform, "V11WindowBarH", rounded,
                new Vector2(.07f, .485f), new Vector2(.93f, .515f), Hex("FFF8FC"));

            var sillShadow = CreatePanel(root.transform, "V11SillShadow", rounded,
                new Vector2(.005f, .463f), new Vector2(.36f, .49f), Hex("743047", .28f));
            sillShadow.rectTransform.anchoredPosition += new Vector2(0f, -7f);
            var sill = CreatePanel(root.transform, "V11Sill", rounded,
                new Vector2(.005f, .472f), new Vector2(.36f, .50f), Hex("FFF2FA"));
            AddShadow(sill.gameObject, new Vector2(0f, -4f), Hex("76314E", .22f));

            // Curtains create a more finished classroom scene.
            var curtainL = CreatePanel(root.transform, "V11CurtainL", rounded,
                new Vector2(.003f, .47f), new Vector2(.072f, .755f), Hex("A84CCF", .92f));
            var curtainR = CreatePanel(root.transform, "V11CurtainR", rounded,
                new Vector2(.30f, .47f), new Vector2(.369f, .755f), Hex("A84CCF", .92f));
            AddShadow(curtainL.gameObject, new Vector2(5f, -5f), Hex("54216D", .24f));
            AddShadow(curtainR.gameObject, new Vector2(-5f, -5f), Hex("54216D", .24f));
            CreatePanel(curtainL.transform, "V11CurtainHiL", rounded,
                new Vector2(.18f, .06f), new Vector2(.36f, .94f), new Color(1f, 1f, 1f, .15f));
            CreatePanel(curtainR.transform, "V11CurtainHiR", rounded,
                new Vector2(.64f, .06f), new Vector2(.82f, .94f), new Color(1f, 1f, 1f, .15f));

            // Soft beam from the window behind Journey.
            var beam = CreatePanel(root.transform, "V11WindowLight", null,
                new Vector2(.03f, .36f), new Vector2(.46f, .55f), new Color(1f, .96f, .76f, .11f));
            beam.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -7f);
            beam.raycastTarget = false;

            // RIGHT SHELVING — upper-right so it frames the logo/game area without covering buttons.
            var shelfShadow = CreatePanel(root.transform, "V11ShelfShadow", rounded,
                new Vector2(.755f, .665f), new Vector2(.988f, .885f), Hex("6E2E3E", .26f));
            shelfShadow.rectTransform.anchoredPosition += new Vector2(-9f, -9f);

            var shelf = CreatePanel(root.transform, "V11Shelf", rounded,
                new Vector2(.75f, .675f), new Vector2(.985f, .895f), Hex("B86A50"));
            AddShadow(shelf.gameObject, new Vector2(-7f, -7f), Hex("6C2C35", .25f));
            AddOutline(shelf.gameObject, Hex("E9A98D", .55f), new Vector2(2f, -2f));

            for (int row = 0; row < 2; row++)
            {
                float y0 = .08f + row * .43f;
                var cubby = CreatePanel(shelf.transform, "V11Cubby" + row, rounded,
                    new Vector2(.07f, y0), new Vector2(.93f, y0 + .34f), Hex("84442F"));
                cubby.raycastTarget = false;
                CreatePanel(cubby.transform, "Back", rounded,
                    new Vector2(.04f, .07f), new Vector2(.96f, .93f), Hex("F5C5B4", .36f));

                if (row == 0)
                {
                    BuildBooks(cubby.transform);
                }
                else
                {
                    BuildToyBins(cubby.transform);
                }
            }

            // Small decorative wall board and classroom accents.
            var board = CreatePanel(root.transform, "V11WallBoard", rounded,
                new Vector2(.44f, .69f), new Vector2(.70f, .785f), Hex("6D28A6", .78f));
            AddShadow(board.gameObject, new Vector2(0f, -6f), Hex("42115F", .30f));
            AddOutline(board.gameObject, Hex("F1C8FF", .62f), new Vector2(2f, -2f));
            var boardText = CreateText(board.transform, "V11BoardText", "PLAY  •  LEARN  •  GROW", 20f,
                Color.white, new Vector2(.06f, .12f), new Vector2(.94f, .88f));
            boardText.alignment = TextAlignmentOptions.Center;

            // Decorative stars/hearts away from the logo so the title remains clean.
            CreateDecor(root.transform, "V11StarL", "★", 38f, Hex("FFD84C"), new Vector2(.39f, .62f), new Vector2(.445f, .665f));
            CreateDecor(root.transform, "V11HeartL", "♥", 32f, Hex("E9489C"), new Vector2(.40f, .52f), new Vector2(.45f, .56f));
            CreateDecor(root.transform, "V11StarR", "★", 34f, Hex("FFD84C"), new Vector2(.91f, .61f), new Vector2(.96f, .65f));

            // Low toy bench behind the lower-left area adds depth without creating a large square behind Journey.
            var bench = CreatePanel(root.transform, "V11ToyBench", rounded,
                new Vector2(.015f, .185f), new Vector2(.31f, .27f), Hex("61C7D7", .72f));
            AddShadow(bench.gameObject, new Vector2(0f, -8f), Hex("216070", .24f));
            CreatePanel(bench.transform, "V11BenchTop", rounded,
                new Vector2(.04f, .72f), new Vector2(.96f, .94f), new Color(1f, 1f, 1f, .24f));

            BuildFloorBlocks(root.transform);
        }

        static void BuildBooks(Transform parent)
        {
            Color[] colors = { Hex("F04994"), Hex("21B9E1"), Hex("FFAE28"), Hex("74C94F"), Hex("8D45D4") };
            for (int i = 0; i < 5; i++)
            {
                float x = .07f + i * .18f;
                var book = CreatePanel(parent, "Book" + i, rounded,
                    new Vector2(x, .15f), new Vector2(x + .11f, .78f - (i % 2) * .08f), colors[i]);
                AddShadow(book.gameObject, new Vector2(2f, -2f), new Color(0f, 0f, 0f, .10f));
                CreatePanel(book.transform, "Spine", rounded,
                    new Vector2(.12f, .08f), new Vector2(.28f, .92f), new Color(1f, 1f, 1f, .20f));
            }
        }

        static void BuildToyBins(Transform parent)
        {
            var binA = CreatePanel(parent, "BinA", rounded, new Vector2(.08f, .17f), new Vector2(.44f, .80f), Hex("F45AA7"));
            var binB = CreatePanel(parent, "BinB", rounded, new Vector2(.56f, .17f), new Vector2(.92f, .80f), Hex("27BDE4"));
            AddShadow(binA.gameObject, new Vector2(0f, -3f), new Color(0f, 0f, 0f, .12f));
            AddShadow(binB.gameObject, new Vector2(0f, -3f), new Color(0f, 0f, 0f, .12f));
            CreateText(binA.transform, "Label", "ABC", 19f, Color.white, new Vector2(.08f, .18f), new Vector2(.92f, .82f)).alignment = TextAlignmentOptions.Center;
            CreateText(binB.transform, "Label", "123", 19f, Color.white, new Vector2(.08f, .18f), new Vector2(.92f, .82f)).alignment = TextAlignmentOptions.Center;
        }

        static void BuildFloorBlocks(Transform parent)
        {
            Color[] colors = { Hex("F04B96"), Hex("20BDE5"), Hex("FFAB28"), Hex("8C45D1") };
            string[] labels = { "A", "B", "1", "2" };
            for (int i = 0; i < 4; i++)
            {
                float x = .62f + i * .075f;
                float y = .205f + (i % 2) * .018f;
                var block = CreatePanel(parent, "V11Block" + i, rounded,
                    new Vector2(x, y), new Vector2(x + .06f, y + .052f), colors[i]);
                AddShadow(block.gameObject, new Vector2(0f, -4f), new Color(0f, 0f, 0f, .18f));
                var t = CreateText(block.transform, "Letter", labels[i], 18f, Color.white, new Vector2(.08f, .08f), new Vector2(.92f, .92f));
                t.alignment = TextAlignmentOptions.Center;
            }
        }

        static void CreateCloud(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var cloud = CreatePanel(parent, name, rounded, min, max, new Color(1f, 1f, 1f, .83f));
            cloud.raycastTarget = false;
            var puff1 = CreatePanel(cloud.transform, "Puff1", circle, new Vector2(.02f, .16f), new Vector2(.42f, .88f), new Color(1f, 1f, 1f, .92f));
            var puff2 = CreatePanel(cloud.transform, "Puff2", circle, new Vector2(.28f, .02f), new Vector2(.72f, .98f), new Color(1f, 1f, 1f, .96f));
            var puff3 = CreatePanel(cloud.transform, "Puff3", circle, new Vector2(.60f, .18f), new Vector2(.98f, .86f), new Color(1f, 1f, 1f, .90f));
            puff1.raycastTarget = puff2.raycastTarget = puff3.raycastTarget = false;
        }

        static void CreateDecor(Transform parent, string name, string value, float size, Color color, Vector2 min, Vector2 max)
        {
            var text = CreateText(parent, name, value, size, color, min, max);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
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
            image.type = sprite == rounded ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, Color color, Vector2 min, Vector2 max)
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
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
            return text;
        }

        static void Tint(string name, Color color)
        {
            var go = GameObject.Find(name);
            var image = go != null ? go.GetComponent<Image>() : null;
            if (image != null) image.color = color;
        }

        static void SetAlpha(string name, float alpha)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image != null)
            {
                var c = image.color;
                c.a = alpha;
                image.color = c;
                image.raycastTarget = false;
            }
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
