#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.Core;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuBuilder
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string GeneratedPath = "Assets/LearningWithJourney/Generated";
        const string GradientPath = GeneratedPath + "/MainMenuGradient.png";
        const string RoundedPath = GeneratedPath + "/RoundedPanel.png";

        static Sprite roundedSprite;
        static Sprite gradientSprite;

        [MenuItem("Learning with Journey/Build Polished Main Menu")]
        public static void BuildPolishedMainMenu()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "MainMenu scene is missing. Run Build Starter Scenes first.", "OK");
                return;
            }

            EnsureGeneratedSprites();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearScene(scene);

            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();
            var router = systems.AddComponent<SceneRouter>();

            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("24183F");
            camera.orthographic = true;
            cameraGO.tag = "MainCamera";

            var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var background = CreateImage(canvasGO.transform, "Background", gradientSprite, Color.white, Vector2.zero, Vector2.one);
            background.type = Image.Type.Simple;

            // Soft layered background decoration.
            CreateBubble(canvasGO.transform, new Vector2(.02f, .78f), new Vector2(.24f, .91f), Hex("FFFFFF", .10f));
            CreateBubble(canvasGO.transform, new Vector2(.76f, .72f), new Vector2(1.03f, .88f), Hex("FFFFFF", .08f));
            CreateBubble(canvasGO.transform, new Vector2(.63f, .02f), new Vector2(.98f, .18f), Hex("FFB7DE", .12f));
            CreateStar(canvasGO.transform, "SkyStar1", "★", 44, new Vector2(.08f, .82f), Hex("FFF0A6"), 5f, 1.2f, .2f);
            CreateStar(canvasGO.transform, "SkyStar2", "★", 30, new Vector2(.88f, .91f), Hex("FFFFFF", .75f), 8f, .9f, 1.4f);
            CreateStar(canvasGO.transform, "SkyStar3", "✦", 34, new Vector2(.79f, .58f), Hex("FFD1EA", .85f), 6f, 1.1f, 2.1f);

            var title = CreateText(canvasGO.transform, "BrandTitle", "Learning with Journey", 64, FontStyles.Bold, Color.white,
                new Vector2(.06f, .905f), new Vector2(.94f, .965f));
            title.alignment = TextAlignmentOptions.Center;
            AddTextShadow(title.gameObject, new Vector2(0, -4), Hex("4A236B", .45f));

            var tagline = CreateText(canvasGO.transform, "Tagline", "LEARN  •  GROW  •  SHINE", 25, FontStyles.Bold, Hex("FFF2B8"),
                new Vector2(.20f, .875f), new Vector2(.80f, .91f));
            tagline.alignment = TextAlignmentOptions.Center;
            tagline.characterSpacing = 2f;

            // Greeting row.
            var greeting = CreateText(canvasGO.transform, "Greeting", "Hi, Little Star!", 34, FontStyles.Bold, Color.white,
                new Vector2(.07f, .815f), new Vector2(.49f, .86f));
            greeting.alignment = TextAlignmentOptions.Left;

            var level = CreatePill(canvasGO.transform, "LevelPill", "Level 1", new Vector2(.50f, .817f), new Vector2(.68f, .858f), Hex("FFFFFF", .18f), 24);
            var starPill = CreateStatPill(canvasGO.transform, "StarPill", "★", "0", new Vector2(.69f, .817f), new Vector2(.82f, .858f), Hex("FFF0A6"));
            var coinPill = CreateStatPill(canvasGO.transform, "CoinPill", "●", "0", new Vector2(.83f, .817f), new Vector2(.96f, .858f), Hex("FFD56A"));

            // Hero panel / character stage.
            var hero = CreatePanel(canvasGO.transform, "HeroPanel", new Vector2(.055f, .535f), new Vector2(.945f, .797f), Hex("FFFFFF", .15f), true);
            AddShadow(hero.gameObject, new Vector2(0, -12), Hex("381C5A", .23f));
            AddOutline(hero.gameObject, Hex("FFFFFF", .26f), new Vector2(2, -2));

            var adventureLabel = CreateText(hero.transform, "AdventureLabel", "TODAY'S ADVENTURE", 23, FontStyles.Bold, Hex("FFF0A6"),
                new Vector2(.08f, .78f), new Vector2(.55f, .92f));
            adventureLabel.alignment = TextAlignmentOptions.Left;
            adventureLabel.characterSpacing = 1.5f;

            var heroTitle = CreateText(hero.transform, "HeroTitle", "Ready to learn\nwith Journey?", 49, FontStyles.Bold, Color.white,
                new Vector2(.08f, .38f), new Vector2(.61f, .78f));
            heroTitle.alignment = TextAlignmentOptions.Left;
            heroTitle.lineSpacing = -6f;

            var heroSubtitle = CreateText(hero.transform, "HeroSubtitle", "Pick a game, earn stars, and shine!", 24, FontStyles.Normal, Hex("F7EFFF"),
                new Vector2(.08f, .17f), new Vector2(.65f, .37f));
            heroSubtitle.alignment = TextAlignmentOptions.Left;

            // Branded character slot. This is intentionally replaceable by Journey's final skeletal rig.
            var medallion = CreatePanel(hero.transform, "JourneyCharacterSlot", new Vector2(.67f, .18f), new Vector2(.93f, .83f), Hex("FCE9FF", .94f), true);
            AddShadow(medallion.gameObject, new Vector2(0, -8), Hex("381C5A", .24f));
            var medallionFloat = medallion.gameObject.AddComponent<UIFloat>();
            SetFloat(medallionFloat, 7f, 1.05f, .5f);

            var crown = CreateText(medallion.transform, "Crown", "♛", 48, FontStyles.Bold, Hex("F0B84F"), new Vector2(.18f, .72f), new Vector2(.82f, .94f));
            crown.alignment = TextAlignmentOptions.Center;
            var j = CreateText(medallion.transform, "JourneyInitial", "J", 120, FontStyles.Bold, Hex("8E56C9"), new Vector2(.08f, .28f), new Vector2(.92f, .76f));
            j.alignment = TextAlignmentOptions.Center;
            var journeyLabel = CreateText(medallion.transform, "JourneyLabel", "JOURNEY", 21, FontStyles.Bold, Hex("6B3B98"), new Vector2(.12f, .08f), new Vector2(.88f, .30f));
            journeyLabel.alignment = TextAlignmentOptions.Center;

            // Game section label.
            var chooseText = CreateText(canvasGO.transform, "ChooseText", "Choose a learning game", 30, FontStyles.Bold, Color.white,
                new Vector2(.07f, .492f), new Vector2(.72f, .53f));
            chooseText.alignment = TextAlignmentOptions.Left;

            CreateGameButton(canvasGO.transform, "CountingButton", "123", "Counting Adventure", "Count, tap, and learn numbers 1–20", Hex("F4A261"),
                new Vector2(.055f, .375f), new Vector2(.945f, .485f), router.OpenCounting);
            CreateGameButton(canvasGO.transform, "ABCButton", "ABC", "ABC Adventure", "Letters, sounds, and first words", Hex("E76BA8"),
                new Vector2(.055f, .255f), new Vector2(.945f, .365f), router.OpenABC);
            CreateGameButton(canvasGO.transform, "AlphabetMatchButton", "A+", "Alphabet Match", "Match each letter to the right picture", Hex("46B8B0"),
                new Vector2(.055f, .135f), new Vector2(.945f, .245f), router.OpenAlphabetMatch);

            // Bottom navigation.
            var nav = CreatePanel(canvasGO.transform, "BottomNav", new Vector2(.055f, .025f), new Vector2(.945f, .112f), Hex("FFFFFF", .16f), true);
            AddOutline(nav.gameObject, Hex("FFFFFF", .22f), new Vector2(1, -1));
            CreateNavButton(nav.transform, "Home", "⌂", "Home", new Vector2(.02f, .08f), new Vector2(.24f, .92f), null, true);
            CreateNavButton(nav.transform, "Rewards", "★", "Rewards", new Vector2(.26f, .08f), new Vector2(.49f, .92f), router.OpenRewards, false);
            CreateNavButton(nav.transform, "Library", "▤", "Library", new Vector2(.51f, .08f), new Vector2(.74f, .92f), router.OpenLibrary, false);
            CreateNavButton(nav.transform, "Parents", "⚙", "Parents", new Vector2(.76f, .08f), new Vector2(.98f, .92f), router.OpenParentZone, false);

            var hud = canvasGO.AddComponent<MainMenuHud>();
            var so = new SerializedObject(hud);
            so.FindProperty("starText").objectReferenceValue = starPill.count;
            so.FindProperty("coinText").objectReferenceValue = coinPill.count;
            so.FindProperty("playerText").objectReferenceValue = greeting;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = canvasGO;
            EditorUtility.DisplayDialog("Learning with Journey", "Polished Main Menu v1 is ready. Open the Game tab in 1080x1920 portrait to preview it.", "OK");
        }

        static void ClearScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);
        }

        static void EnsureGeneratedSprites()
        {
            Directory.CreateDirectory(GeneratedPath);

            const int gh = 256;
            var gradient = new Texture2D(16, gh, TextureFormat.RGBA32, false);
            var bottom = Hex("6E3FA5");
            var middle = Hex("A15FD0");
            var top = Hex("C47BD9");
            for (int y = 0; y < gh; y++)
            {
                var t = y / (float)(gh - 1);
                var color = t < .55f ? Color.Lerp(bottom, middle, t / .55f) : Color.Lerp(middle, top, (t - .55f) / .45f);
                for (int x = 0; x < gradient.width; x++) gradient.SetPixel(x, y, color);
            }
            gradient.Apply();
            File.WriteAllBytes(GradientPath, gradient.EncodeToPNG());
            Object.DestroyImmediate(gradient);
            ConfigureSprite(GradientPath, Vector4.zero);

            const int size = 128;
            const int radius = 28;
            var rounded = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var dx = Mathf.Max(radius - x, 0, x - (size - 1 - radius));
                    var dy = Mathf.Max(radius - y, 0, y - (size - 1 - radius));
                    var inside = (dx * dx) + (dy * dy) <= radius * radius;
                    rounded.SetPixel(x, y, inside ? Color.white : new Color(1, 1, 1, 0));
                }
            }
            rounded.Apply();
            File.WriteAllBytes(RoundedPath, rounded.EncodeToPNG());
            Object.DestroyImmediate(rounded);
            ConfigureSprite(RoundedPath, new Vector4(radius, radius, radius, radius));

            AssetDatabase.Refresh();
            gradientSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GradientPath);
            roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
        }

        static void ConfigureSprite(string path, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color, bool sliced)
        {
            var image = CreateImage(parent, name, roundedSprite, color, min, max);
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            return image;
        }

        static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        static TMP_Text CreateText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.enableWordWrapping = true;
            return text;
        }

        static TMP_Text CreatePill(Transform parent, string name, string textValue, Vector2 min, Vector2 max, Color color, float fontSize)
        {
            var pill = CreatePanel(parent, name, min, max, color, true);
            var text = CreateText(pill.transform, "Text", textValue, fontSize, FontStyles.Bold, Color.white, new Vector2(.08f, .08f), new Vector2(.92f, .92f));
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        struct StatPill
        {
            public TMP_Text count;
        }

        static StatPill CreateStatPill(Transform parent, string name, string icon, string value, Vector2 min, Vector2 max, Color iconColor)
        {
            var pill = CreatePanel(parent, name, min, max, Hex("FFFFFF", .18f), true);
            var iconText = CreateText(pill.transform, "Icon", icon, 24, FontStyles.Bold, iconColor, new Vector2(.08f, .12f), new Vector2(.42f, .88f));
            iconText.alignment = TextAlignmentOptions.Center;
            var count = CreateText(pill.transform, "Count", value, 23, FontStyles.Bold, Color.white, new Vector2(.40f, .12f), new Vector2(.92f, .88f));
            count.alignment = TextAlignmentOptions.Center;
            return new StatPill { count = count };
        }

        static void CreateGameButton(Transform parent, string name, string icon, string title, string subtitle, Color accent, Vector2 min, Vector2 max, UnityAction action)
        {
            var panel = CreatePanel(parent, name, min, max, Hex("FFFFFF", .94f), true);
            AddShadow(panel.gameObject, new Vector2(0, -10), Hex("32184E", .25f));

            var button = panel.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.98f, .98f, 1f, 1f);
            colors.pressedColor = new Color(.91f, .89f, .96f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = .08f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);

            var iconPanel = CreatePanel(panel.transform, "IconPanel", new Vector2(.035f, .16f), new Vector2(.23f, .84f), accent, true);
            var iconText = CreateText(iconPanel.transform, "Icon", icon, 38, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            iconText.alignment = TextAlignmentOptions.Center;

            var titleText = CreateText(panel.transform, "Title", title, 32, FontStyles.Bold, Hex("4A2A68"), new Vector2(.27f, .48f), new Vector2(.84f, .84f));
            titleText.alignment = TextAlignmentOptions.Left;
            var subtitleText = CreateText(panel.transform, "Subtitle", subtitle, 21, FontStyles.Normal, Hex("725B82"), new Vector2(.27f, .16f), new Vector2(.84f, .51f));
            subtitleText.alignment = TextAlignmentOptions.Left;
            var arrow = CreateText(panel.transform, "Arrow", "›", 48, FontStyles.Bold, accent, new Vector2(.86f, .20f), new Vector2(.96f, .80f));
            arrow.alignment = TextAlignmentOptions.Center;
        }

        static void CreateNavButton(Transform parent, string name, string icon, string label, Vector2 min, Vector2 max, UnityAction action, bool active)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);

            var color = active ? Hex("FFF0A6") : Hex("FFFFFF", .82f);
            var iconText = CreateText(go.transform, "Icon", icon, 28, FontStyles.Bold, color, new Vector2(.05f, .43f), new Vector2(.95f, .94f));
            iconText.alignment = TextAlignmentOptions.Center;
            var labelText = CreateText(go.transform, "Label", label, 17, active ? FontStyles.Bold : FontStyles.Normal, color, new Vector2(.04f, .06f), new Vector2(.96f, .46f));
            labelText.alignment = TextAlignmentOptions.Center;
        }

        static void CreateBubble(Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var image = CreatePanel(parent, "GlowBubble", min, max, color, true);
            image.raycastTarget = false;
        }

        static void CreateStar(Transform parent, string name, string symbol, float size, Vector2 anchor, Color color, float amplitude, float speed, float phase)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(size * 1.8f, size * 1.8f);
            rect.anchoredPosition = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = symbol;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            var floating = go.AddComponent<UIFloat>();
            SetFloat(floating, amplitude, speed, phase);
        }

        static void SetFloat(UIFloat floating, float amplitude, float speed, float phase)
        {
            var so = new SerializedObject(floating);
            so.FindProperty("amplitude").floatValue = amplitude;
            so.FindProperty("speed").floatValue = speed;
            so.FindProperty("phase").floatValue = phase;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void AddTextShadow(GameObject go, Vector2 distance, Color color)
        {
            AddShadow(go, distance, color);
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
            if (!ColorUtility.TryParseHtmlString("#" + hex, out var color)) color = Color.white;
            color.a = alpha;
            return color;
        }
    }
}
#endif
