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
    public static class LWJABCWorldBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";
        const string GeneratedPath = "Assets/LearningWithJourney/Generated/ABC";
        const string RoundedPath = GeneratedPath + "/Rounded.png";
        const string CirclePath = GeneratedPath + "/Circle.png";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Build ABC World V1")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            EnsureGeneratedSprites();

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
            AssetDatabase.Refresh();

            string journeyMessage = journeyTexture != null
                ? "Journey was found and placed on the ABC screen."
                : "Journey art was not found. The ABC screen was built, but Journey still needs her texture imported.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V1 is ready. " + journeyMessage +
                " It uses the approved Counting World layout style with a different Alphabet Sky background, 10 levels, 5 successful rounds per level, letter recognition, lowercase matching, beginning-letter challenges, saved progress, Points, Journey, and her backpack in the approved cover position.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("3A2B79");
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

            BuildAlphabetSky(canvasGo.transform);

            var controllerGo = new GameObject("ABCWorldController");
            var controller = controllerGo.AddComponent<ABCWorldPlayControllerV1>();
            var audio = controllerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0f;

            BuildHeader(canvasGo.transform, controller, out TMP_Text pointsText, out TMP_Text levelText);
            RectTransform journeyRect = BuildJourneyArea(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speechText);
            BuildActivityArea(canvasGo.transform, out TMP_Text promptText, out TMP_Text focusLetterText, out TMP_Text wordText);
            BuildAnswers(canvasGo.transform, out Button[] answers, out TMP_Text feedbackText, out TMP_Text roundText);

            var so = new SerializedObject(controller);
            so.FindProperty("promptText").objectReferenceValue = promptText;
            so.FindProperty("focusLetterText").objectReferenceValue = focusLetterText;
            so.FindProperty("wordText").objectReferenceValue = wordText;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            so.FindProperty("roundText").objectReferenceValue = roundText;
            so.FindProperty("pointsText").objectReferenceValue = pointsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            SetObjectArray(so.FindProperty("answerButtons"), answers.Cast<Object>().ToArray());
            so.FindProperty("journeyRect").objectReferenceValue = journeyRect;
            so.FindProperty("letterAudioSource").objectReferenceValue = audio;
            so.FindProperty("totalLevels").intValue = 10;
            so.FindProperty("roundsPerLevel").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildAlphabetSky(Transform parent)
        {
            // ABC World has its own magical sky identity, separate from the Main Menu classroom
            // and Counting World's sunny garden.
            CreateRect(parent, "SkyBase", Vector2.zero, Vector2.one, Hex("7062D7"));
            CreateRect(parent, "SkyTop", new Vector2(0f, .58f), new Vector2(1f, 1f), Hex("75C7F4"));
            CreateRect(parent, "SkyGlow", new Vector2(0f, .43f), new Vector2(1f, .75f), new Color(1f, .84f, .96f, .18f));
            CreateRect(parent, "CloudFloor", new Vector2(0f, 0f), new Vector2(1f, .35f), Hex("8D73DD"));
            CreateRect(parent, "CloudFloorGlow", new Vector2(0f, .30f), new Vector2(1f, .375f), Hex("C6B3FF", .80f));

            BuildCloud(parent, "CloudA", new Vector2(.02f, .70f), new Vector2(.25f, .775f), .82f);
            BuildCloud(parent, "CloudB", new Vector2(.72f, .72f), new Vector2(.96f, .795f), .75f);
            BuildCloud(parent, "CloudC", new Vector2(.03f, .39f), new Vector2(.25f, .455f), .46f);

            // Rainbow bands sit high in the background and never overlap the learning board.
            CreateImage(parent, "RainbowOuter", circle, new Vector2(.36f, .69f), new Vector2(.78f, .88f), Hex("F15C9F", .70f));
            CreateImage(parent, "Rainbow2", circle, new Vector2(.385f, .705f), new Vector2(.755f, .865f), Hex("FFB33F", .82f));
            CreateImage(parent, "Rainbow3", circle, new Vector2(.41f, .72f), new Vector2(.73f, .85f), Hex("FFD859", .90f));
            CreateImage(parent, "Rainbow4", circle, new Vector2(.435f, .735f), new Vector2(.705f, .835f), Hex("65C979", .92f));
            CreateImage(parent, "Rainbow5", circle, new Vector2(.46f, .75f), new Vector2(.68f, .82f), Hex("5BB9E8", .94f));
            CreateImage(parent, "RainbowCutout", circle, new Vector2(.485f, .763f), new Vector2(.655f, .808f), Hex("79C9F3"));

            BuildLetterBlock(parent, "DecorA", "A", new Vector2(.055f, .50f), new Vector2(.145f, .565f), Hex("F25AA7"), -7f);
            BuildLetterBlock(parent, "DecorB", "B", new Vector2(.17f, .47f), new Vector2(.26f, .535f), Hex("FFAD3E"), 6f);
            BuildLetterBlock(parent, "DecorC", "C", new Vector2(.77f, .48f), new Vector2(.86f, .545f), Hex("54BEE8"), -5f);

            for (int i = 0; i < 4; i++)
            {
                float x = .08f + i * .26f;
                CreateImage(parent, "SkyDot" + i, circle, new Vector2(x, .82f), new Vector2(x + .025f, .835f), Color.white * new Color(1f, 1f, 1f, .55f));
            }
        }

        static void BuildHeader(Transform parent, ABCWorldPlayControllerV1 controller, out TMP_Text pointsText, out TMP_Text levelText)
        {
            var back = CreateButton(parent, "BackButton", "<", new Vector2(.035f, .855f), new Vector2(.13f, .915f), Hex("6941C6"), Hex("35206E"), 40f);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);

            pointsText = BuildPointsPill(parent);
            levelText = BuildLevelPill(parent);

            var title = CreateText(parent, "ABCTitle", "ABC WITH JOURNEY", 54f, FontStyles.Bold,
                Color.white, new Vector2(.16f, .85f), new Vector2(.89f, .91f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("493180");
            title.outlineWidth = .16f;

            var subtitle = CreateText(parent, "ABCSubtitle", "LETTERS A-Z  |  10 LEVELS", 20f, FontStyles.Bold,
                Hex("4E2A83"), new Vector2(.27f, .81f), new Vector2(.73f, .842f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static RectTransform BuildJourneyArea(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .31f);
            rect.anchorMax = new Vector2(.42f, .69f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.color = Color.white;
            raw.texture = journeyTexture;
            raw.uvRect = journeyUv;

            var fitter = journeyGo.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 128f / 220f;

            if (journeyTexture == null)
            {
                raw.color = new Color(1f, 1f, 1f, 0f);
                var warning = CreatePanel(parent, "JourneyArtWarning", new Vector2(.055f, .40f), new Vector2(.35f, .57f), Hex("6C43B5", .90f));
                var warningText = CreateText(warning.transform, "Text", "JOURNEY ART\nNEEDS IMPORT", 23f, FontStyles.Bold, Color.white,
                    new Vector2(.07f, .10f), new Vector2(.93f, .90f));
                warningText.enableWordWrapping = true;
                warningText.alignment = TextAlignmentOptions.Center;
            }

            // Approved backpack cover position from Main Menu / Counting World.
            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.255f, .335f), new Vector2(.355f, .425f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .52f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;

            if (journeyGo.transform.parent == bag.transform.parent)
                bag.transform.SetSiblingIndex(Mathf.Min(journeyGo.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));

            // Same clean left-side placement that was approved for Counting World.
            var bubble = CreatePanel(parent, "JourneyABCBubble", new Vector2(.045f, .690f), new Vector2(.385f, .770f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -7f), Hex("4B2B79", .26f));
            AddOutline(bubble.gameObject, Hex("8056C5"), new Vector2(3f, -3f));
            speechText = CreateText(bubble.transform, "SpeechText", "Let's learn our letters!", 22f, FontStyles.Bold,
                Hex("52317A"), new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            speechText.enableWordWrapping = true;
            speechText.alignment = TextAlignmentOptions.Center;
            speechText.margin = new Vector4(14f, 8f, 14f, 8f);
            bubble.raycastTarget = false;
            bubble.transform.SetAsLastSibling();

            return rect;
        }

        static void BuildActivityArea(Transform parent, out TMP_Text promptText, out TMP_Text focusLetterText, out TMP_Text wordText)
        {
            var card = CreatePanel(parent, "ABCActivityCard", new Vector2(.40f, .36f), new Vector2(.965f, .765f), Hex("5633A5", .98f));
            AddShadow(card.gameObject, new Vector2(0f, -13f), Hex("25114F", .58f));
            AddOutline(card.gameObject, Hex("D9C4FF"), new Vector2(4f, -4f));

            var gloss = CreatePanel(card.transform, "TopGloss", new Vector2(.025f, .86f), new Vector2(.975f, .98f), new Color(1f, 1f, 1f, .11f));
            gloss.raycastTarget = false;

            promptText = CreateText(card.transform, "PromptText", "Can you find the letter A?", 28f, FontStyles.Bold,
                Color.white, new Vector2(.055f, .84f), new Vector2(.945f, .965f));
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.enableWordWrapping = true;
            promptText.outlineColor = Hex("2B165A");
            promptText.outlineWidth = .08f;

            var board = CreatePanel(card.transform, "LetterBoard", new Vector2(.055f, .07f), new Vector2(.945f, .82f), Hex("FFF9FE"));
            AddShadow(board.gameObject, new Vector2(0f, -7f), Hex("291451", .24f));
            AddOutline(board.gameObject, Color.white, new Vector2(3f, -3f));

            var badge = CreateImage(board.transform, "LetterHalo", circle, new Vector2(.20f, .23f), new Vector2(.80f, .86f), Hex("EEE4FF"));
            AddOutline(badge.gameObject, Hex("CDB7F6"), new Vector2(3f, -3f));

            focusLetterText = CreateText(badge.transform, "FocusLetter", "A", 170f, FontStyles.Bold,
                Hex("6A36BB"), new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            focusLetterText.alignment = TextAlignmentOptions.Center;
            focusLetterText.enableAutoSizing = true;
            focusLetterText.fontSizeMin = 46f;
            focusLetterText.fontSizeMax = 170f;
            focusLetterText.outlineColor = Hex("FFFFFF");
            focusLetterText.outlineWidth = .10f;

            wordText = CreateText(board.transform, "WordText", "A is for Apple", 32f, FontStyles.Bold,
                Hex("5D3B85"), new Vector2(.07f, .055f), new Vector2(.93f, .22f));
            wordText.alignment = TextAlignmentOptions.Center;
            wordText.enableWordWrapping = true;
        }

        static void BuildAnswers(Transform parent, out Button[] answers, out TMP_Text feedbackText, out TMP_Text roundText)
        {
            var instruction = CreateText(parent, "AnswerInstruction", "TAP THE CORRECT LETTER", 24f, FontStyles.Bold,
                Hex("4E2A83"), new Vector2(.20f, .305f), new Vector2(.90f, .342f));
            instruction.alignment = TextAlignmentOptions.Center;

            var a = CreateButton(parent, "AnswerA", "A", new Vector2(.14f, .205f), new Vector2(.385f, .292f), Hex("F34A9C"), Hex("AE175F"), 48f);
            var b = CreateButton(parent, "AnswerB", "B", new Vector2(.395f, .205f), new Vector2(.64f, .292f), Hex("FFAA35"), Hex("B96A13"), 48f);
            var c = CreateButton(parent, "AnswerC", "C", new Vector2(.65f, .205f), new Vector2(.895f, .292f), Hex("39B7E3"), Hex("1C779A"), 48f);
            answers = new[] { a, b, c };

            feedbackText = CreateText(parent, "FeedbackText", "Level 1: practice A through C.", 22f, FontStyles.Bold,
                Hex("513181"), new Vector2(.12f, .155f), new Vector2(.88f, .198f));
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.enableWordWrapping = true;

            roundText = CreateText(parent, "RoundText", "ROUND 1 / 5", 20f, FontStyles.Bold, Hex("6B4497"),
                new Vector2(.35f, .115f), new Vector2(.65f, .15f));
            roundText.alignment = TextAlignmentOptions.Center;
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
            var pill = CreatePanel(parent, "LevelPill", new Vector2(.70f, .935f), new Vector2(.965f, .982f), Hex("6C43BF"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("2D175C", .58f));
            AddOutline(pill.gameObject, Hex("E7D9FF"), new Vector2(3f, -3f));
            var level = CreateText(pill.transform, "Level", "LEVEL 1 / 10", 19f, FontStyles.Bold, Color.white, new Vector2(.05f, .10f), new Vector2(.95f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            return level;
        }

        static void BuildLetterBlock(Transform parent, string name, string letter, Vector2 min, Vector2 max, Color color, float rotation)
        {
            var block = CreatePanel(parent, name, min, max, color);
            block.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            AddShadow(block.gameObject, new Vector2(0f, -5f), Hex("3C245F", .25f));
            AddOutline(block.gameObject, Color.white, new Vector2(2f, -2f));
            var text = CreateText(block.transform, "Letter", letter, 30f, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            text.alignment = TextAlignmentOptions.Center;
        }

        static void BuildCloud(Transform parent, string name, Vector2 min, Vector2 max, float alpha)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = min;
            rr.anchorMax = max;
            rr.offsetMin = rr.offsetMax = Vector2.zero;

            CreateImage(root.transform, "Puff1", circle, new Vector2(.00f, .08f), new Vector2(.48f, .88f), new Color(1f, 1f, 1f, alpha));
            CreateImage(root.transform, "Puff2", circle, new Vector2(.28f, .20f), new Vector2(.72f, 1.00f), new Color(1f, 1f, 1f, Mathf.Min(1f, alpha + .08f)));
            CreateImage(root.transform, "Puff3", circle, new Vector2(.55f, .07f), new Vector2(1.00f, .86f), new Color(1f, 1f, 1f, alpha));
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
            button.navigation = new Navigation { mode = Navigation.Mode.None };
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

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            if (go == null) return;
            var outline = go.AddComponent<Outline>();
            outline.effectDistance = distance;
            outline.effectColor = color;
            outline.useGraphicAlpha = true;
        }

        static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static void EnsureGeneratedSprites()
        {
            Directory.CreateDirectory(GeneratedPath);

            if (!File.Exists(RoundedPath))
                WriteRoundedTexture(RoundedPath, 64, 14);

            if (!File.Exists(CirclePath))
                WriteCircleTexture(CirclePath, 64);

            AssetDatabase.Refresh();
            ConfigureSprite(RoundedPath, new Vector4(14f, 14f, 14f, 14f));
            ConfigureSprite(CirclePath, Vector4.zero);

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
        }

        static void WriteRoundedTexture(string path, int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Max(Mathf.Max(radius - x, 0), x - (size - 1 - radius));
                    int dy = Mathf.Max(Mathf.Max(radius - y, 0), y - (size - 1 - radius));
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    tex.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void WriteCircleTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = (size - 1) * .5f;
            float r = c;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    tex.SetPixel(x, y, dx * dx + dy * dy <= r * r ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void ConfigureSprite(string path, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
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
            string fallbackPath = null;

            foreach (string guid in all)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                string lower = path.ToLowerInvariant();
                if (lower.Contains("clean") || lower.Contains("fixed"))
                    return tex;

                if (fallback == null)
                {
                    fallback = tex;
                    fallbackPath = path;
                }

                if (lower.Contains("atlas"))
                {
                    uv = new Rect(0f, 2f / 3f, .2f, 1f / 3f);
                    return tex;
                }
            }

            if (fallback != null && fallbackPath != null && fallbackPath.ToLowerInvariant().Contains("atlas"))
                uv = new Rect(0f, 2f / 3f, .2f, 1f / 3f);

            return fallback;
        }

        static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath))
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
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
