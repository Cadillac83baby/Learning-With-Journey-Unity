#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LearningWithJourney.Core;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJLibraryBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/Library.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";

        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Library V1")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            Texture2D journeyTexture = FindJourneyTexture(out Rect journeyUv);

            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);

            BuildScene(journeyTexture, journeyUv);
            EnsureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Library V1 is ready. It includes a cozy reading-room background, Journey with her approved backpack placement, Stars, Journey Coins, Level, four large learning book shelves, interactive category selection, and the Home / Library / Rewards / Parents navigation bar.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("4E2A6B");
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            BuildBackground(canvasGo.transform);

            var controllerGo = new GameObject("LibraryController");
            var controller = controllerGo.AddComponent<LibraryScreenControllerV1>();

            BuildHeader(canvasGo.transform, out TMP_Text starsText, out TMP_Text coinsText, out TMP_Text levelText);
            BuildTitle(canvasGo.transform);
            BuildJourney(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speechText);
            BuildCategoryCards(canvasGo.transform, controller);
            BuildSelectionPanel(canvasGo.transform, out TMP_Text selectionTitleText, out TMP_Text selectionMessageText);
            BuildBottomNav(canvasGo.transform, controller);

            var so = new SerializedObject(controller);
            so.FindProperty("starsText").objectReferenceValue = starsText;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("selectionTitleText").objectReferenceValue = selectionTitleText;
            so.FindProperty("selectionMessageText").objectReferenceValue = selectionMessageText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "LibraryWall", Vector2.zero, Vector2.one, Hex("F6D8EB"));
            CreateImage(parent, "LibraryWallGlow", new Vector2(0f, .43f), new Vector2(1f, 1f), Hex("FFF4D8", .36f));
            CreateImage(parent, "LibraryFloor", new Vector2(0f, 0f), new Vector2(1f, .34f), Hex("C98A63"));
            CreateImage(parent, "LibraryRug", new Vector2(.035f, .09f), new Vector2(.965f, .39f), Hex("8B57D0", .93f));

            // Warm reading nook architecture. These are deliberately simple Unity UI shapes
            // so the screen stays crisp on both Android and iOS without external art dependencies.
            var arch = CreatePanel(parent, "ReadingNook", new Vector2(.035f, .39f), new Vector2(.965f, .78f), Hex("FFF6EE", .74f));
            AddOutline(arch.gameObject, Hex("C591B9", .7f), new Vector2(3f, -3f));
            arch.raycastTarget = false;

            CreateImage(parent, "CeilingTrim", new Vector2(.02f, .775f), new Vector2(.98f, .79f), Hex("D8A13B"));
            CreateImage(parent, "CeilingTrimGlow", new Vector2(.04f, .789f), new Vector2(.96f, .796f), Hex("FFF0A1", .88f));

            BuildSideShelf(parent, "LeftShelf", new Vector2(.02f, .43f), new Vector2(.15f, .66f));
            BuildSideShelf(parent, "RightShelf", new Vector2(.86f, .43f), new Vector2(.99f, .66f));

            // Soft floor cushions to make the scene feel like a preschool reading corner.
            CreatePanel(parent, "CushionPink", new Vector2(.08f, .27f), new Vector2(.23f, .33f), Hex("F35AA6", .75f));
            CreatePanel(parent, "CushionGold", new Vector2(.24f, .255f), new Vector2(.38f, .315f), Hex("F7C64B", .72f));
        }

        static void BuildSideShelf(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var shelf = CreatePanel(parent, name, min, max, Hex("9A633E", .92f));
            AddShadow(shelf.gameObject, new Vector2(5f, -6f), Hex("5B3223", .24f));
            AddOutline(shelf.gameObject, Hex("E1B06A"), new Vector2(2f, -2f));

            CreateImage(shelf.transform, "ShelfA", new Vector2(.05f, .32f), new Vector2(.95f, .36f), Hex("6C432F"));
            CreateImage(shelf.transform, "ShelfB", new Vector2(.05f, .65f), new Vector2(.95f, .69f), Hex("6C432F"));

            Color[] colors = { Hex("F04AA4"), Hex("6D48C8"), Hex("34B9D5"), Hex("F0AE34") };
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = .10f + col * .27f;
                    float y = .07f + row * .33f;
                    CreateImage(shelf.transform, "Book_" + row + "_" + col,
                        new Vector2(x, y), new Vector2(x + .15f, y + .20f), colors[(row + col) % colors.Length]);
                }
            }
        }

        static void BuildHeader(Transform parent, out TMP_Text starsText, out TMP_Text coinsText, out TMP_Text levelText)
        {
            var starPill = CreatePanel(parent, "StarPill", new Vector2(.035f, .925f), new Vector2(.35f, .98f), Hex("E83F9C"));
            AddShadow(starPill.gameObject, new Vector2(0f, -6f), Hex("7F1D63", .42f));
            AddOutline(starPill.gameObject, Hex("FFD8EE"), new Vector2(3f, -3f));
            var label = CreateText(starPill.transform, "Label", "STARS", 18f, FontStyles.Bold, Color.white, new Vector2(.05f, .14f), new Vector2(.48f, .86f));
            label.alignment = TextAlignmentOptions.Center;
            starsText = CreateText(starPill.transform, "Count", "0", 32f, FontStyles.Bold, Color.white, new Vector2(.48f, .10f), new Vector2(.94f, .90f));
            starsText.alignment = TextAlignmentOptions.Center;

            var coinPill = CreatePanel(parent, "CoinPill", new Vector2(.56f, .925f), new Vector2(.965f, .98f), Hex("6636C5"));
            AddShadow(coinPill.gameObject, new Vector2(0f, -6f), Hex("35186F", .42f));
            AddOutline(coinPill.gameObject, Hex("EEE1FF"), new Vector2(3f, -3f));
            var coinLabel = CreateText(coinPill.transform, "Label", "JOURNEY COINS", 18f, FontStyles.Bold, Color.white, new Vector2(.04f, .16f), new Vector2(.62f, .84f));
            coinLabel.alignment = TextAlignmentOptions.Center;
            coinsText = CreateText(coinPill.transform, "Count", "0", 32f, FontStyles.Bold, Hex("FFE05B"), new Vector2(.62f, .10f), new Vector2(.96f, .90f));
            coinsText.alignment = TextAlignmentOptions.Center;

            var levelPill = CreatePanel(parent, "LevelPill", new Vector2(.39f, .882f), new Vector2(.61f, .922f), Hex("FFFFFF", .95f));
            AddOutline(levelPill.gameObject, Hex("B98ADB"), new Vector2(2f, -2f));
            levelText = CreateText(levelPill.transform, "Level", "LEVEL 1", 20f, FontStyles.Bold, Hex("6031A3"), new Vector2(.05f, .10f), new Vector2(.95f, .90f));
            levelText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildTitle(Transform parent)
        {
            var title = CreateText(parent, "LibraryTitle", "LIBRARY", 74f, FontStyles.Bold, Color.white, new Vector2(.12f, .80f), new Vector2(.88f, .89f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("54228E");
            title.outlineWidth = .26f;

            var ribbon = CreatePanel(parent, "LibraryRibbon", new Vector2(.24f, .765f), new Vector2(.76f, .81f), Hex("31BFC5"));
            AddOutline(ribbon.gameObject, Hex("167F8C"), new Vector2(3f, -3f));
            var subtitle = CreateText(ribbon.transform, "Subtitle", "READ  |  LEARN  |  IMAGINE", 22f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static void BuildJourney(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .30f);
            rect.anchorMax = new Vector2(.42f, .69f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.texture = journeyTexture;
            raw.uvRect = journeyUv;
            raw.color = journeyTexture != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            if (journeyTexture == null)
            {
                var placeholder = CreatePanel(parent, "JourneyPlaceholder", new Vector2(.07f, .39f), new Vector2(.35f, .60f), Hex("E84FA0", .9f));
                var t = CreateText(placeholder.transform, "Text", "JOURNEY", 34f, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
                t.alignment = TextAlignmentOptions.Center;
            }

            // Canonical backpack placement approved on Main Menu and reused across worlds.
            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.255f, .335f), new Vector2(.355f, .425f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .50f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;
            bag.transform.SetSiblingIndex(Mathf.Min(journeyGo.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));

            var bubble = CreatePanel(parent, "JourneyLibraryBubble", new Vector2(.055f, .665f), new Vector2(.43f, .755f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -6f), Hex("67357C", .22f));
            AddOutline(bubble.gameObject, Hex("8D4CC3"), new Vector2(3f, -3f));

            var tail = CreatePanel(bubble.transform, "SpeechTail", new Vector2(.15f, -.17f), new Vector2(.27f, .09f), Color.white);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.raycastTarget = false;
            tail.transform.SetAsFirstSibling();

            speechText = CreateText(bubble.transform, "Speech", "What should we read today?", 23f, FontStyles.Bold, Hex("593078"), new Vector2(.07f, .10f), new Vector2(.93f, .90f));
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.enableAutoSizing = true;
            speechText.fontSizeMin = 17f;
            speechText.fontSizeMax = 24f;
            speechText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildCategoryCards(Transform parent, LibraryScreenControllerV1 controller)
        {
            var heading = CreateText(parent, "ChooseShelf", "CHOOSE A BOOK SHELF", 27f, FontStyles.Bold, Hex("6031A3"), new Vector2(.42f, .715f), new Vector2(.95f, .75f));
            heading.alignment = TextAlignmentOptions.Center;

            var abc = CreateBookCard(parent, "ABCBooks", "ABC", "ABC BOOKS", new Vector2(.42f, .545f), new Vector2(.675f, .71f), Hex("F04AA4"), Hex("A82170"));
            var numbers = CreateBookCard(parent, "NumbersCounting", "123", "NUMBERS", new Vector2(.70f, .545f), new Vector2(.955f, .71f), Hex("F4A528"), Hex("B86B09"));
            var colors = CreateColorBookCard(parent, "ColorsShapes", "COLORS + SHAPES", new Vector2(.42f, .355f), new Vector2(.675f, .52f));
            var story = CreateBookCard(parent, "StoryTime", "STORY", "STORY TIME", new Vector2(.70f, .355f), new Vector2(.955f, .52f), Hex("6C48C8"), Hex("3F2586"));

            UnityEventTools.AddPersistentListener(abc.onClick, controller.SelectABCBooks);
            UnityEventTools.AddPersistentListener(numbers.onClick, controller.SelectNumbersCounting);
            UnityEventTools.AddPersistentListener(colors.onClick, controller.SelectColorsShapes);
            UnityEventTools.AddPersistentListener(story.onClick, controller.SelectStoryTime);
        }

        static Button CreateBookCard(Transform parent, string name, string coverText, string titleText, Vector2 min, Vector2 max, Color coverColor, Color shadowColor)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), shadowColor);
            shadow.raycastTarget = false;

            var card = CreatePanel(parent, name, min, max, Hex("FFF9FE", .98f));
            AddOutline(card.gameObject, coverColor, new Vector2(4f, -4f));
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();

            var cover = CreatePanel(card.transform, "BookCover", new Vector2(.15f, .31f), new Vector2(.85f, .92f), coverColor);
            AddShadow(cover.gameObject, new Vector2(5f, -6f), shadowColor);
            AddOutline(cover.gameObject, Hex("FFF1B5"), new Vector2(2f, -2f));
            cover.raycastTarget = false;
            CreateImage(cover.transform, "Spine", new Vector2(.05f, .04f), new Vector2(.16f, .96f), shadowColor).raycastTarget = false;
            CreateImage(cover.transform, "PageBlock", new Vector2(.18f, .08f), new Vector2(.92f, .18f), Hex("FFF7DC")).raycastTarget = false;

            var coverLabel = CreateText(cover.transform, "CoverText", coverText, 37f, FontStyles.Bold, Color.white, new Vector2(.18f, .28f), new Vector2(.92f, .82f));
            coverLabel.enableAutoSizing = true;
            coverLabel.fontSizeMin = 20f;
            coverLabel.fontSizeMax = 40f;
            coverLabel.alignment = TextAlignmentOptions.Center;

            var title = CreateText(card.transform, "Title", titleText, 19f, FontStyles.Bold, Hex("5A2C85"), new Vector2(.05f, .05f), new Vector2(.95f, .28f));
            title.enableAutoSizing = true;
            title.fontSizeMin = 14f;
            title.fontSizeMax = 21f;
            title.textWrappingMode = TextWrappingModes.Normal;
            title.alignment = TextAlignmentOptions.Center;

            return button;
        }

        static Button CreateColorBookCard(Transform parent, string name, string titleText, Vector2 min, Vector2 max)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), Hex("14798E"));
            shadow.raycastTarget = false;

            var card = CreatePanel(parent, name, min, max, Hex("FFF9FE", .98f));
            AddOutline(card.gameObject, Hex("30BFD0"), new Vector2(4f, -4f));
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();

            var cover = CreatePanel(card.transform, "BookCover", new Vector2(.15f, .31f), new Vector2(.85f, .92f), Hex("35BCD0"));
            AddShadow(cover.gameObject, new Vector2(5f, -6f), Hex("14798E"));
            AddOutline(cover.gameObject, Hex("FFF1B5"), new Vector2(2f, -2f));
            cover.raycastTarget = false;
            CreateImage(cover.transform, "Spine", new Vector2(.05f, .04f), new Vector2(.16f, .96f), Hex("14798E")).raycastTarget = false;
            CreateImage(cover.transform, "PageBlock", new Vector2(.18f, .08f), new Vector2(.92f, .18f), Hex("FFF7DC")).raycastTarget = false;

            Color[] swatches = { Hex("F04AA4"), Hex("F5BD38"), Hex("68C85E"), Hex("7B43CB") };
            Vector2[] mins = { new Vector2(.26f, .55f), new Vector2(.56f, .55f), new Vector2(.26f, .28f), new Vector2(.56f, .28f) };
            for (int i = 0; i < 4; i++)
            {
                var swatch = CreatePanel(cover.transform, "Swatch" + i, mins[i], mins[i] + new Vector2(.20f, .19f), swatches[i]);
                AddOutline(swatch.gameObject, Color.white, new Vector2(2f, -2f));
                swatch.raycastTarget = false;
            }

            var title = CreateText(card.transform, "Title", titleText, 18f, FontStyles.Bold, Hex("5A2C85"), new Vector2(.05f, .05f), new Vector2(.95f, .28f));
            title.enableAutoSizing = true;
            title.fontSizeMin = 13f;
            title.fontSizeMax = 20f;
            title.textWrappingMode = TextWrappingModes.Normal;
            title.alignment = TextAlignmentOptions.Center;

            return button;
        }

        static void BuildSelectionPanel(Transform parent, out TMP_Text titleText, out TMP_Text messageText)
        {
            var panel = CreatePanel(parent, "LibrarySelectionPanel", new Vector2(.405f, .145f), new Vector2(.955f, .325f), Hex("6133A9", .97f));
            AddShadow(panel.gameObject, new Vector2(0f, -8f), Hex("421E72", .34f));
            AddOutline(panel.gameObject, Hex("EFDFFF"), new Vector2(3f, -3f));

            titleText = CreateText(panel.transform, "SelectionTitle", "CHOOSE A BOOK SHELF", 25f, FontStyles.Bold, Hex("FFE36A"), new Vector2(.06f, .61f), new Vector2(.94f, .90f));
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 17f;
            titleText.fontSizeMax = 27f;
            titleText.alignment = TextAlignmentOptions.Center;

            messageText = CreateText(panel.transform, "SelectionMessage", "Pick something fun to read and learn with Journey!", 19f, FontStyles.Normal, Color.white, new Vector2(.07f, .10f), new Vector2(.93f, .61f));
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 14f;
            messageText.fontSizeMax = 20f;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildBottomNav(Transform parent, LibraryScreenControllerV1 controller)
        {
            var bar = CreatePanel(parent, "BottomNav", new Vector2(.02f, .02f), new Vector2(.98f, .125f), Hex("5D21AA", .98f));
            AddOutline(bar.gameObject, Hex("8D5BDD"), new Vector2(4f, -4f));

            var home = CreateButton(parent: bar.transform, name: "Home", label: "HOME", min: new Vector2(.02f, .08f), max: new Vector2(.245f, .92f), top: Hex("EF4C9D"), shadowColor: Hex("A21E6C"), fontSize: 22f);
            var library = CreateButton(parent: bar.transform, name: "Library", label: "LIBRARY", min: new Vector2(.26f, .08f), max: new Vector2(.485f, .92f), top: Hex("27B7E8"), shadowColor: Hex("137EAA"), fontSize: 22f);
            var rewards = CreateButton(parent: bar.transform, name: "Rewards", label: "REWARDS", min: new Vector2(.50f, .08f), max: new Vector2(.725f, .92f), top: Hex("F4A821"), shadowColor: Hex("B56C08"), fontSize: 22f);
            var parents = CreateButton(parent: bar.transform, name: "Parents", label: "PARENTS", min: new Vector2(.74f, .08f), max: new Vector2(.965f, .92f), top: Hex("8E44D4"), shadowColor: Hex("542483"), fontSize: 22f);

            AddOutline(library.gameObject, Hex("FFF4B0"), new Vector2(4f, -4f));
            UnityEventTools.AddPersistentListener(home.onClick, controller.GoHome);
            UnityEventTools.AddPersistentListener(rewards.onClick, controller.GoRewards);
            UnityEventTools.AddPersistentListener(parents.onClick, controller.GoParents);
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            return CreateImage(parent, name, min, max, color);
        }

        static Image CreateImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
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
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color top, Color shadowColor, float fontSize)
        {
            var shadow = CreateImage(parent, name + "Shadow", min + new Vector2(0f, -.006f), max + new Vector2(0f, -.006f), shadowColor);
            shadow.raycastTarget = false;

            var image = CreateImage(parent, name, min, max, top);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, .94f);
            colors.pressedColor = new Color(.90f, .90f, .90f, 1f);
            colors.disabledColor = new Color(.60f, .60f, .60f, .72f);
            button.colors = colors;

            var text = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f, .06f), new Vector2(.96f, .94f));
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(12f, fontSize * .55f);
            text.fontSizeMax = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void AddShadow(GameObject target, Vector2 distance, Color color)
        {
            var shadow = target.GetComponent<Shadow>();
            if (shadow == null) shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        static Texture2D FindJourneyTexture(out Rect uv)
        {
            uv = new Rect(0f, 0f, 1f, 1f);

            var clean = AssetDatabase.LoadAssetAtPath<Texture2D>(CleanJourneyPath);
            if (clean != null) return clean;

            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(JourneyAtlasPath);
            if (atlas != null)
            {
                uv = new Rect(0f, 2f / 3f, 1f / 5f, 1f / 3f);
                return atlas;
            }

            string[] candidates = AssetDatabase.FindAssets("Journey t:Texture2D");
            foreach (string guid in candidates)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                if (tex.width >= 600 && tex.height >= 600)
                {
                    uv = new Rect(0f, 2f / 3f, 1f / 5f, 1f / 3f);
                    return tex;
                }
                return tex;
            }
            return null;
        }

        static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
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
