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
    public static class LWJParentZoneBuilderV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ParentZone.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Parent Zone V3 - Clean Layout")]
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
                "Parent Zone V3 is ready with a cleaner hierarchy, larger readable controls, less duplicated information, a compact privacy note, and a protected parent gate.",
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
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            CreateImage(canvasGo.transform, "Background", Vector2.zero, Vector2.one, Hex("F8E4EF"));
            CreateImage(canvasGo.transform, "TopGlow", new Vector2(0f, .73f), new Vector2(1f, 1f), Hex("FFF8DF", .55f));
            CreateImage(canvasGo.transform, "BottomGlow", new Vector2(0f, 0f), new Vector2(1f, .18f), Hex("E4CCFF", .38f));

            var controllerGo = new GameObject("ParentZoneController");
            var controller = controllerGo.AddComponent<ParentZoneControllerV3>();

            BuildHeader(canvasGo.transform, out TMP_Text stars, out TMP_Text coins, out TMP_Text level);
            BuildTitle(canvasGo.transform);
            BuildProfile(canvasGo.transform, controller, journeyTexture, journeyUv,
                out TMP_Text profileName,
                out TMP_InputField childNameInput,
                out TMP_Text games,
                out TMP_Text streak,
                out TMP_Text speech);
            BuildProgress(canvasGo.transform, out TMP_Text abc, out TMP_Text counting, out TMP_Text matching,
                out Image abcFill, out Image countingFill, out Image matchingFill);
            BuildParentTools(canvasGo.transform, controller, out TMP_Text status, out TMP_Text resetText);
            BuildPrivacyNote(canvasGo.transform);
            BuildBottomNav(canvasGo.transform, controller);
            BuildParentGate(canvasGo.transform, controller, out GameObject gatePanel, out TMP_Text gateMessage);

            var so = new SerializedObject(controller);
            so.FindProperty("starsText").objectReferenceValue = stars;
            so.FindProperty("coinsText").objectReferenceValue = coins;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("profileNameText").objectReferenceValue = profileName;
            so.FindProperty("childNameInput").objectReferenceValue = childNameInput;
            so.FindProperty("gamesCompletedText").objectReferenceValue = games;
            so.FindProperty("streakText").objectReferenceValue = streak;
            so.FindProperty("abcCorrectText").objectReferenceValue = abc;
            so.FindProperty("countingCorrectText").objectReferenceValue = counting;
            so.FindProperty("alphabetPairsText").objectReferenceValue = matching;
            so.FindProperty("abcProgressFill").objectReferenceValue = abcFill;
            so.FindProperty("countingProgressFill").objectReferenceValue = countingFill;
            so.FindProperty("matchingProgressFill").objectReferenceValue = matchingFill;
            so.FindProperty("journeySpeechText").objectReferenceValue = speech;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("resetButtonText").objectReferenceValue = resetText;
            so.FindProperty("parentGatePanel").objectReferenceValue = gatePanel;
            so.FindProperty("parentGateMessageText").objectReferenceValue = gateMessage;
            so.ApplyModifiedPropertiesWithoutUndo();

            gatePanel.SetActive(false);
        }

        static void BuildHeader(Transform parent, out TMP_Text starsText, out TMP_Text coinsText, out TMP_Text levelText)
        {
            var stars = CreatePanel(parent, "StarsPill", new Vector2(.04f, .935f), new Vector2(.39f, .985f), Hex("EC3C95"));
            AddShadow(stars.gameObject, new Vector2(0f, -6f), Hex("9C1F65", .32f));
            AddOutline(stars.gameObject, Hex("FFB9DD"), new Vector2(3f, -3f));
            var starsLabel = CreateText(stars.transform, "Label", "STARS", 20f, FontStyles.Bold, Color.white, new Vector2(.08f, .12f), new Vector2(.55f, .88f));
            starsText = CreateText(stars.transform, "Value", "0", 34f, FontStyles.Bold, Color.white, new Vector2(.58f, .10f), new Vector2(.92f, .90f));
            starsLabel.alignment = starsText.alignment = TextAlignmentOptions.Center;

            var coins = CreatePanel(parent, "CoinsPill", new Vector2(.61f, .935f), new Vector2(.96f, .985f), Hex("6A38C8"));
            AddShadow(coins.gameObject, new Vector2(0f, -6f), Hex("3A1A7B", .32f));
            AddOutline(coins.gameObject, Hex("D8C6FF"), new Vector2(3f, -3f));
            var coinsLabel = CreateText(coins.transform, "Label", "JOURNEY COINS", 18f, FontStyles.Bold, Color.white, new Vector2(.05f, .12f), new Vector2(.67f, .88f));
            coinsText = CreateText(coins.transform, "Value", "0", 34f, FontStyles.Bold, Hex("FFE35C"), new Vector2(.68f, .10f), new Vector2(.95f, .90f));
            coinsLabel.alignment = coinsText.alignment = TextAlignmentOptions.Center;

            var level = CreatePanel(parent, "LevelPill", new Vector2(.39f, .895f), new Vector2(.61f, .93f), Hex("FFFFFF", .97f));
            AddOutline(level.gameObject, Hex("A979D8"), new Vector2(2f, -2f));
            levelText = CreateText(level.transform, "Value", "LEVEL 1", 20f, FontStyles.Bold, Hex("5C2F9E"), new Vector2(.05f, .08f), new Vector2(.95f, .92f));
            levelText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildTitle(Transform parent)
        {
            var title = CreateText(parent, "Title", "PARENT ZONE", 66f, FontStyles.Bold, Hex("54259A"), new Vector2(.10f, .82f), new Vector2(.90f, .89f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Color.white;
            title.outlineWidth = .20f;

            var ribbon = CreatePanel(parent, "Ribbon", new Vector2(.22f, .785f), new Vector2(.78f, .825f), Hex("2BBBC4"));
            AddShadow(ribbon.gameObject, new Vector2(0f, -4f), Hex("147B82", .28f));
            AddOutline(ribbon.gameObject, Hex("147F8B"), new Vector2(2f, -2f));
            var text = CreateText(ribbon.transform, "Text", "PROGRESS  •  SUPPORT  •  GROWTH", 20f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            text.alignment = TextAlignmentOptions.Center;
        }

        static void BuildProfile(Transform parent, ParentZoneControllerV3 controller, Texture2D texture, Rect uv,
            out TMP_Text profileName, out TMP_InputField childNameInput, out TMP_Text games, out TMP_Text streak, out TMP_Text speech)
        {
            var frame = CreatePanel(parent, "ProfileArea", new Vector2(.04f, .59f), new Vector2(.96f, .775f), Hex("FFFFFF", .95f));
            AddShadow(frame.gameObject, new Vector2(0f, -7f), Hex("6D4777", .18f));
            AddOutline(frame.gameObject, Hex("E2C6ED"), new Vector2(3f, -3f));

            var journey = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journey.transform.SetParent(frame.transform, false);
            var jr = (RectTransform)journey.transform;
            jr.anchorMin = new Vector2(.02f, .02f);
            jr.anchorMax = new Vector2(.30f, .86f);
            jr.offsetMin = jr.offsetMax = Vector2.zero;
            var raw = journey.GetComponent<RawImage>();
            raw.texture = texture;
            raw.uvRect = uv;
            raw.raycastTarget = false;
            raw.color = texture != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            var bag = CreatePanel(frame.transform, "JourneyBackpack", new Vector2(.205f, .10f), new Vector2(.285f, .40f), Hex("D93DA8"));
            AddOutline(bag.gameObject, Hex("FFD65A"), new Vector2(2f, -2f));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F35DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.14f, .08f), new Vector2(.86f, .42f), Hex("A92A91"));

            var bubble = CreatePanel(frame.transform, "Bubble", new Vector2(.015f, .70f), new Vector2(.31f, .97f), Color.white);
            AddOutline(bubble.gameObject, Hex("E74E9F"), new Vector2(2f, -2f));
            speech = CreateText(bubble.transform, "Speech", "Look how much I'm learning!", 19f, FontStyles.Bold, Hex("5B2D8D"), new Vector2(.08f, .10f), new Vector2(.92f, .90f));
            speech.enableAutoSizing = true;
            speech.fontSizeMin = 14f;
            speech.fontSizeMax = 20f;
            speech.textWrappingMode = TextWrappingModes.Normal;

            var profile = CreatePanel(frame.transform, "ProfileCard", new Vector2(.32f, .06f), new Vector2(.98f, .95f), Hex("FFF9FE"));
            AddOutline(profile.gameObject, Hex("9B63D8"), new Vector2(3f, -3f));
            var head = CreatePanel(profile.transform, "Header", new Vector2(.03f, .78f), new Vector2(.97f, .96f), Hex("6C3BC1"));
            var ht = CreateText(head.transform, "Text", "CHILD PROFILE", 26f, FontStyles.Bold, Color.white, new Vector2(.05f, .08f), new Vector2(.95f, .92f));
            ht.alignment = TextAlignmentOptions.Center;

            profileName = CreateText(profile.transform, "ProfileName", "Little Star", 29f, FontStyles.Bold, Hex("E63E98"), new Vector2(.06f, .58f), new Vector2(.94f, .76f));
            profileName.alignment = TextAlignmentOptions.Center;

            childNameInput = CreateInputField(profile.transform, "ChildNameInput", "Child name", new Vector2(.07f, .39f), new Vector2(.93f, .57f));

            BuildStat(profile.transform, "Games", "GAMES PLAYED", Hex("F04AA4"), new Vector2(.08f, .08f), new Vector2(.47f, .34f), out games);
            BuildStat(profile.transform, "Streak", "CURRENT STREAK", Hex("2DB8D7"), new Vector2(.53f, .08f), new Vector2(.92f, .34f), out streak);
        }

        static void BuildStat(Transform parent, string name, string label, Color accent, Vector2 min, Vector2 max, out TMP_Text value)
        {
            var card = CreatePanel(parent, name, min, max, Hex("FFFFFF"));
            AddOutline(card.gameObject, accent, new Vector2(2f, -2f));
            value = CreateText(card.transform, "Value", "0", 31f, FontStyles.Bold, accent, new Vector2(.05f, .35f), new Vector2(.95f, .90f));
            var t = CreateText(card.transform, "Label", label, 14f, FontStyles.Bold, Hex("66506C"), new Vector2(.05f, .05f), new Vector2(.95f, .35f));
            value.alignment = t.alignment = TextAlignmentOptions.Center;
        }

        static void BuildProgress(Transform parent, out TMP_Text abc, out TMP_Text counting, out TMP_Text matching,
            out Image abcFill, out Image countingFill, out Image matchingFill)
        {
            var heading = CreatePanel(parent, "ProgressHeading", new Vector2(.04f, .535f), new Vector2(.96f, .585f), Hex("6131B2"));
            AddOutline(heading.gameObject, Hex("8E61D6"), new Vector2(2f, -2f));
            var ht = CreateText(heading.transform, "Text", "LEARNING PROGRESS", 25f, FontStyles.Bold, Color.white, new Vector2(.05f, .08f), new Vector2(.95f, .92f));
            ht.alignment = TextAlignmentOptions.Center;

            BuildProgressCard(parent, "ABC", "ABC LEARNING", "LETTERS + SOUNDS", Hex("F04AA4"), new Vector2(.04f, .365f), new Vector2(.34f, .525f), out abc, out abcFill);
            BuildProgressCard(parent, "Counting", "COUNTING", "NUMBERS 1–20", Hex("F3A72C"), new Vector2(.35f, .365f), new Vector2(.65f, .525f), out counting, out countingFill);
            BuildProgressCard(parent, "Match", "ALPHABET MATCH", "LETTER PAIRS", Hex("2FB7DA"), new Vector2(.66f, .365f), new Vector2(.96f, .525f), out matching, out matchingFill);
        }

        static void BuildProgressCard(Transform parent, string name, string title, string subtitle, Color accent, Vector2 min, Vector2 max,
            out TMP_Text result, out Image fill)
        {
            var card = CreatePanel(parent, name + "Card", min, max, Hex("FFFFFF", .97f));
            AddShadow(card.gameObject, new Vector2(0f, -5f), Hex("5B4166", .14f));
            AddOutline(card.gameObject, accent, new Vector2(3f, -3f));

            var header = CreatePanel(card.transform, "Header", new Vector2(.03f, .72f), new Vector2(.97f, .96f), accent);
            var titleText = CreateText(header.transform, "Title", title, 19f, FontStyles.Bold, Color.white, new Vector2(.04f, .05f), new Vector2(.96f, .95f));
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 14f;
            titleText.fontSizeMax = 20f;

            result = CreateText(card.transform, "Result", "0 / 20", 28f, FontStyles.Bold, Hex("5D2FA3"), new Vector2(.05f, .35f), new Vector2(.95f, .69f));
            result.alignment = TextAlignmentOptions.Center;

            var sub = CreateText(card.transform, "Subtitle", subtitle, 13f, FontStyles.Bold, Hex("68566F"), new Vector2(.05f, .20f), new Vector2(.95f, .35f));
            sub.enableAutoSizing = true;
            sub.fontSizeMin = 10f;
            sub.fontSizeMax = 14f;

            var bar = CreatePanel(card.transform, "ProgressBack", new Vector2(.08f, .07f), new Vector2(.92f, .16f), Hex("EAE2F2"));
            fill = CreateImage(bar.transform, "Fill", new Vector2(.02f, .15f), new Vector2(.98f, .85f), accent);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;
        }

        static void BuildParentTools(Transform parent, ParentZoneControllerV3 controller, out TMP_Text status, out TMP_Text resetText)
        {
            var panel = CreatePanel(parent, "ParentTools", new Vector2(.04f, .195f), new Vector2(.96f, .345f), Hex("FFFFFF", .97f));
            AddOutline(panel.gameObject, Hex("A678DB"), new Vector2(3f, -3f));
            AddShadow(panel.gameObject, new Vector2(0f, -6f), Hex("5D3A70", .15f));

            var header = CreatePanel(panel.transform, "Header", new Vector2(.02f, .69f), new Vector2(.98f, .96f), Hex("6332B2"));
            var h = CreateText(header.transform, "Text", "PARENT TOOLS", 24f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            h.alignment = TextAlignmentOptions.Center;

            var edit = CreateButton(panel.transform, "EditName", "EDIT NAME", new Vector2(.03f, .30f), new Vector2(.25f, .63f), Hex("2BBCC4"), Hex("147B82"), 17f);
            var save = CreateButton(panel.transform, "SaveName", "SAVE NAME", new Vector2(.27f, .30f), new Vector2(.49f, .63f), Hex("28A8E2"), Hex("156A95"), 17f);
            var gate = CreateButton(panel.transform, "ParentGate", "PARENT GATE", new Vector2(.51f, .30f), new Vector2(.73f, .63f), Hex("7D43D2"), Hex("4B2685"), 16f);
            var reset = CreateButton(panel.transform, "Reset", "RESET PROGRESS", new Vector2(.75f, .30f), new Vector2(.97f, .63f), Hex("E9447B"), Hex("98264D"), 15f);

            UnityEventTools.AddPersistentListener(edit.onClick, controller.RequestEditName);
            UnityEventTools.AddPersistentListener(save.onClick, controller.SaveName);
            UnityEventTools.AddPersistentListener(gate.onClick, controller.OpenParentGate);
            UnityEventTools.AddPersistentListener(reset.onClick, controller.RequestResetProgress);
            resetText = reset.GetComponentInChildren<TextMeshProUGUI>();

            status = CreateText(panel.transform, "Status", "Profile stays on this device. Parent tools require a grown-up check.", 15f, FontStyles.Normal, Hex("604D67"), new Vector2(.05f, .05f), new Vector2(.95f, .26f));
            status.enableAutoSizing = true;
            status.fontSizeMin = 12f;
            status.fontSizeMax = 16f;
            status.textWrappingMode = TextWrappingModes.Normal;
        }

        static void BuildPrivacyNote(Transform parent)
        {
            var note = CreatePanel(parent, "PrivacyNote", new Vector2(.04f, .145f), new Vector2(.96f, .185f), Hex("F1E8FA"));
            AddOutline(note.gameObject, Hex("CDB6E3"), new Vector2(2f, -2f));
            var text = CreateText(note.transform, "Text", "PROFILE SAVED LOCALLY  •  GAMEPLAY STATS CAN STAY ANONYMOUS  •  PURCHASES REQUIRE A PARENT", 14f, FontStyles.Bold, Hex("5D3D7D"), new Vector2(.03f, .10f), new Vector2(.97f, .90f));
            text.enableAutoSizing = true;
            text.fontSizeMin = 10f;
            text.fontSizeMax = 15f;
            text.alignment = TextAlignmentOptions.Center;
        }

        static void BuildBottomNav(Transform parent, ParentZoneControllerV3 controller)
        {
            var bar = CreatePanel(parent, "BottomNav", new Vector2(.02f, .02f), new Vector2(.98f, .13f), Hex("5C20A9"));
            AddOutline(bar.gameObject, Hex("8B58D6"), new Vector2(4f, -4f));

            var home = CreateButton(bar.transform, "Home", "HOME", new Vector2(.02f, .08f), new Vector2(.245f, .92f), Hex("EF4C9D"), Hex("A21E6C"), 21f);
            var library = CreateButton(bar.transform, "Library", "LIBRARY", new Vector2(.26f, .08f), new Vector2(.485f, .92f), Hex("27B7E8"), Hex("137EAA"), 21f);
            var rewards = CreateButton(bar.transform, "Rewards", "REWARDS", new Vector2(.50f, .08f), new Vector2(.725f, .92f), Hex("F4A821"), Hex("B56C08"), 21f);
            var parents = CreateButton(bar.transform, "Parents", "PARENTS", new Vector2(.74f, .08f), new Vector2(.965f, .92f), Hex("8E44D4"), Hex("542483"), 21f);
            AddOutline(parents.gameObject, Hex("FFF0A8"), new Vector2(4f, -4f));

            UnityEventTools.AddPersistentListener(home.onClick, controller.GoHome);
            UnityEventTools.AddPersistentListener(library.onClick, controller.GoLibrary);
            UnityEventTools.AddPersistentListener(rewards.onClick, controller.GoRewards);
        }

        static void BuildParentGate(Transform parent, ParentZoneControllerV3 controller, out GameObject gatePanel, out TMP_Text gateMessage)
        {
            var overlay = CreateImage(parent, "ParentGateOverlay", Vector2.zero, Vector2.one, Hex("32194E", .78f));
            overlay.raycastTarget = true;
            gatePanel = overlay.gameObject;

            var card = CreatePanel(overlay.transform, "GateCard", new Vector2(.16f, .33f), new Vector2(.84f, .67f), Hex("FFF9FE"));
            AddShadow(card.gameObject, new Vector2(0f, -9f), Hex("21102F", .45f));
            AddOutline(card.gameObject, Hex("8E44D4"), new Vector2(4f, -4f));

            var title = CreateText(card.transform, "Title", "PARENT CHECK", 34f, FontStyles.Bold, Hex("6130A9"), new Vector2(.08f, .75f), new Vector2(.92f, .93f));
            title.alignment = TextAlignmentOptions.Center;

            gateMessage = CreateText(card.transform, "Message", "For grown-ups only.\n\nWhat is 4 + 3?", 22f, FontStyles.Bold, Hex("5D436B"), new Vector2(.08f, .42f), new Vector2(.92f, .73f));
            gateMessage.textWrappingMode = TextWrappingModes.Normal;
            gateMessage.alignment = TextAlignmentOptions.Center;

            var six = CreateButton(card.transform, "Six", "6", new Vector2(.10f, .18f), new Vector2(.34f, .38f), Hex("F04AA4"), Hex("A6266D"), 27f);
            var seven = CreateButton(card.transform, "Seven", "7", new Vector2(.38f, .18f), new Vector2(.62f, .38f), Hex("2BBCC4"), Hex("167D85"), 27f);
            var eight = CreateButton(card.transform, "Eight", "8", new Vector2(.66f, .18f), new Vector2(.90f, .38f), Hex("F4A821"), Hex("B66F0A"), 27f);
            UnityEventTools.AddIntPersistentListener(six.onClick, controller.AnswerParentGate, 6);
            UnityEventTools.AddIntPersistentListener(seven.onClick, controller.AnswerParentGate, 7);
            UnityEventTools.AddIntPersistentListener(eight.onClick, controller.AnswerParentGate, 8);

            var close = CreateButton(card.transform, "Close", "CANCEL", new Vector2(.35f, .04f), new Vector2(.65f, .14f), Hex("7A6B82"), Hex("4F4555"), 15f);
            UnityEventTools.AddPersistentListener(close.onClick, controller.CloseParentGate);
        }

        static TMP_InputField CreateInputField(Transform parent, string name, string placeholderValue, Vector2 min, Vector2 max)
        {
            var image = CreateImage(parent, name, min, max, Color.white);
            image.raycastTarget = true;
            AddOutline(image.gameObject, Hex("C9ACE2"), new Vector2(2f, -2f));
            var input = image.gameObject.AddComponent<TMP_InputField>();

            var viewportGo = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(image.transform, false);
            var viewport = (RectTransform)viewportGo.transform;
            viewport.anchorMin = new Vector2(.05f, .12f);
            viewport.anchorMax = new Vector2(.95f, .88f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;

            var placeholder = CreateText(viewport, "Placeholder", placeholderValue, 18f, FontStyles.Italic, Hex("8B7692", .72f), Vector2.zero, Vector2.one);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            var text = CreateText(viewport, "Text", "", 20f, FontStyles.Bold, Hex("4D315B"), Vector2.zero, Vector2.one);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;

            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 20;
            input.contentType = TMP_InputField.ContentType.Name;
            return input;
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
            var shadow = CreateImage(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), shadowColor);
            shadow.raycastTarget = false;
            var image = CreateImage(parent, name, min, max, top);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            var text = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f, .07f), new Vector2(.96f, .93f));
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(11f, fontSize * .62f);
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

            foreach (string guid in AssetDatabase.FindAssets("Journey t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                if (tex.width >= 600 && tex.height >= 600)
                    uv = new Rect(0f, 2f / 3f, 1f / 5f, 1f / 3f);
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
