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
    public static class LWJParentZoneBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ParentZone.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Parent Zone V1")]
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
                "Parent Zone V1 is ready. Parents can review Journey's learning progress, level progress, streaks, games completed, ABC/counting/matching results, save the child profile name, and safely reset progress with a two-tap confirmation.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("4D286F");
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            BuildBackground(canvasGo.transform);

            var controllerGo = new GameObject("ParentZoneController");
            var controller = controllerGo.AddComponent<ParentZoneControllerV1>();

            BuildHeader(canvasGo.transform, out TMP_Text stars, out TMP_Text coins, out TMP_Text level);
            BuildTitle(canvasGo.transform);
            BuildJourney(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speech);

            BuildSnapshot(canvasGo.transform,
                out TMP_Text profileName,
                out TMP_Text games,
                out TMP_Text streak,
                out TMP_Text bestStreak,
                out Image progressFill,
                out TMP_Text progressText);

            BuildLearningCards(canvasGo.transform,
                out TMP_Text abc,
                out TMP_Text counting,
                out TMP_Text matching);

            BuildParentTools(canvasGo.transform, controller,
                out TMP_InputField nameInput,
                out TMP_Text status,
                out TMP_Text resetText);

            BuildBottomNav(canvasGo.transform, controller);

            var so = new SerializedObject(controller);
            so.FindProperty("starsText").objectReferenceValue = stars;
            so.FindProperty("coinsText").objectReferenceValue = coins;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("childNameInput").objectReferenceValue = nameInput;
            so.FindProperty("profileNameText").objectReferenceValue = profileName;
            so.FindProperty("gamesCompletedText").objectReferenceValue = games;
            so.FindProperty("streakText").objectReferenceValue = streak;
            so.FindProperty("bestStreakText").objectReferenceValue = bestStreak;
            so.FindProperty("levelProgressFill").objectReferenceValue = progressFill;
            so.FindProperty("levelProgressText").objectReferenceValue = progressText;
            so.FindProperty("abcCorrectText").objectReferenceValue = abc;
            so.FindProperty("countingCorrectText").objectReferenceValue = counting;
            so.FindProperty("alphabetPairsText").objectReferenceValue = matching;
            so.FindProperty("journeySpeechText").objectReferenceValue = speech;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("resetButtonText").objectReferenceValue = resetText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "ParentWall", Vector2.zero, Vector2.one, Hex("F7DCEC"));
            CreateImage(parent, "WarmGlow", new Vector2(0f, .44f), new Vector2(1f, 1f), Hex("FFF6DB", .46f));
            CreateImage(parent, "LowerFloor", new Vector2(0f, 0f), new Vector2(1f, .33f), Hex("C98A67"));
            CreateImage(parent, "ParentRug", new Vector2(.03f, .08f), new Vector2(.97f, .36f), Hex("8B58D0", .88f));

            var frame = CreatePanel(parent, "DashboardArch", new Vector2(.025f, .135f), new Vector2(.975f, .79f), Hex("FFF9F3", .60f));
            AddOutline(frame.gameObject, Hex("D7A7C8", .80f), new Vector2(3f, -3f));
            frame.raycastTarget = false;

            CreateImage(parent, "TopGoldRail", new Vector2(.03f, .788f), new Vector2(.97f, .801f), Hex("D9A23D"));
            CreateImage(parent, "TopGoldGlow", new Vector2(.05f, .801f), new Vector2(.95f, .808f), Hex("FFF0A0", .9f));
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
            var title = CreateText(parent, "ParentTitle", "PARENT ZONE", 66f, FontStyles.Bold, Color.white, new Vector2(.10f, .805f), new Vector2(.90f, .89f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("54228E");
            title.outlineWidth = .26f;

            var ribbon = CreatePanel(parent, "ParentRibbon", new Vector2(.22f, .77f), new Vector2(.78f, .812f), Hex("31BFC5"));
            AddOutline(ribbon.gameObject, Hex("167F8C"), new Vector2(3f, -3f));
            var subtitle = CreateText(ribbon.transform, "Subtitle", "PROGRESS  •  SUPPORT  •  GROWTH", 21f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static void BuildJourney(Transform parent, Texture2D texture, Rect uv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .465f);
            rect.anchorMax = new Vector2(.34f, .735f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.texture = texture;
            raw.uvRect = uv;
            raw.raycastTarget = false;
            raw.color = texture != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.235f, .492f), new Vector2(.325f, .572f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -5f), Hex("4B125B", .48f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 24f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;

            var bubble = CreatePanel(parent, "ParentBubble", new Vector2(.035f, .69f), new Vector2(.36f, .765f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -5f), Hex("67357C", .20f));
            AddOutline(bubble.gameObject, Hex("8D4CC3"), new Vector2(3f, -3f));
            speechText = CreateText(bubble.transform, "Speech", "Look how much I'm learning!", 21f, FontStyles.Bold, Hex("593078"), new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            speechText.enableAutoSizing = true;
            speechText.fontSizeMin = 15f;
            speechText.fontSizeMax = 22f;
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildSnapshot(Transform parent,
            out TMP_Text profileName,
            out TMP_Text games,
            out TMP_Text streak,
            out TMP_Text bestStreak,
            out Image progressFill,
            out TMP_Text progressText)
        {
            var panel = CreatePanel(parent, "LearningSnapshot", new Vector2(.35f, .465f), new Vector2(.955f, .755f), Hex("FFFFFF", .97f));
            AddShadow(panel.gameObject, new Vector2(7f, -9f), Hex("57346E", .22f));
            AddOutline(panel.gameObject, Hex("B985D9"), new Vector2(3f, -3f));

            var heading = CreateText(panel.transform, "Heading", "LEARNING SNAPSHOT", 28f, FontStyles.Bold, Hex("6031A3"), new Vector2(.05f, .83f), new Vector2(.95f, .96f));
            heading.alignment = TextAlignmentOptions.Center;

            profileName = CreateText(panel.transform, "ProfileName", "Little Star", 31f, FontStyles.Bold, Hex("E83F9C"), new Vector2(.07f, .67f), new Vector2(.93f, .82f));
            profileName.enableAutoSizing = true;
            profileName.fontSizeMin = 22f;
            profileName.fontSizeMax = 32f;

            BuildMiniStat(panel.transform, "GamesStat", "GAMES", new Vector2(.05f, .39f), new Vector2(.34f, .65f), Hex("F04AA4"), out games);
            BuildMiniStat(panel.transform, "StreakStat", "CURRENT STREAK", new Vector2(.355f, .39f), new Vector2(.65f, .65f), Hex("35B8D7"), out streak);
            BuildMiniStat(panel.transform, "BestStat", "BEST STREAK", new Vector2(.665f, .39f), new Vector2(.95f, .65f), Hex("F4A62A"), out bestStreak);

            var barBack = CreatePanel(panel.transform, "LevelProgressBack", new Vector2(.08f, .20f), new Vector2(.92f, .30f), Hex("E8DFF3"));
            AddOutline(barBack.gameObject, Hex("C3A3D9"), new Vector2(2f, -2f));
            progressFill = CreateImage(barBack.transform, "Fill", new Vector2(.02f, .16f), new Vector2(.98f, .84f), Hex("8E44D4"));
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;
            progressFill.raycastTarget = false;

            progressText = CreateText(panel.transform, "ProgressText", "0 / 50 STARS TOWARD LEVEL 2", 17f, FontStyles.Bold, Hex("5A3B72"), new Vector2(.05f, .05f), new Vector2(.95f, .19f));
            progressText.enableAutoSizing = true;
            progressText.fontSizeMin = 13f;
            progressText.fontSizeMax = 18f;
            progressText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildMiniStat(Transform parent, string name, string label, Vector2 min, Vector2 max, Color accent, out TMP_Text value)
        {
            var card = CreatePanel(parent, name, min, max, Hex("FFF8FD"));
            AddOutline(card.gameObject, accent, new Vector2(2f, -2f));
            value = CreateText(card.transform, "Value", "0", 34f, FontStyles.Bold, accent, new Vector2(.06f, .40f), new Vector2(.94f, .90f));
            value.alignment = TextAlignmentOptions.Center;
            var t = CreateText(card.transform, "Label", label, 13f, FontStyles.Bold, Hex("654B72"), new Vector2(.05f, .08f), new Vector2(.95f, .42f));
            t.enableAutoSizing = true;
            t.fontSizeMin = 10f;
            t.fontSizeMax = 14f;
            t.alignment = TextAlignmentOptions.Center;
        }

        static void BuildLearningCards(Transform parent, out TMP_Text abc, out TMP_Text counting, out TMP_Text matching)
        {
            var header = CreateText(parent, "ProgressHeading", "LEARNING PROGRESS", 27f, FontStyles.Bold, Hex("6031A3"), new Vector2(.07f, .435f), new Vector2(.93f, .468f));
            header.alignment = TextAlignmentOptions.Center;

            BuildLearningCard(parent, "ABCProgress", "ABC LEARNING", "LETTERS + SOUNDS", new Vector2(.045f, .30f), new Vector2(.34f, .43f), Hex("F04AA4"), out abc);
            BuildLearningCard(parent, "CountingProgress", "COUNTING", "NUMBERS 1–20", new Vector2(.352f, .30f), new Vector2(.647f, .43f), Hex("F4A62A"), out counting);
            BuildLearningCard(parent, "MatchingProgress", "ALPHABET MATCH", "LETTER PAIRS", new Vector2(.66f, .30f), new Vector2(.955f, .43f), Hex("35B8D7"), out matching);
        }

        static void BuildLearningCard(Transform parent, string name, string title, string subtitle, Vector2 min, Vector2 max, Color accent, out TMP_Text result)
        {
            var panel = CreatePanel(parent, name, min, max, Hex("FFFFFF", .98f));
            AddShadow(panel.gameObject, new Vector2(4f, -6f), Hex("5A3C67", .18f));
            AddOutline(panel.gameObject, accent, new Vector2(3f, -3f));
            CreateImage(panel.transform, "Accent", new Vector2(.04f, .76f), new Vector2(.96f, .94f), accent).raycastTarget = false;

            var h = CreateText(panel.transform, "Title", title, 18f, FontStyles.Bold, Color.white, new Vector2(.05f, .76f), new Vector2(.95f, .94f));
            h.enableAutoSizing = true;
            h.fontSizeMin = 13f;
            h.fontSizeMax = 19f;

            result = CreateText(panel.transform, "Result", "0 CORRECT", 25f, FontStyles.Bold, Hex("6031A3"), new Vector2(.05f, .32f), new Vector2(.95f, .72f));
            result.enableAutoSizing = true;
            result.fontSizeMin = 18f;
            result.fontSizeMax = 27f;

            var s = CreateText(panel.transform, "Subtitle", subtitle, 13f, FontStyles.Bold, Hex("735F7B"), new Vector2(.05f, .08f), new Vector2(.95f, .31f));
            s.enableAutoSizing = true;
            s.fontSizeMin = 10f;
            s.fontSizeMax = 14f;
        }

        static void BuildParentTools(Transform parent, ParentZoneControllerV1 controller,
            out TMP_InputField nameInput,
            out TMP_Text status,
            out TMP_Text resetText)
        {
            var panel = CreatePanel(parent, "ParentTools", new Vector2(.045f, .145f), new Vector2(.955f, .285f), Hex("6133A9", .97f));
            AddShadow(panel.gameObject, new Vector2(0f, -7f), Hex("421E72", .30f));
            AddOutline(panel.gameObject, Hex("EFDFFF"), new Vector2(3f, -3f));

            var heading = CreateText(panel.transform, "Heading", "PARENT TOOLS", 21f, FontStyles.Bold, Hex("FFE36A"), new Vector2(.04f, .73f), new Vector2(.29f, .94f));
            heading.alignment = TextAlignmentOptions.Left;

            nameInput = CreateInputField(panel.transform, "ChildNameInput", "Child name", new Vector2(.04f, .41f), new Vector2(.42f, .70f));
            var save = CreateButton(panel.transform, "SaveName", "SAVE NAME", new Vector2(.435f, .41f), new Vector2(.64f, .70f), Hex("30BFC5"), Hex("167F8C"), 17f);
            UnityEventTools.AddPersistentListener(save.onClick, controller.SaveChildName);

            var reset = CreateButton(panel.transform, "ResetProgress", "RESET PROGRESS", new Vector2(.665f, .41f), new Vector2(.96f, .70f), Hex("D94A68"), Hex("8A233C"), 16f);
            UnityEventTools.AddPersistentListener(reset.onClick, controller.RequestResetProgress);
            resetText = reset.GetComponentInChildren<TextMeshProUGUI>();

            status = CreateText(panel.transform, "Status", "Review progress, celebrate growth, and keep learning fun at home.", 15f, FontStyles.Normal, Color.white, new Vector2(.04f, .07f), new Vector2(.96f, .36f));
            status.enableAutoSizing = true;
            status.fontSizeMin = 11f;
            status.fontSizeMax = 16f;
            status.textWrappingMode = TextWrappingModes.Normal;
            status.alignment = TextAlignmentOptions.Center;
        }

        static TMP_InputField CreateInputField(Transform parent, string name, string placeholderValue, Vector2 min, Vector2 max)
        {
            var image = CreateImage(parent, name, min, max, Color.white);
            image.raycastTarget = true;
            AddOutline(image.gameObject, Hex("CBA9E5"), new Vector2(2f, -2f));
            var input = image.gameObject.AddComponent<TMP_InputField>();

            var viewportGo = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(image.transform, false);
            var viewport = (RectTransform)viewportGo.transform;
            viewport.anchorMin = new Vector2(.05f, .10f);
            viewport.anchorMax = new Vector2(.95f, .90f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;

            var placeholder = CreateText(viewport, "Placeholder", placeholderValue, 17f, FontStyles.Italic, Hex("8B7692", .75f), Vector2.zero, Vector2.one);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;

            var text = CreateText(viewport, "Text", "", 18f, FontStyles.Normal, Hex("4A3153"), Vector2.zero, Vector2.one);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;

            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 24;
            input.pointSize = 18f;
            return input;
        }

        static void BuildBottomNav(Transform parent, ParentZoneControllerV1 controller)
        {
            var bar = CreatePanel(parent, "BottomNav", new Vector2(.02f, .02f), new Vector2(.98f, .125f), Hex("5D21AA", .98f));
            AddOutline(bar.gameObject, Hex("8D5BDD"), new Vector2(4f, -4f));

            var home = CreateButton(bar.transform, "Home", "HOME", new Vector2(.02f, .08f), new Vector2(.245f, .92f), Hex("EF4C9D"), Hex("A21E6C"), 22f);
            var library = CreateButton(bar.transform, "Library", "LIBRARY", new Vector2(.26f, .08f), new Vector2(.485f, .92f), Hex("27B7E8"), Hex("137EAA"), 22f);
            var rewards = CreateButton(bar.transform, "Rewards", "REWARDS", new Vector2(.50f, .08f), new Vector2(.725f, .92f), Hex("F4A821"), Hex("B56C08"), 22f);
            var parents = CreateButton(bar.transform, "Parents", "PARENTS", new Vector2(.74f, .08f), new Vector2(.965f, .92f), Hex("8E44D4"), Hex("542483"), 22f);

            AddOutline(parents.gameObject, Hex("FFF4B0"), new Vector2(4f, -4f));
            UnityEventTools.AddPersistentListener(home.onClick, controller.GoHome);
            UnityEventTools.AddPersistentListener(library.onClick, controller.GoLibrary);
            UnityEventTools.AddPersistentListener(rewards.onClick, controller.GoRewards);
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color) => CreateImage(parent, name, min, max, color);

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
            text.fontSizeMin = Mathf.Max(11f, fontSize * .55f);
            text.fontSizeMax = fontSize;
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
