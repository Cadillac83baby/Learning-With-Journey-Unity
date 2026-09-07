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
    public static class LWJParentZoneBuilderV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ParentZone.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Parent Zone V2 - Polished")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Texture2D journeyTexture = FindJourneyTexture(out Rect journeyUv);

            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

            BuildScene(journeyTexture, journeyUv);
            EnsureBuildSettings();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Parent Zone V2 is ready with the polished child profile, learning progress cards, Parent Tools, privacy/purchase messaging, parent gate, and highlighted Parents navigation.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            new GameObject("Systems").AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("F7D9EA");
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

            var controllerGo = new GameObject("ParentZoneControllerV2");
            var controller = controllerGo.AddComponent<ParentZoneControllerV2>();

            BuildHeader(canvasGo.transform, out TMP_Text starsText, out TMP_Text coinsText, out TMP_Text levelText);
            BuildTitle(canvasGo.transform);
            BuildProfileArea(canvasGo.transform, journeyTexture, journeyUv,
                out TMP_Text profileNameText,
                out TMP_InputField childNameInput,
                out TMP_Text profileStarsText,
                out TMP_Text streakText,
                out TMP_Text currentLevelText,
                out TMP_Text journeySpeechText);

            BuildLearningProgress(canvasGo.transform,
                out TMP_Text abcCorrectText,
                out TMP_Text countingCorrectText,
                out TMP_Text alphabetPairsText,
                out Image abcProgressFill,
                out Image countingProgressFill,
                out Image matchingProgressFill);

            BuildParentTools(canvasGo.transform, controller,
                out Button editNameButton,
                out Button resetProgressButton,
                out TMP_Text resetButtonText,
                out TMP_Text statusText);

            BuildPrivacyPanel(canvasGo.transform);
            BuildBottomNav(canvasGo.transform, controller);
            BuildParentGate(canvasGo.transform, controller, out GameObject parentGatePanel, out TMP_Text gateMessage);

            var so = new SerializedObject(controller);
            so.FindProperty("starsText").objectReferenceValue = starsText;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("profileNameText").objectReferenceValue = profileNameText;
            so.FindProperty("childNameInput").objectReferenceValue = childNameInput;
            so.FindProperty("profileStarsText").objectReferenceValue = profileStarsText;
            so.FindProperty("streakText").objectReferenceValue = streakText;
            so.FindProperty("currentLevelText").objectReferenceValue = currentLevelText;
            so.FindProperty("abcCorrectText").objectReferenceValue = abcCorrectText;
            so.FindProperty("countingCorrectText").objectReferenceValue = countingCorrectText;
            so.FindProperty("alphabetPairsText").objectReferenceValue = alphabetPairsText;
            so.FindProperty("abcProgressFill").objectReferenceValue = abcProgressFill;
            so.FindProperty("countingProgressFill").objectReferenceValue = countingProgressFill;
            so.FindProperty("matchingProgressFill").objectReferenceValue = matchingProgressFill;
            so.FindProperty("editNameButton").objectReferenceValue = editNameButton;
            so.FindProperty("resetProgressButton").objectReferenceValue = resetProgressButton;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("journeySpeechText").objectReferenceValue = journeySpeechText;
            so.FindProperty("resetButtonText").objectReferenceValue = resetButtonText;
            so.FindProperty("parentGatePanel").objectReferenceValue = parentGatePanel;
            so.FindProperty("parentGateMessageText").objectReferenceValue = gateMessage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "Background", Vector2.zero, Vector2.one, Hex("F9E2EF"));
            CreateImage(parent, "TopGlow", new Vector2(0f, .76f), new Vector2(1f, 1f), Hex("FFF7FC", .65f));
            var body = CreatePanel(parent, "MainWhiteCard", new Vector2(.018f, .135f), new Vector2(.982f, .80f), Hex("FFFDFE", .97f));
            AddOutline(body.gameObject, Hex("E6B8D7"), new Vector2(3f, -3f));
            AddShadow(body.gameObject, new Vector2(0f, -7f), Hex("734A73", .15f));
            CreateImage(parent, "GoldRail", new Vector2(.045f, .795f), new Vector2(.955f, .807f), Hex("F7B51E"));
        }

        static void BuildHeader(Transform parent, out TMP_Text starsText, out TMP_Text coinsText, out TMP_Text levelText)
        {
            var stars = CreatePanel(parent, "StarsPill", new Vector2(.045f, .925f), new Vector2(.41f, .98f), Hex("EC3598"));
            AddShadow(stars.gameObject, new Vector2(0f, -6f), Hex("9E1A66", .4f));
            AddOutline(stars.gameObject, Hex("FFBFE2"), new Vector2(3f, -3f));
            CreateText(stars.transform, "Icon", "★", 34f, FontStyles.Bold, Hex("FFD43C"), new Vector2(.02f,.08f), new Vector2(.18f,.92f));
            CreateText(stars.transform, "Label", "STARS", 21f, FontStyles.Bold, Color.white, new Vector2(.18f,.08f), new Vector2(.68f,.92f));
            starsText = CreateText(stars.transform, "Count", "0", 34f, FontStyles.Bold, Color.white, new Vector2(.70f,.08f), new Vector2(.95f,.92f));

            var coins = CreatePanel(parent, "CoinsPill", new Vector2(.59f, .925f), new Vector2(.955f, .98f), Hex("7537D5"));
            AddShadow(coins.gameObject, new Vector2(0f, -6f), Hex("3C187D", .4f));
            AddOutline(coins.gameObject, Hex("C6A4FF"), new Vector2(3f, -3f));
            CreateText(coins.transform, "Icon", "●", 34f, FontStyles.Bold, Hex("FFD43C"), new Vector2(.02f,.08f), new Vector2(.18f,.92f));
            CreateText(coins.transform, "Label", "JOURNEY COINS", 18f, FontStyles.Bold, Color.white, new Vector2(.17f,.08f), new Vector2(.73f,.92f));
            coinsText = CreateText(coins.transform, "Count", "0", 34f, FontStyles.Bold, Color.white, new Vector2(.75f,.08f), new Vector2(.95f,.92f));

            var level = CreatePanel(parent, "LevelPill", new Vector2(.37f,.875f), new Vector2(.63f,.915f), Hex("FFF9FF"));
            AddOutline(level.gameObject, Hex("8D59D8"), new Vector2(2f,-2f));
            levelText = CreateText(level.transform, "Text", "LEVEL 1", 24f, FontStyles.Bold, Hex("5426A7"), new Vector2(.04f,.05f), new Vector2(.96f,.95f));
        }

        static void BuildTitle(Transform parent)
        {
            var title = CreateText(parent, "Title", "PARENT ZONE", 62f, FontStyles.Bold, Hex("5C2FB3"), new Vector2(.12f,.81f), new Vector2(.88f,.875f));
            title.outlineColor = Color.white;
            title.outlineWidth = .22f;
            var ribbon = CreatePanel(parent, "Ribbon", new Vector2(.20f,.772f), new Vector2(.80f,.81f), Hex("20BFC7"));
            AddOutline(ribbon.gameObject, Hex("128793"), new Vector2(3f,-3f));
            CreateText(ribbon.transform, "Text", "♥   PROGRESS  •  SUPPORT  •  GROWTH   ♥", 18f, FontStyles.Bold, Color.white, new Vector2(.03f,.05f), new Vector2(.97f,.95f));
        }

        static void BuildProfileArea(Transform parent, Texture2D texture, Rect uv,
            out TMP_Text profileNameText,
            out TMP_InputField childNameInput,
            out TMP_Text profileStarsText,
            out TMP_Text streakText,
            out TMP_Text currentLevelText,
            out TMP_Text journeySpeechText)
        {
            var bubble = CreatePanel(parent, "JourneyBubble", new Vector2(.035f,.665f), new Vector2(.285f,.755f), Color.white);
            AddOutline(bubble.gameObject, Hex("F04AA4"), new Vector2(3f,-3f));
            AddShadow(bubble.gameObject, new Vector2(0f,-5f), Hex("A63C77", .18f));
            journeySpeechText = CreateText(bubble.transform, "Speech", "Look how much I'm learning!", 20f, FontStyles.Bold, Hex("4F278D"), new Vector2(.06f,.08f), new Vector2(.94f,.92f));
            journeySpeechText.textWrappingMode = TextWrappingModes.Normal;
            journeySpeechText.enableAutoSizing = true;
            journeySpeechText.fontSizeMin = 15f;
            journeySpeechText.fontSizeMax = 21f;

            var journey = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journey.transform.SetParent(parent, false);
            var jr = (RectTransform)journey.transform;
            jr.anchorMin = new Vector2(.035f,.49f);
            jr.anchorMax = new Vector2(.29f,.665f);
            jr.offsetMin = jr.offsetMax = Vector2.zero;
            var raw = journey.GetComponent<RawImage>();
            raw.texture = texture;
            raw.uvRect = uv;
            raw.raycastTarget = false;
            raw.color = texture != null ? Color.white : new Color(1,1,1,0);

            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.205f,.505f), new Vector2(.282f,.575f), Hex("D83BA7"));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f,-3f));
            AddShadow(bag.gameObject, new Vector2(0f,-5f), Hex("4B125B", .45f));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f,.52f), new Vector2(.92f,.87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f,.08f), new Vector2(.85f,.42f), Hex("A92A91"));

            var card = CreatePanel(parent, "ChildProfile", new Vector2(.30f,.49f), new Vector2(.955f,.755f), Color.white);
            AddOutline(card.gameObject, Hex("B985E4"), new Vector2(3f,-3f));
            AddShadow(card.gameObject, new Vector2(7f,-9f), Hex("57346E", .18f));

            var header = CreatePanel(card.transform, "Header", new Vector2(.04f,.80f), new Vector2(.96f,.96f), Hex("7540D3"));
            AddOutline(header.gameObject, Hex("4F2699"), new Vector2(2f,-2f));
            CreateText(header.transform, "Text", "●   CHILD PROFILE", 27f, FontStyles.Bold, Color.white, new Vector2(.03f,.06f), new Vector2(.97f,.94f));

            profileNameText = CreateText(card.transform, "ProfileName", "Child Name: Little Star", 24f, FontStyles.Bold, Hex("4D278D"), new Vector2(.08f,.65f), new Vector2(.92f,.78f));
            profileNameText.enableAutoSizing = true;
            profileNameText.fontSizeMin = 18f;
            profileNameText.fontSizeMax = 25f;

            childNameInput = CreateInputField(card.transform, "NameInput", "Child name", new Vector2(.13f,.54f), new Vector2(.87f,.64f));

            BuildStatTile(card.transform, "Stars", "★", "STARS EARNED", new Vector2(.07f,.26f), new Vector2(.35f,.51f), Hex("FFF4FA"), Hex("F3A5CB"), out profileStarsText);
            BuildStatTile(card.transform, "Streak", "♦", "CURRENT STREAK", new Vector2(.36f,.26f), new Vector2(.64f,.51f), Hex("F2FBFF"), Hex("9FD8F0"), out streakText);
            BuildStatTile(card.transform, "Level", "▥", "CURRENT LEVEL", new Vector2(.65f,.26f), new Vector2(.93f,.51f), Hex("FFF9EC"), Hex("F1C66D"), out currentLevelText);

            var privacy = CreatePanel(card.transform, "LocalOnly", new Vector2(.08f,.07f), new Vector2(.92f,.19f), Hex("E9D8FF"));
            CreateText(privacy.transform, "Text", "🔒  This profile is saved locally on this device.", 15f, FontStyles.Normal, Hex("5F438A"), new Vector2(.04f,.06f), new Vector2(.96f,.94f));
        }

        static void BuildStatTile(Transform parent, string name, string icon, string label, Vector2 min, Vector2 max, Color bg, Color outline, out TMP_Text value)
        {
            var tile = CreatePanel(parent, name, min, max, bg);
            AddOutline(tile.gameObject, outline, new Vector2(2f,-2f));
            CreateText(tile.transform, "Icon", icon, 30f, FontStyles.Bold, Hex("F0A514"), new Vector2(.10f,.57f), new Vector2(.90f,.90f));
            value = CreateText(tile.transform, "Value", "0", 30f, FontStyles.Bold, Hex("5B2DAA"), new Vector2(.10f,.30f), new Vector2(.90f,.60f));
            CreateText(tile.transform, "Label", label, 12f, FontStyles.Bold, Hex("573B75"), new Vector2(.05f,.05f), new Vector2(.95f,.30f));
        }

        static void BuildLearningProgress(Transform parent,
            out TMP_Text abcCorrect, out TMP_Text countingCorrect, out TMP_Text matchingCorrect,
            out Image abcFill, out Image countingFill, out Image matchingFill)
        {
            var heading = CreatePanel(parent, "LearningHeading", new Vector2(.035f,.445f), new Vector2(.965f,.485f), Hex("6C36C8"));
            AddOutline(heading.gameObject, Hex("4A1C98"), new Vector2(2f,-2f));
            CreateText(heading.transform, "Text", "▥   LEARNING PROGRESS", 24f, FontStyles.Bold, Color.white, new Vector2(.03f,.05f), new Vector2(.70f,.95f));
            CreateText(heading.transform, "Tip", "PRACTICE TODAY\nFOR A BRIGHTER TOMORROW! ♥", 12f, FontStyles.Bold, Color.white, new Vector2(.70f,.05f), new Vector2(.98f,.95f));

            BuildProgressCard(parent, "ABC", "ABC Learning", "ABC", Hex("F2459E"), Hex("FAD9EA"), new Vector2(.045f,.275f), new Vector2(.335f,.435f), out abcCorrect, out abcFill);
            BuildProgressCard(parent, "Counting", "Counting", "1 2 3", Hex("F6A515"), Hex("FCE9C4"), new Vector2(.355f,.275f), new Vector2(.645f,.435f), out countingCorrect, out countingFill);
            BuildProgressCard(parent, "Match", "Alphabet Match", "A B C", Hex("19A8EA"), Hex("D9F1FD"), new Vector2(.665f,.275f), new Vector2(.955f,.435f), out matchingCorrect, out matchingFill);
        }

        static void BuildProgressCard(Transform parent, string name, string title, string icon, Color accent, Color body, Vector2 min, Vector2 max, out TMP_Text result, out Image fill)
        {
            var card = CreatePanel(parent, name + "Card", min, max, Color.white);
            AddOutline(card.gameObject, accent, new Vector2(3f,-3f));
            AddShadow(card.gameObject, new Vector2(4f,-6f), Hex("6E5277", .12f));
            var head = CreatePanel(card.transform, "Header", new Vector2(.02f,.79f), new Vector2(.98f,.98f), accent);
            CreateText(head.transform, "Text", title, 20f, FontStyles.Bold, Color.white, new Vector2(.03f,.05f), new Vector2(.97f,.95f));
            CreateText(card.transform, "Icon", icon, 34f, FontStyles.Bold, accent, new Vector2(.08f,.53f), new Vector2(.92f,.78f));
            var resultBg = CreatePanel(card.transform, "ResultBg", new Vector2(.07f,.23f), new Vector2(.93f,.50f), body);
            result = CreateText(resultBg.transform, "Result", "0 / 20\nCorrect Answers", 18f, FontStyles.Bold, Hex("56309D"), new Vector2(.04f,.05f), new Vector2(.96f,.95f));
            result.textWrappingMode = TextWrappingModes.Normal;
            var track = CreatePanel(card.transform, "ProgressTrack", new Vector2(.08f,.07f), new Vector2(.92f,.17f), Hex("E7DDF0"));
            fill = CreateImage(track.transform, "Fill", new Vector2(.02f,.14f), new Vector2(.98f,.86f), accent);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;
        }

        static void BuildParentTools(Transform parent, ParentZoneControllerV2 controller, out Button editNameButton, out Button resetButton, out TMP_Text resetText, out TMP_Text statusText)
        {
            var heading = CreatePanel(parent, "ParentToolsHeading", new Vector2(.035f,.225f), new Vector2(.965f,.265f), Hex("6C36C8"));
            AddOutline(heading.gameObject, Hex("4A1C98"), new Vector2(2f,-2f));
            CreateText(heading.transform, "Text", "⚙   PARENT TOOLS", 24f, FontStyles.Bold, Color.white, new Vector2(.03f,.05f), new Vector2(.60f,.95f));
            CreateText(heading.transform, "Sub", "SUPPORT THEIR LEARNING JOURNEY ♥", 12f, FontStyles.Bold, Color.white, new Vector2(.58f,.05f), new Vector2(.98f,.95f));

            editNameButton = CreateToolButton(parent, "EditName", "✎", "Edit Name", "Personalize\nyour child's profile", new Vector2(.045f,.155f), new Vector2(.265f,.22f), Hex("25BFC8"));
            var saveButton = CreateToolButton(parent, "SaveProgress", "↓", "Save Progress", "Keep progress\non this device", new Vector2(.285f,.155f), new Vector2(.505f,.22f), Hex("27A9EA"));
            var gateButton = CreateToolButton(parent, "ParentGate", "🔒", "Parent Gate", "Kid-safe settings\nand links", new Vector2(.525f,.155f), new Vector2(.745f,.22f), Hex("7E42D6"));
            resetButton = CreateToolButton(parent, "ResetProgress", "↻", "Reset Progress", "Clear all progress\nfor a fresh start", new Vector2(.765f,.155f), new Vector2(.955f,.22f), Hex("EE3F83"));

            UnityEventTools.AddPersistentListener(editNameButton.onClick, controller.RequestEditName);
            UnityEventTools.AddPersistentListener(saveButton.onClick, controller.SaveProgress);
            UnityEventTools.AddPersistentListener(gateButton.onClick, controller.OpenParentGate);
            UnityEventTools.AddPersistentListener(resetButton.onClick, controller.RequestResetProgress);
            resetText = resetButton.GetComponentInChildren<TextMeshProUGUI>();

            statusText = CreateText(parent, "ParentStatus", "Child profile is saved locally on this device.", 13f, FontStyles.Normal, Hex("5F438A"), new Vector2(.08f,.137f), new Vector2(.92f,.153f));
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = 11f;
            statusText.fontSizeMax = 14f;
        }

        static Button CreateToolButton(Transform parent, string name, string icon, string title, string subtitle, Vector2 min, Vector2 max, Color accent)
        {
            var card = CreatePanel(parent, name + "Card", min, max, Hex("F7F4FB"));
            AddOutline(card.gameObject, Hex("D9C9E7"), new Vector2(2f,-2f));
            var top = CreatePanel(card.transform, "Button", new Vector2(.02f,.30f), new Vector2(.98f,.98f), accent);
            AddShadow(top.gameObject, new Vector2(0f,-5f), Hex("4C2A69", .22f));
            top.raycastTarget = true;
            var button = top.gameObject.AddComponent<Button>();
            CreateText(top.transform, "Icon", icon, 26f, FontStyles.Bold, Color.white, new Vector2(.05f,.48f), new Vector2(.95f,.92f));
            CreateText(top.transform, "Title", title, 16f, FontStyles.Bold, Color.white, new Vector2(.05f,.08f), new Vector2(.95f,.48f));
            var sub = CreateText(card.transform, "Subtitle", subtitle, 11f, FontStyles.Normal, Hex("5D4770"), new Vector2(.05f,.02f), new Vector2(.95f,.29f));
            sub.textWrappingMode = TextWrappingModes.Normal;
            return button;
        }

        static void BuildPrivacyPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "PrivacyPurchases", new Vector2(.035f,.085f), new Vector2(.965f,.135f), Hex("F5ECFF"));
            AddOutline(panel.gameObject, Hex("B999DC"), new Vector2(2f,-2f));
            CreateText(panel.transform, "Title", "◆  PRIVACY & PURCHASES", 16f, FontStyles.Bold, Hex("542B99"), new Vector2(.03f,.52f), new Vector2(.38f,.95f));
            CreateText(panel.transform, "One", "✓  Child profile is stored locally\non this device", 11f, FontStyles.Normal, Hex("5C4670"), new Vector2(.03f,.05f), new Vector2(.34f,.55f)).textWrappingMode = TextWrappingModes.Normal;
            CreateText(panel.transform, "Two", "✓  Gameplay stats are\nanonymous", 11f, FontStyles.Normal, Hex("5C4670"), new Vector2(.35f,.05f), new Vector2(.64f,.55f)).textWrappingMode = TextWrappingModes.Normal;
            CreateText(panel.transform, "Three", "✓  In-app purchases are\nmanaged by parents", 11f, FontStyles.Normal, Hex("5C4670"), new Vector2(.65f,.05f), new Vector2(.97f,.55f)).textWrappingMode = TextWrappingModes.Normal;
        }

        static void BuildBottomNav(Transform parent, ParentZoneControllerV2 controller)
        {
            var bar = CreatePanel(parent, "BottomNav", new Vector2(.015f,.012f), new Vector2(.985f,.078f), Hex("5A20B0"));
            AddOutline(bar.gameObject, Hex("7B4AD6"), new Vector2(3f,-3f));
            var home = CreateNavButton(bar.transform, "Home", "⌂\nHOME", new Vector2(.015f,.06f), new Vector2(.245f,.94f), Hex("EF4C9D"));
            var library = CreateNavButton(bar.transform, "Library", "▣\nLIBRARY", new Vector2(.255f,.06f), new Vector2(.485f,.94f), Hex("24AFE7"));
            var rewards = CreateNavButton(bar.transform, "Rewards", "★\nREWARDS", new Vector2(.495f,.06f), new Vector2(.725f,.94f), Hex("F3A91C"));
            var parents = CreateNavButton(bar.transform, "Parents", "●●\nPARENTS", new Vector2(.735f,.06f), new Vector2(.985f,.94f), Hex("7B3FD1"));
            AddOutline(parents.gameObject, Hex("FFE36A"), new Vector2(4f,-4f));
            UnityEventTools.AddPersistentListener(home.onClick, controller.GoHome);
            UnityEventTools.AddPersistentListener(library.onClick, controller.GoLibrary);
            UnityEventTools.AddPersistentListener(rewards.onClick, controller.GoRewards);
        }

        static Button CreateNavButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color)
        {
            var image = CreatePanel(parent, name, min, max, color);
            image.raycastTarget = true;
            AddShadow(image.gameObject, new Vector2(0f,-5f), Hex("35135F", .35f));
            var button = image.gameObject.AddComponent<Button>();
            var t = CreateText(image.transform, "Text", label, 15f, FontStyles.Bold, Color.white, new Vector2(.04f,.05f), new Vector2(.96f,.95f));
            t.textWrappingMode = TextWrappingModes.Normal;
            return button;
        }

        static void BuildParentGate(Transform parent, ParentZoneControllerV2 controller, out GameObject gatePanel, out TMP_Text message)
        {
            var overlay = CreatePanel(parent, "ParentGateOverlay", Vector2.zero, Vector2.one, Hex("2B123D", .74f));
            overlay.raycastTarget = true;
            gatePanel = overlay.gameObject;

            var modal = CreatePanel(overlay.transform, "Modal", new Vector2(.15f,.34f), new Vector2(.85f,.66f), Color.white);
            AddOutline(modal.gameObject, Hex("7B42D3"), new Vector2(4f,-4f));
            AddShadow(modal.gameObject, new Vector2(0f,-10f), Hex("160A24", .45f));
            CreateText(modal.transform, "Title", "PARENT GATE", 30f, FontStyles.Bold, Hex("5D2BAD"), new Vector2(.08f,.78f), new Vector2(.92f,.94f));
            message = CreateText(modal.transform, "Message", "For grown-ups only.\n\nWhat is 4 + 3?", 20f, FontStyles.Bold, Hex("55356B"), new Vector2(.08f,.45f), new Vector2(.92f,.77f));
            message.textWrappingMode = TextWrappingModes.Normal;

            var b6 = CreateButton(modal.transform, "Six", "6", new Vector2(.10f,.24f), new Vector2(.32f,.42f), Hex("EF4C9D"), 26f);
            var b7 = CreateButton(modal.transform, "Seven", "7", new Vector2(.39f,.24f), new Vector2(.61f,.42f), Hex("25BFC8"), 26f);
            var b8 = CreateButton(modal.transform, "Eight", "8", new Vector2(.68f,.24f), new Vector2(.90f,.42f), Hex("F3A91C"), 26f);
            var close = CreateButton(modal.transform, "Close", "CANCEL", new Vector2(.31f,.07f), new Vector2(.69f,.18f), Hex("7D5A8C"), 16f);

            UnityEventTools.AddPersistentListener(b6.onClick, controller.AnswerParentGate6);
            UnityEventTools.AddPersistentListener(b7.onClick, controller.AnswerParentGate7);
            UnityEventTools.AddPersistentListener(b8.onClick, controller.AnswerParentGate8);
            UnityEventTools.AddPersistentListener(close.onClick, controller.CloseParentGate);

            gatePanel.SetActive(false);
        }

        static TMP_InputField CreateInputField(Transform parent, string name, string placeholderValue, Vector2 min, Vector2 max)
        {
            var image = CreateImage(parent, name, min, max, Hex("F7F0FF"));
            image.raycastTarget = true;
            AddOutline(image.gameObject, Hex("C8A9E9"), new Vector2(2f,-2f));
            var input = image.gameObject.AddComponent<TMP_InputField>();
            var viewportGo = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(image.transform, false);
            var viewport = (RectTransform)viewportGo.transform;
            viewport.anchorMin = new Vector2(.06f,.10f);
            viewport.anchorMax = new Vector2(.94f,.90f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            var placeholder = CreateText(viewport, "Placeholder", placeholderValue, 18f, FontStyles.Italic, Hex("8B7692", .7f), Vector2.zero, Vector2.one);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            var text = CreateText(viewport, "Text", "", 19f, FontStyles.Bold, Hex("4A3153"), Vector2.zero, Vector2.one);
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

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color, float fontSize)
        {
            var image = CreatePanel(parent, name, min, max, color);
            image.raycastTarget = true;
            AddShadow(image.gameObject, new Vector2(0f,-5f), Hex("35135F", .28f));
            var button = image.gameObject.AddComponent<Button>();
            var t = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f,.06f), new Vector2(.96f,.94f));
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(11f, fontSize * .55f);
            t.fontSizeMax = fontSize;
            return button;
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>() ?? target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void AddShadow(GameObject target, Vector2 distance, Color color)
        {
            var shadow = target.GetComponent<Shadow>() ?? target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        static Texture2D FindJourneyTexture(out Rect uv)
        {
            uv = new Rect(0f,0f,1f,1f);
            var clean = AssetDatabase.LoadAssetAtPath<Texture2D>(CleanJourneyPath);
            if (clean != null) return clean;
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(JourneyAtlasPath);
            if (atlas != null)
            {
                uv = new Rect(0f, 2f/3f, 1f/5f, 1f/3f);
                return atlas;
            }
            string[] candidates = AssetDatabase.FindAssets("Journey t:Texture2D");
            foreach (string guid in candidates)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;
                if (tex.width >= 600 && tex.height >= 600) uv = new Rect(0f, 2f/3f, 1f/5f, 1f/3f);
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
            return new Color(1f,1f,1f,alpha);
        }
    }
}
#endif
