#if UNITY_EDITOR
using System.Collections.Generic;
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
    public static class LWJRewardsBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/RewardsRoom.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";

        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Rewards V1")]
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
                "Rewards V1 is ready. The screen includes Journey, Stars/Points, Journey Coins, Level, Next Reward progress, a treasure case that opens, sparkles, a prize that rises out of the chest, a first-visit welcome treasure, and a new treasure every 5 earned stars.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("5A2B79");
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

            var controllerGo = new GameObject("RewardsController");
            var controller = controllerGo.AddComponent<RewardsScreenControllerV1>();

            BuildHeader(canvasGo.transform, out TMP_Text pointsText, out TMP_Text coinsText, out TMP_Text levelText);
            BuildTitle(canvasGo.transform);
            BuildJourney(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speechText);
            BuildTreasure(canvasGo.transform,
                out RectTransform chestRoot,
                out RectTransform chestLid,
                out RectTransform prizeRoot,
                out CanvasGroup prizeCanvasGroup,
                out TMP_Text prizeTitleText,
                out TMP_Text prizeAmountText,
                out RectTransform[] sparkles);
            BuildProgress(canvasGo.transform, out TMP_Text rewardProgressText, out Image[] rewardMarkers);
            BuildOpenButton(canvasGo.transform, out Button openButton, out TMP_Text openButtonText);
            BuildBottomNav(canvasGo.transform, controller);

            var so = new SerializedObject(controller);
            so.FindProperty("pointsText").objectReferenceValue = pointsText;
            so.FindProperty("coinsText").objectReferenceValue = coinsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("rewardProgressText").objectReferenceValue = rewardProgressText;
            so.FindProperty("prizeTitleText").objectReferenceValue = prizeTitleText;
            so.FindProperty("prizeAmountText").objectReferenceValue = prizeAmountText;
            so.FindProperty("openTreasureButton").objectReferenceValue = openButton;
            so.FindProperty("openTreasureButtonText").objectReferenceValue = openButtonText;
            so.FindProperty("chestRoot").objectReferenceValue = chestRoot;
            so.FindProperty("chestLid").objectReferenceValue = chestLid;
            so.FindProperty("prizeRoot").objectReferenceValue = prizeRoot;
            so.FindProperty("prizeCanvasGroup").objectReferenceValue = prizeCanvasGroup;
            SetObjectArray(so.FindProperty("sparkles"), sparkles.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("rewardMarkers"), rewardMarkers.Cast<Object>().ToArray());
            so.FindProperty("starsPerTreasure").intValue = 5;
            so.FindProperty("coinsPerTreasure").intValue = 25;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "Wall", Vector2.zero, Vector2.one, Hex("F9C7DA"));
            CreateImage(parent, "WallGlow", new Vector2(0f, .44f), new Vector2(1f, 1f), Hex("FFE0EB", .55f));
            CreateImage(parent, "Floor", new Vector2(0f, 0f), new Vector2(1f, .34f), Hex("F3B08F"));
            CreateImage(parent, "Rug", new Vector2(.04f, .10f), new Vector2(.96f, .39f), Hex("B97AE8", .92f));

            var window = CreatePanel(parent, "WindowFrame", new Vector2(.025f, .51f), new Vector2(.25f, .79f), Hex("FFFFFF"));
            AddOutline(window.gameObject, Hex("E79DBB"), new Vector2(4f, -4f));
            CreateImage(window.transform, "Sky", new Vector2(.07f, .08f), new Vector2(.93f, .92f), Hex("69D5F5"));
            CreateImage(window.transform, "Grass", new Vector2(.07f, .08f), new Vector2(.93f, .31f), Hex("75D66B"));
            CreateImage(window.transform, "PaneV", new Vector2(.48f, .08f), new Vector2(.52f, .92f), Hex("FFFFFF", .85f));
            CreateImage(window.transform, "PaneH", new Vector2(.07f, .48f), new Vector2(.93f, .52f), Hex("FFFFFF", .85f));

            var shelf = CreatePanel(parent, "BookShelf", new Vector2(.79f, .42f), new Vector2(.99f, .72f), Hex("D89A63"));
            AddOutline(shelf.gameObject, Hex("A56A42"), new Vector2(3f, -3f));
            CreateImage(shelf.transform, "Shelf1", new Vector2(.04f, .31f), new Vector2(.96f, .36f), Hex("A56A42"));
            CreateImage(shelf.transform, "Shelf2", new Vector2(.04f, .64f), new Vector2(.96f, .69f), Hex("A56A42"));
            for (int i = 0; i < 6; i++)
            {
                float x = .08f + i * .14f;
                Color c = i % 3 == 0 ? Hex("EF4B9A") : (i % 3 == 1 ? Hex("6D5DD3") : Hex("4BC8D6"));
                CreateImage(shelf.transform, "Book" + i, new Vector2(x, .08f), new Vector2(x + .09f, .30f), c);
            }

            var sign = CreatePanel(parent, "SuccessSign", new Vector2(.77f, .72f), new Vector2(.985f, .88f), Hex("FFF4FA", .96f));
            AddOutline(sign.gameObject, Hex("E9A5CC"), new Vector2(3f, -3f));
            var signText = CreateText(sign.transform, "Text", "SMALL STEPS\nBIG SUCCESS!", 22f, FontStyles.Bold, Hex("7A3D9C"), new Vector2(.08f, .12f), new Vector2(.92f, .88f));
            signText.textWrappingMode = TextWrappingModes.Normal;
            signText.alignment = TextAlignmentOptions.Center;

            CreateImage(parent, "PlantPot", new Vector2(.01f, .08f), new Vector2(.12f, .16f), Hex("E9579F"));
            CreateImage(parent, "PlantLeaf1", new Vector2(.02f, .15f), new Vector2(.075f, .24f), Hex("52BA60"));
            CreateImage(parent, "PlantLeaf2", new Vector2(.07f, .15f), new Vector2(.125f, .25f), Hex("71CC64"));
        }

        static void BuildHeader(Transform parent, out TMP_Text pointsText, out TMP_Text coinsText, out TMP_Text levelText)
        {
            var starPill = CreatePanel(parent, "StarPill", new Vector2(.035f, .925f), new Vector2(.35f, .98f), Hex("E83F9C"));
            AddShadow(starPill.gameObject, new Vector2(0f, -6f), Hex("7F1D63", .45f));
            AddOutline(starPill.gameObject, Hex("FFD8EE"), new Vector2(3f, -3f));
            var label = CreateText(starPill.transform, "Label", "STARS", 18f, FontStyles.Bold, Color.white, new Vector2(.05f, .14f), new Vector2(.48f, .86f));
            label.alignment = TextAlignmentOptions.Center;
            pointsText = CreateText(starPill.transform, "Count", "0", 32f, FontStyles.Bold, Color.white, new Vector2(.48f, .10f), new Vector2(.94f, .90f));
            pointsText.alignment = TextAlignmentOptions.Center;

            var coinPill = CreatePanel(parent, "CoinPill", new Vector2(.56f, .925f), new Vector2(.965f, .98f), Hex("6636C5"));
            AddShadow(coinPill.gameObject, new Vector2(0f, -6f), Hex("35186F", .45f));
            AddOutline(coinPill.gameObject, Hex("EEE1FF"), new Vector2(3f, -3f));
            var coinLabel = CreateText(coinPill.transform, "Label", "JOURNEY COINS", 18f, FontStyles.Bold, Color.white, new Vector2(.04f, .16f), new Vector2(.62f, .84f));
            coinLabel.alignment = TextAlignmentOptions.Center;
            coinsText = CreateText(coinPill.transform, "Count", "0", 32f, FontStyles.Bold, Hex("FFE05B"), new Vector2(.62f, .10f), new Vector2(.96f, .90f));
            coinsText.alignment = TextAlignmentOptions.Center;

            var levelPill = CreatePanel(parent, "LevelPill", new Vector2(.39f, .882f), new Vector2(.61f, .922f), Hex("FFFFFF", .92f));
            AddOutline(levelPill.gameObject, Hex("D3B4F2"), new Vector2(2f, -2f));
            levelText = CreateText(levelPill.transform, "Level", "LEVEL 1", 19f, FontStyles.Bold, Hex("6031A3"), new Vector2(.05f, .10f), new Vector2(.95f, .90f));
            levelText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildTitle(Transform parent)
        {
            var title = CreateText(parent, "RewardsTitle", "REWARDS", 76f, FontStyles.Bold, Color.white, new Vector2(.12f, .80f), new Vector2(.88f, .89f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("55208D");
            title.outlineWidth = .28f;

            var ribbon = CreatePanel(parent, "TitleRibbon", new Vector2(.24f, .765f), new Vector2(.76f, .81f), Hex("35BFC3"));
            AddOutline(ribbon.gameObject, Hex("167F8C"), new Vector2(3f, -3f));
            var subtitle = CreateText(ribbon.transform, "Subtitle", "LEARN  |  GROW  |  SHINE", 22f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static void BuildJourney(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .30f);
            rect.anchorMax = new Vector2(.43f, .70f);
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

            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.255f, .335f), new Vector2(.355f, .425f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .52f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;
            bag.transform.SetSiblingIndex(Mathf.Min(journeyGo.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));

            var bubble = CreatePanel(parent, "JourneyRewardsBubble", new Vector2(.08f, .665f), new Vector2(.43f, .755f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -6f), Hex("67357C", .25f));
            AddOutline(bubble.gameObject, Hex("8D4CC3"), new Vector2(3f, -3f));
            speechText = CreateText(bubble.transform, "Speech", "Let's open your reward!", 24f, FontStyles.Bold, Hex("593078"), new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildTreasure(Transform parent,
            out RectTransform chestRoot,
            out RectTransform chestLid,
            out RectTransform prizeRoot,
            out CanvasGroup prizeCanvasGroup,
            out TMP_Text prizeTitleText,
            out TMP_Text prizeAmountText,
            out RectTransform[] sparkles)
        {
            var chest = new GameObject("TreasureCase", typeof(RectTransform));
            chest.transform.SetParent(parent, false);
            chestRoot = (RectTransform)chest.transform;
            chestRoot.anchorMin = new Vector2(.39f, .34f);
            chestRoot.anchorMax = new Vector2(.95f, .62f);
            chestRoot.offsetMin = chestRoot.offsetMax = Vector2.zero;

            var body = CreatePanel(chest.transform, "ChestBody", new Vector2(.04f, .06f), new Vector2(.96f, .63f), Hex("7429B9"));
            AddShadow(body.gameObject, new Vector2(0f, -10f), Hex("4B1F66", .45f));
            AddOutline(body.gameObject, Hex("FFC52D"), new Vector2(6f, -6f));
            CreatePanel(body.transform, "BodyGlow", new Vector2(.06f, .58f), new Vector2(.94f, .88f), Hex("B645D2", .58f));
            CreateImage(body.transform, "GoldTop", new Vector2(.02f, .80f), new Vector2(.98f, .96f), Hex("FFC52D"));
            CreateImage(body.transform, "GoldBottom", new Vector2(.02f, .04f), new Vector2(.98f, .17f), Hex("E9A619"));
            CreateImage(body.transform, "GoldLeft", new Vector2(.02f, .08f), new Vector2(.11f, .90f), Hex("FFD34D"));
            CreateImage(body.transform, "GoldRight", new Vector2(.89f, .08f), new Vector2(.98f, .90f), Hex("FFD34D"));

            var emblem = CreatePanel(body.transform, "ChestEmblem", new Vector2(.38f, .17f), new Vector2(.62f, .58f), Hex("FFC92E"));
            AddOutline(emblem.gameObject, Hex("FFF0A6"), new Vector2(3f, -3f));
            var emblemText = CreateText(emblem.transform, "J", "J", 56f, FontStyles.Bold, Hex("8A3EBF"), new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            emblemText.alignment = TextAlignmentOptions.Center;

            var lid = CreatePanel(chest.transform, "ChestLid", new Vector2(.06f, .60f), new Vector2(.94f, .94f), Hex("7C2BC1"));
            chestLid = lid.rectTransform;
            AddShadow(lid.gameObject, new Vector2(0f, -7f), Hex("4C1B6E", .38f));
            AddOutline(lid.gameObject, Hex("FFD252"), new Vector2(6f, -6f));
            CreatePanel(lid.transform, "LidGlow", new Vector2(.06f, .48f), new Vector2(.94f, .86f), Hex("D85AD9", .38f));
            CreateImage(lid.transform, "GoldBand", new Vector2(.02f, .08f), new Vector2(.98f, .28f), Hex("FFC52D"));

            sparkles = new RectTransform[8];
            Vector2[] mins =
            {
                new(.35f,.58f), new(.54f,.65f), new(.74f,.58f), new(.83f,.48f),
                new(.31f,.48f), new(.48f,.72f), new(.66f,.71f), new(.88f,.62f)
            };
            for (int i = 0; i < sparkles.Length; i++)
            {
                var s = CreateImage(parent, "RewardSparkle" + i, mins[i], mins[i] + new Vector2(.025f, .025f), i % 2 == 0 ? Hex("FFF4A8") : Hex("FFFFFF"));
                s.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                sparkles[i] = s.rectTransform;
                s.gameObject.SetActive(false);
            }

            var prize = new GameObject("PrizeReveal", typeof(RectTransform), typeof(CanvasGroup));
            prize.transform.SetParent(parent, false);
            prizeRoot = (RectTransform)prize.transform;
            prizeRoot.anchorMin = new Vector2(.52f, .46f);
            prizeRoot.anchorMax = new Vector2(.82f, .61f);
            prizeRoot.offsetMin = prizeRoot.offsetMax = Vector2.zero;
            prizeCanvasGroup = prize.GetComponent<CanvasGroup>();

            var gift = CreatePanel(prize.transform, "GiftBox", new Vector2(.25f, .10f), new Vector2(.75f, .63f), Hex("F044A0"));
            AddOutline(gift.gameObject, Hex("FFD957"), new Vector2(4f, -4f));
            CreateImage(gift.transform, "RibbonV", new Vector2(.43f, .02f), new Vector2(.57f, .98f), Hex("FFD23A"));
            CreateImage(gift.transform, "RibbonH", new Vector2(.02f, .48f), new Vector2(.98f, .64f), Hex("FFD23A"));
            var bowL = CreatePanel(gift.transform, "BowL", new Vector2(.18f, .79f), new Vector2(.48f, 1.10f), Hex("FF77C1"));
            bowL.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            var bowR = CreatePanel(gift.transform, "BowR", new Vector2(.52f, .79f), new Vector2(.82f, 1.10f), Hex("FF77C1"));
            bowR.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -18f);

            prizeTitleText = CreateText(prize.transform, "PrizeTitle", "SURPRISE PRIZE", 24f, FontStyles.Bold, Color.white, new Vector2(.02f, .67f), new Vector2(.98f, .88f));
            prizeTitleText.alignment = TextAlignmentOptions.Center;
            prizeTitleText.outlineColor = Hex("56217D");
            prizeTitleText.outlineWidth = .16f;
            prizeAmountText = CreateText(prize.transform, "PrizeAmount", "+25 JOURNEY COINS", 19f, FontStyles.Bold, Hex("FFE05B"), new Vector2(.02f, .87f), new Vector2(.98f, 1.05f));
            prizeAmountText.alignment = TextAlignmentOptions.Center;

            prize.gameObject.SetActive(false);
        }

        static void BuildProgress(Transform parent, out TMP_Text rewardProgressText, out Image[] rewardMarkers)
        {
            var panel = CreatePanel(parent, "NextRewardPanel", new Vector2(.17f, .235f), new Vector2(.83f, .335f), Hex("6B2EB3", .98f));
            AddShadow(panel.gameObject, new Vector2(0f, -8f), Hex("592178", .35f));
            AddOutline(panel.gameObject, Hex("F0D9FF"), new Vector2(4f, -4f));

            var title = CreateText(panel.transform, "Title", "NEXT REWARD", 25f, FontStyles.Bold, Color.white, new Vector2(.05f, .61f), new Vector2(.48f, .93f));
            title.alignment = TextAlignmentOptions.Left;

            rewardMarkers = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                float x = .07f + i * .105f;
                rewardMarkers[i] = CreateImage(panel.transform, "RewardMarker" + (i + 1), new Vector2(x, .20f), new Vector2(x + .075f, .55f), Hex("6C4A94", .58f));
            }

            var counter = CreatePanel(panel.transform, "CounterCard", new Vector2(.67f, .14f), new Vector2(.94f, .88f), Hex("FFF8FE", .96f));
            AddOutline(counter.gameObject, Hex("E5C8F8"), new Vector2(2f, -2f));
            var small = CreateText(counter.transform, "Small", "TREASURE", 16f, FontStyles.Bold, Hex("7E47A3"), new Vector2(.08f, .60f), new Vector2(.92f, .88f));
            small.alignment = TextAlignmentOptions.Center;
            rewardProgressText = CreateText(counter.transform, "Progress", "5 / 5", 34f, FontStyles.Bold, Hex("5C2D9C"), new Vector2(.08f, .12f), new Vector2(.92f, .63f));
            rewardProgressText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildOpenButton(Transform parent, out Button button, out TMP_Text text)
        {
            button = CreateButton(parent, "OpenTreasureButton", "OPEN TREASURE", new Vector2(.18f, .145f), new Vector2(.82f, .222f), Hex("4CCC2E"), Hex("21861D"), 37f);
            AddOutline(button.gameObject, Hex("FFF4A8"), new Vector2(4f, -4f));
            text = button.GetComponentInChildren<TMP_Text>();
        }

        static void BuildBottomNav(Transform parent, RewardsScreenControllerV1 controller)
        {
            var bar = CreatePanel(parent, "BottomNav", new Vector2(.02f, .02f), new Vector2(.98f, .125f), Hex("5D21AA", .98f));
            AddOutline(bar.gameObject, Hex("8D5BDD"), new Vector2(4f, -4f));

            var home = CreateButton(bar.transform, "Home", "HOME", new Vector2(.02f, .08f), new Vector2(.245f, .92f), Hex("EF4C9D"), Hex("A21E6C"), 22f);
            var library = CreateButton(bar.transform, "Library", "LIBRARY", new Vector2(.26f, .08f), new Vector2(.485f, .92f), Hex("27B7E8"), Hex("137EAA"), 22f);
            var rewards = CreateButton(bar.transform, "Rewards", "REWARDS", new Vector2(.50f, .08f), new Vector2(.725f, .92f), Hex("F4A821"), Hex("B56C08"), 22f);
            var parents = CreateButton(bar.transform, "Parents", "PARENTS", new Vector2(.74f, .08f), new Vector2(.965f, .92f), Hex("8E44D4"), Hex("542483"), 22f);

            AddOutline(rewards.gameObject, Hex("FFF4B0"), new Vector2(4f, -4f));
            UnityEventTools.AddPersistentListener(home.onClick, controller.GoHome);
            UnityEventTools.AddPersistentListener(library.onClick, controller.GoLibrary);
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
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(12f, fontSize * .55f);
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

        static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
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
