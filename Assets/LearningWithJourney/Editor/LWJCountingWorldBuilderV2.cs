#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LearningWithJourney.Core;
using LearningWithJourney.Games;
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
    public static class LWJCountingWorldBuilderV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";
        const string GeneratedPath = "Assets/LearningWithJourney/Generated/Counting";
        const string RoundedPath = GeneratedPath + "/Rounded.png";
        const string CirclePath = GeneratedPath + "/Circle.png";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Rebuild Counting World V2")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            EnsureGeneratedSprites();

            Rect journeyUv;
            Texture2D journeyTexture = FindJourneyTexture(out journeyUv);

            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);

            BuildScene(journeyTexture, journeyUv);
            EnsureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            string journeyMessage = journeyTexture != null
                ? "Journey was found and placed on the Counting screen."
                : "Journey art was not found in the project. The Counting screen was rebuilt, but Journey still needs her texture imported.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World V2 is ready. " + journeyMessage + " The counting apples are now generated directly in Unity so they remain visible and do not depend on missing image files.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("2A1648");
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

            var controllerGo = new GameObject("CountingWorldController");
            var controller = controllerGo.AddComponent<CountingWorldPlayControllerV2>();

            BuildHeader(canvasGo.transform, controller, out TMP_Text pointsText, out TMP_Text levelText);

            var journeyRect = BuildJourneyArea(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speechText);
            BuildActivityArea(canvasGo.transform, out GameObject[] apples, out TMP_Text promptText);
            BuildAnswers(canvasGo.transform, out Button[] answers, out TMP_Text feedbackText, out TMP_Text roundText);

            var so = new SerializedObject(controller);
            SetObjectArray(so.FindProperty("countObjects"), apples.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("answerButtons"), answers.Cast<Object>().ToArray());
            so.FindProperty("promptText").objectReferenceValue = promptText;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            so.FindProperty("roundText").objectReferenceValue = roundText;
            so.FindProperty("pointsText").objectReferenceValue = pointsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("journeyRect").objectReferenceValue = journeyRect;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateRect(parent, "Wall", Vector2.zero, Vector2.one, Hex("F391BF"));
            CreateRect(parent, "WallGlow", new Vector2(0f, .45f), new Vector2(1f, 1f), new Color(1f, .88f, .96f, .15f));
            CreateRect(parent, "Floor", new Vector2(0f, 0f), new Vector2(1f, .36f), Hex("B85A30"));
            CreateRect(parent, "Baseboard", new Vector2(0f, .35f), new Vector2(1f, .375f), Hex("FFF0F7"));

            // A clean window gives classroom depth without crowding the learning area.
            var windowShadow = CreatePanel(parent, "WindowShadow", new Vector2(.025f, .52f), new Vector2(.33f, .72f), Hex("74304F", .22f));
            windowShadow.rectTransform.anchoredPosition += new Vector2(7f, -8f);
            var window = CreatePanel(parent, "Window", new Vector2(.02f, .53f), new Vector2(.32f, .73f), Hex("FFF5FA"));
            AddOutline(window.gameObject, Hex("F7CAE4"), new Vector2(3f, -3f));
            var sky = CreatePanel(window.transform, "Sky", new Vector2(.07f, .08f), new Vector2(.93f, .92f), Hex("59C8F4"));
            CreateRect(sky.transform, "WindowBarV", new Vector2(.485f, .04f), new Vector2(.515f, .96f), Color.white);
            CreateRect(sky.transform, "WindowBarH", new Vector2(.04f, .485f), new Vector2(.96f, .515f), Color.white);
            CreateImage(sky.transform, "CloudA", circle, new Vector2(.10f, .64f), new Vector2(.44f, .80f), new Color(1f, 1f, 1f, .90f));
            CreateImage(sky.transform, "CloudB", circle, new Vector2(.57f, .32f), new Vector2(.86f, .47f), new Color(1f, 1f, 1f, .80f));

            // A small shelf keeps the classroom visual language from the approved Main Menu.
            var shelf = CreatePanel(parent, "CountingShelf", new Vector2(.80f, .67f), new Vector2(.97f, .82f), Hex("B46A50"));
            AddShadow(shelf.gameObject, new Vector2(-5f, -6f), Hex("6A2D37", .22f));
            var cubby1 = CreatePanel(shelf.transform, "Cubby1", new Vector2(.07f, .54f), new Vector2(.93f, .91f), Hex("81412E"));
            var cubby2 = CreatePanel(shelf.transform, "Cubby2", new Vector2(.07f, .09f), new Vector2(.93f, .46f), Hex("81412E"));
            BuildMiniBooks(cubby1.transform);
            BuildMiniBins(cubby2.transform);

            // Subtle floor planks only.
            for (int i = 0; i < 4; i++)
            {
                float y = .04f + i * .065f;
                CreateRect(parent, "FloorBand" + i, new Vector2(.03f, y), new Vector2(.97f, y + .010f),
                    i % 2 == 0 ? Hex("7C351F", .18f) : Hex("F1A16B", .10f));
            }
        }

        static void BuildHeader(Transform parent, CountingWorldPlayControllerV2 controller, out TMP_Text pointsText, out TMP_Text levelText)
        {
            var back = CreateButton(parent, "BackButton", "<", new Vector2(.035f, .855f), new Vector2(.13f, .915f), Hex("6B23AE"), Hex("3A0E66"), 40f);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);

            pointsText = BuildPointsPill(parent);
            levelText = BuildLevelPill(parent);

            var title = CreateText(parent, "CountingTitle", "COUNT WITH JOURNEY", 54f, FontStyles.Bold,
                Color.white, new Vector2(.16f, .85f), new Vector2(.89f, .91f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("53117D");
            title.outlineWidth = .16f;

            var subtitle = CreateText(parent, "CountingSubtitle", "NUMBERS 1-20", 22f, FontStyles.Bold,
                Hex("6B238F"), new Vector2(.32f, .81f), new Vector2(.68f, .842f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static RectTransform BuildJourneyArea(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            // Journey is intentionally large and unobstructed on the left side.
            var go = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(.015f, .31f);
            rect.anchorMax = new Vector2(.42f, .69f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = go.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.color = Color.white;
            raw.texture = journeyTexture;
            raw.uvRect = journeyUv;

            if (journeyTexture == null)
            {
                raw.color = new Color(1f, 1f, 1f, 0f);
                var warning = CreatePanel(parent, "JourneyArtWarning", new Vector2(.055f, .39f), new Vector2(.35f, .57f), Hex("7C2DB0", .82f));
                var warningText = CreateText(warning.transform, "Text", "JOURNEY ART\nNEEDS IMPORT", 24f, FontStyles.Bold, Color.white,
                    new Vector2(.07f, .10f), new Vector2(.93f, .90f));
                warningText.enableWordWrapping = true;
                warningText.alignment = TextAlignmentOptions.Center;
            }

            // Backpack stays on only one leg so it does not cover Journey's body.
            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.30f, .325f), new Vector2(.385f, .405f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .52f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;

            var bubble = CreatePanel(parent, "JourneyCountingBubble", new Vector2(.10f, .675f), new Vector2(.49f, .755f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -7f), Hex("652276", .26f));
            AddOutline(bubble.gameObject, Hex("8A3AB6"), new Vector2(3f, -3f));
            speechText = CreateText(bubble.transform, "SpeechText", "Count the apples with me!", 24f, FontStyles.Bold,
                Hex("55207B"), new Vector2(.06f, .10f), new Vector2(.94f, .90f));
            speechText.alignment = TextAlignmentOptions.Center;

            return rect;
        }

        static void BuildActivityArea(Transform parent, out GameObject[] apples, out TMP_Text promptText)
        {
            var card = CreatePanel(parent, "CountingActivityCard", new Vector2(.40f, .36f), new Vector2(.965f, .765f), Hex("6420A8", .97f));
            AddShadow(card.gameObject, new Vector2(0f, -13f), Hex("280543", .58f));
            AddOutline(card.gameObject, Hex("E4B9FF"), new Vector2(4f, -4f));

            var gloss = CreatePanel(card.transform, "TopGloss", new Vector2(.025f, .86f), new Vector2(.975f, .98f), new Color(1f, 1f, 1f, .10f));
            gloss.raycastTarget = false;

            promptText = CreateText(card.transform, "PromptText", "How many apples do you see?", 28f, FontStyles.Bold,
                Color.white, new Vector2(.055f, .84f), new Vector2(.945f, .965f));
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.outlineColor = Hex("3B0A62");
            promptText.outlineWidth = .08f;

            var board = CreatePanel(card.transform, "ObjectBoard", new Vector2(.055f, .07f), new Vector2(.945f, .82f), Hex("FFF8FC"));
            AddShadow(board.gameObject, new Vector2(0f, -7f), Hex("3A0E5F", .24f));
            AddOutline(board.gameObject, Color.white, new Vector2(3f, -3f));

            var gridGo = new GameObject("ObjectGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(board.transform, false);
            var gridRect = (RectTransform)gridGo.transform;
            gridRect.anchorMin = new Vector2(.045f, .065f);
            gridRect.anchorMax = new Vector2(.955f, .94f);
            gridRect.offsetMin = gridRect.offsetMax = Vector2.zero;

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(82f, 82f);
            grid.spacing = new Vector2(8f, 9f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            apples = new GameObject[20];
            for (int i = 0; i < apples.Length; i++)
                apples[i] = BuildApple(gridGo.transform, i);
        }

        static void BuildAnswers(Transform parent, out Button[] answers, out TMP_Text feedbackText, out TMP_Text roundText)
        {
            var instruction = CreateText(parent, "AnswerInstruction", "TAP THE CORRECT NUMBER", 24f, FontStyles.Bold,
                Hex("5E1B86"), new Vector2(.20f, .305f), new Vector2(.90f, .342f));
            instruction.alignment = TextAlignmentOptions.Center;

            var a = CreateButton(parent, "AnswerA", "1", new Vector2(.14f, .205f), new Vector2(.385f, .292f), Hex("F23B98"), Hex("AE175F"), 48f);
            var b = CreateButton(parent, "AnswerB", "2", new Vector2(.395f, .205f), new Vector2(.64f, .292f), Hex("FFAF27"), Hex("C66B04"), 48f);
            var c = CreateButton(parent, "AnswerC", "3", new Vector2(.65f, .205f), new Vector2(.895f, .292f), Hex("23BDE4"), Hex("087B9B"), 48f);
            answers = new[] { a, b, c };

            feedbackText = CreateText(parent, "FeedbackText", "", 27f, FontStyles.Bold, Hex("62208B"),
                new Vector2(.14f, .155f), new Vector2(.86f, .198f));
            feedbackText.alignment = TextAlignmentOptions.Center;

            roundText = CreateText(parent, "RoundText", "ROUND 1 / 5", 20f, FontStyles.Bold, Hex("7D389C"),
                new Vector2(.35f, .115f), new Vector2(.65f, .15f));
            roundText.alignment = TextAlignmentOptions.Center;
        }

        static GameObject BuildApple(Transform parent, int index)
        {
            var root = new GameObject("Apple" + (index + 1), typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var body = CreateImage(root.transform, "Body", circle, new Vector2(.10f, .08f), new Vector2(.90f, .84f),
                index % 3 == 0 ? Hex("F04478") : index % 3 == 1 ? Hex("EF4A91") : Hex("E93D67"));
            AddShadow(body.gameObject, new Vector2(0f, -4f), Hex("7C163D", .26f));

            var body2 = CreateImage(root.transform, "BodySide", circle, new Vector2(.40f, .08f), new Vector2(.93f, .82f),
                index % 2 == 0 ? Hex("F04C7D", .92f) : Hex("F05A96", .92f));
            body2.raycastTarget = false;

            var stem = CreatePanel(root.transform, "Stem", new Vector2(.48f, .72f), new Vector2(.56f, .93f), Hex("7A4A2A"));
            stem.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -5f);
            stem.raycastTarget = false;

            var leaf = CreatePanel(root.transform, "Leaf", new Vector2(.55f, .71f), new Vector2(.79f, .88f), Hex("67BA4B"));
            leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            leaf.raycastTarget = false;

            var shine = CreateImage(root.transform, "Shine", circle, new Vector2(.25f, .54f), new Vector2(.40f, .70f), new Color(1f, 1f, 1f, .38f));
            shine.raycastTarget = false;
            return root;
        }

        static void BuildMiniBooks(Transform parent)
        {
            Color[] colors = { Hex("F24996"), Hex("23B9E2"), Hex("FFAA28"), Hex("7BCB4E") };
            for (int i = 0; i < 4; i++)
            {
                float x = .08f + i * .22f;
                CreatePanel(parent, "Book" + i, new Vector2(x, .14f), new Vector2(x + .13f, .84f), colors[i]);
            }
        }

        static void BuildMiniBins(Transform parent)
        {
            var a = CreatePanel(parent, "BinA", new Vector2(.08f, .15f), new Vector2(.44f, .82f), Hex("F15AA4"));
            var b = CreatePanel(parent, "BinB", new Vector2(.56f, .15f), new Vector2(.92f, .82f), Hex("26BDE4"));
            var ta = CreateText(a.transform, "Label", "ABC", 15f, FontStyles.Bold, Color.white, new Vector2(.06f, .10f), new Vector2(.94f, .90f));
            var tb = CreateText(b.transform, "Label", "123", 15f, FontStyles.Bold, Color.white, new Vector2(.06f, .10f), new Vector2(.94f, .90f));
            ta.alignment = tb.alignment = TextAlignmentOptions.Center;
        }

        static TMP_Text BuildPointsPill(Transform parent)
        {
            var pill = CreatePanel(parent, "PointsPill", new Vector2(.035f, .935f), new Vector2(.30f, .982f), Hex("F02F8E"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("73134E", .56f));
            AddOutline(pill.gameObject, Hex("FFD6EC"), new Vector2(3f, -3f));
            var label = CreateText(pill.transform, "Label", "POINTS", 15f, FontStyles.Bold, Color.white, new Vector2(.08f, .48f), new Vector2(.62f, .88f));
            label.alignment = TextAlignmentOptions.Left;
            var count = CreateText(pill.transform, "Count", "0", 27f, FontStyles.Bold, Color.white, new Vector2(.62f, .12f), new Vector2(.94f, .88f));
            count.alignment = TextAlignmentOptions.Center;
            return count;
        }

        static TMP_Text BuildLevelPill(Transform parent)
        {
            var pill = CreatePanel(parent, "LevelPill", new Vector2(.70f, .935f), new Vector2(.965f, .982f), Hex("7025B8"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("2D0A53", .58f));
            AddOutline(pill.gameObject, Hex("E7C8FF"), new Vector2(3f, -3f));
            var level = CreateText(pill.transform, "Level", "LEVEL 1", 22f, FontStyles.Bold, Color.white, new Vector2(.08f, .10f), new Vector2(.92f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            return level;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color main, Color depth, float fontSize)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0f, -.010f), max + new Vector2(0f, -.010f), depth);
            shadow.raycastTarget = false;

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = main;
            AddOutline(go, Color.white, new Vector2(3f, -3f));
            AddShadow(go, new Vector2(0f, -5f), new Color(depth.r, depth.g, depth.b, .32f));

            var gloss = CreatePanel(go.transform, "Gloss", new Vector2(.05f, .66f), new Vector2(.95f, .93f), new Color(1f, 1f, 1f, .22f));
            gloss.raycastTarget = false;
            var text = CreateText(go.transform, "Label", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            text.alignment = TextAlignmentOptions.Center;
            text.outlineColor = new Color(depth.r, depth.g, depth.b, .55f);
            text.outlineWidth = .07f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
            => CreateImage(parent, name, rounded, min, max, color);

        static Image CreateRect(Transform parent, string name, Vector2 min, Vector2 max, Color color)
            => CreateImage(parent, name, null, min, max, color);

        static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == rounded && rounded != null ? Image.Type.Sliced : Image.Type.Simple;
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
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.enableWordWrapping = false;
            text.extraPadding = true;
            text.raycastTarget = false;
            return text;
        }

        static Texture2D FindJourneyTexture(out Rect uv)
        {
            uv = new Rect(0f, 0f, 1f, 1f);

            var exact = AssetDatabase.LoadAssetAtPath<Texture2D>(CleanJourneyPath);
            if (exact != null) return exact;

            string[] preferred = AssetDatabase.FindAssets("JourneyMenuCleanFixed t:Texture2D");
            foreach (string guid in preferred)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                if (tex != null) return tex;
            }

            string[] all = AssetDatabase.FindAssets("Journey t:Texture2D");
            Texture2D fallback = null;
            foreach (string guid in all)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                string lower = path.ToLowerInvariant();
                if (lower.Contains("clean") || lower.Contains("fixed"))
                    return tex;

                if (lower.Contains("atlas"))
                {
                    uv = new Rect(0f, 2f / 3f, .2f, 1f / 3f);
                    return tex;
                }

                fallback ??= tex;
            }

            return fallback;
        }

        static void EnsureGeneratedSprites()
        {
            Directory.CreateDirectory(GeneratedPath);

            if (!File.Exists(RoundedPath)) MakeRoundedTexture(RoundedPath, 128, 28);
            if (!File.Exists(CirclePath)) MakeCircleTexture(CirclePath, 128);

            AssetDatabase.Refresh();
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
        }

        static void MakeRoundedTexture(string path, int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Max(radius - x, 0, x - (size - 1 - radius));
                    int dy = Mathf.Max(radius - y, 0, y - (size - 1 - radius));
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    tex.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ConfigureSprite(path, new Vector4(radius, radius, radius, radius));
        }

        static void MakeCircleTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * .5f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float a = Mathf.Clamp01(radius - d + 1f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ConfigureSprite(path, Vector4.zero);
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

        static void SetObjectArray(SerializedProperty property, Object[] objects)
        {
            property.arraySize = objects.Length;
            for (int i = 0; i < objects.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
        }

        static void EnsureBuildSettings()
        {
            var paths = EditorBuildSettings.scenes.Select(s => s.path).ToList();
            if (!paths.Contains(ScenePath)) paths.Add(ScenePath);
            EditorBuildSettings.scenes = paths.Where(File.Exists).Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
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
