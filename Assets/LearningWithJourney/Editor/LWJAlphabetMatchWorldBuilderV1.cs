#if UNITY_EDITOR
using System.Collections.Generic;
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
    public static class LWJAlphabetMatchWorldBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string GeneratedPath = "Assets/LearningWithJourney/Generated/AlphabetMatch";
        const string RoundedPath = GeneratedPath + "/Rounded.png";
        const string CirclePath = GeneratedPath + "/Circle.png";
        const string ApprovedFolder = "Assets/LearningWithJourney/Art/ABC/ApprovedPictures";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";

        static readonly string[] PictureFiles =
        {
            "A_Apple.png", "B_Ball.png", "C_Cat.png", "D_Dog.png", "E_Elephant.png", "F_Fish.png",
            "G_Grapes.png", "H_Hat.png", "I_Ice_Cream.png", "J_Juice.png", "K_Kite.png", "L_Lion.png",
            "M_Moon.png", "N_Nest.png", "O_Owl.png", "P_Pig.png", "Q_Queen.png", "R_Rainbow.png",
            "S_Sun.png", "T_Turtle.png", "U_Umbrella.png", "V_Violin.png", "W_Watermelon.png",
            "X_Xylophone.png", "Y_Yo_Yo.png", "Z_Zebra.png"
        };

        static readonly string[] Words =
        {
            "Apple", "Ball", "Cat", "Dog", "Elephant", "Fish", "Grapes", "Hat", "Ice Cream",
            "Juice", "Kite", "Lion", "Moon", "Nest", "Owl", "Pig", "Queen", "Rainbow",
            "Sun", "Turtle", "Umbrella", "Violin", "Watermelon", "Xylophone", "Yo-Yo", "Zebra"
        };

        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Build Alphabet Match World V1")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            EnsureGeneratedSprites();
            AssetDatabase.Refresh();

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            Texture2D journeyTexture = FindJourneyTexture(out Rect journeyUv);
            Sprite[] pictures = LoadApprovedPictures(out int pictureCount);

            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);

            BuildScene(journeyTexture, journeyUv, pictures);
            EnsureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string artMessage = pictureCount == 26
                ? "All 26 approved ABC pictures were connected."
                : pictureCount + " of 26 approved ABC pictures were found; missing pictures will show the word until their image is available.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Alphabet Match World V1 is ready. " + artMessage +
                " It has 10 levels, 5 successful rounds per level, 2-pair beginner rounds, 3-pair intermediate rounds, 4-pair advanced rounds, uppercase/lowercase matching at Level 9, a full letter-to-picture challenge at Level 10, saved progress, Journey speech, Points, and the approved backpack cover position.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv, Sprite[] pictures)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("392768");
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

            BuildPuzzlePark(canvasGo.transform);

            var controllerGo = new GameObject("AlphabetMatchWorldController");
            var controller = controllerGo.AddComponent<AlphabetMatchWorldPlayControllerV1>();
            var audio = controllerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0f;
            var speech = controllerGo.AddComponent<JourneyAlphabetMatchSpeech>();
            WireSpeechAssets(speech, audio);

            BuildHeader(canvasGo.transform, controller, out TMP_Text pointsText, out TMP_Text levelText);
            RectTransform journeyRect = BuildJourneyArea(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speechText);
            BuildMatchBoard(canvasGo.transform,
                out TMP_Text promptText,
                out TMP_Text feedbackText,
                out Button[] cardButtons,
                out GameObject[] cardBacks,
                out GameObject[] cardFronts,
                out TMP_Text[] cardLetters,
                out Image[] cardPictures,
                out TMP_Text[] cardWords,
                out RectTransform[] cardRects);
            BuildProgressStrip(canvasGo.transform, out TMP_Text pairText, out TMP_Text moveText, out TMP_Text roundText);

            var so = new SerializedObject(controller);
            so.FindProperty("promptText").objectReferenceValue = promptText;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            so.FindProperty("pairProgressText").objectReferenceValue = pairText;
            so.FindProperty("moveText").objectReferenceValue = moveText;
            so.FindProperty("roundText").objectReferenceValue = roundText;
            so.FindProperty("pointsText").objectReferenceValue = pointsText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            SetObjectArray(so.FindProperty("cardButtons"), cardButtons.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardBacks"), cardBacks.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardFronts"), cardFronts.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardLetterTexts"), cardLetters.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardPictureImages"), cardPictures.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardWordTexts"), cardWords.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("cardRects"), cardRects.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("pictures"), pictures.Cast<Object>().ToArray());
            so.FindProperty("journeyRect").objectReferenceValue = journeyRect;
            so.FindProperty("journeySpeech").objectReferenceValue = speech;
            so.FindProperty("totalLevels").intValue = 10;
            so.FindProperty("roundsPerLevel").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildPuzzlePark(Transform parent)
        {
            CreateRect(parent, "MatchSky", Vector2.zero, Vector2.one, Hex("6E5CCB"));
            CreateRect(parent, "MatchSkyTop", new Vector2(0f, .58f), Vector2.one, Hex("70D4D1"));
            CreateRect(parent, "MatchGlow", new Vector2(0f, .43f), new Vector2(1f, .76f), new Color(1f, .83f, .95f, .20f));
            CreateRect(parent, "MatchFloor", new Vector2(0f, 0f), new Vector2(1f, .35f), Hex("9275DC"));
            CreateRect(parent, "MatchFloorGlow", new Vector2(0f, .29f), new Vector2(1f, .37f), Hex("C7B1FF", .78f));

            // Dimensional puzzle-park decorations kept away from the activity board.
            CreateImage(parent, "BubbleLeft1", circle, new Vector2(.03f, .77f), new Vector2(.105f, .815f), Hex("FFF0A6", .92f));
            CreateImage(parent, "BubbleLeft2", circle, new Vector2(.10f, .73f), new Vector2(.17f, .775f), Hex("F8A7D7", .82f));
            CreateImage(parent, "BubbleRight1", circle, new Vector2(.87f, .80f), new Vector2(.94f, .843f), Hex("FFFFFF", .72f));
            CreateImage(parent, "BubbleRight2", circle, new Vector2(.92f, .74f), new Vector2(.985f, .78f), Hex("FFE07A", .80f));

            var leftTile = CreatePanel(parent, "DecorMatchTileA", new Vector2(.045f, .49f), new Vector2(.145f, .56f), Hex("EF4E9B"));
            leftTile.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -8f);
            AddShadow(leftTile.gameObject, new Vector2(0f, -5f), Hex("4A276E", .28f));
            var a = CreateText(leftTile.transform, "Letter", "A", 32f, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            a.alignment = TextAlignmentOptions.Center;

            var tile2 = CreatePanel(parent, "DecorMatchTileStar", new Vector2(.17f, .455f), new Vector2(.27f, .525f), Hex("FFB53E"));
            tile2.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 7f);
            AddShadow(tile2.gameObject, new Vector2(0f, -5f), Hex("4A276E", .28f));
            var q = CreateText(tile2.transform, "Mark", "?", 32f, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            q.alignment = TextAlignmentOptions.Center;
        }

        static void BuildHeader(Transform parent, AlphabetMatchWorldPlayControllerV1 controller, out TMP_Text pointsText, out TMP_Text levelText)
        {
            var back = CreateButton(parent, "BackButton", "<", new Vector2(.035f, .855f), new Vector2(.13f, .915f), Hex("6540BF"), Hex("39206F"), 40f);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);

            pointsText = BuildPointsPill(parent);
            levelText = BuildLevelPill(parent);

            var title = CreateText(parent, "MatchTitle", "ALPHABET MATCH", 50f, FontStyles.Bold,
                Color.white, new Vector2(.18f, .875f), new Vector2(.86f, .925f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("3E266F");
            title.outlineWidth = .18f;

            var subtitle = CreateText(parent, "MatchSubtitle", "MATCH LETTERS + PICTURES  |  10 LEVELS", 23f, FontStyles.Bold,
                Hex("43256D"), new Vector2(.20f, .835f), new Vector2(.86f, .87f));
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.outlineColor = new Color(1f, 1f, 1f, .75f);
            subtitle.outlineWidth = .08f;
        }

        static RectTransform BuildJourneyArea(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .305f);
            rect.anchorMax = new Vector2(.42f, .685f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.color = journeyTexture != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            raw.texture = journeyTexture;
            raw.uvRect = journeyUv;

            if (journeyTexture == null)
            {
                var warning = CreatePanel(parent, "JourneyArtWarning", new Vector2(.055f, .40f), new Vector2(.35f, .57f), Hex("6946B8", .92f));
                var wt = CreateText(warning.transform, "Text", "JOURNEY ART\nNEEDS IMPORT", 23f, FontStyles.Bold, Color.white,
                    new Vector2(.07f, .10f), new Vector2(.93f, .90f));
                wt.textWrappingMode = TextWrappingModes.Normal;
                wt.alignment = TextAlignmentOptions.Center;
            }

            // Canonical Main Menu / Counting / ABC backpack coordinates.
            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.255f, .335f), new Vector2(.355f, .425f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .52f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;
            bag.transform.SetSiblingIndex(Mathf.Min(journeyGo.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));

            var bubble = CreatePanel(parent, "JourneyMatchBubble", new Vector2(.045f, .690f), new Vector2(.385f, .770f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -7f), Hex("4B2B79", .26f));
            AddOutline(bubble.gameObject, Hex("8056C5"), new Vector2(3f, -3f));
            speechText = CreateText(bubble.transform, "MatchSpeechText", "Let's find the matching pairs!", 23f, FontStyles.Bold,
                Hex("4F3175"), new Vector2(.055f, .07f), new Vector2(.945f, .93f));
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.alignment = TextAlignmentOptions.Center;
            speechText.margin = new Vector4(12f, 7f, 12f, 7f);
            bubble.raycastTarget = false;
            bubble.transform.SetAsLastSibling();

            return rect;
        }

        static void BuildMatchBoard(
            Transform parent,
            out TMP_Text promptText,
            out TMP_Text feedbackText,
            out Button[] buttons,
            out GameObject[] backs,
            out GameObject[] fronts,
            out TMP_Text[] letters,
            out Image[] pictures,
            out TMP_Text[] words,
            out RectTransform[] rects)
        {
            var board = CreatePanel(parent, "MatchActivityCard", new Vector2(.40f, .265f), new Vector2(.965f, .785f), Hex("52339A", .99f));
            AddShadow(board.gameObject, new Vector2(0f, -13f), Hex("241246", .58f));
            AddOutline(board.gameObject, Hex("DCCBFF"), new Vector2(4f, -4f));

            var gloss = CreatePanel(board.transform, "TopGloss", new Vector2(.025f, .87f), new Vector2(.975f, .985f), new Color(1f, 1f, 1f, .12f));
            gloss.raycastTarget = false;

            promptText = CreateText(board.transform, "MatchPromptText", "Match each letter to its picture!", 27f, FontStyles.Bold,
                Color.white, new Vector2(.045f, .885f), new Vector2(.955f, .975f));
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.textWrappingMode = TextWrappingModes.Normal;
            promptText.outlineColor = Hex("291753");
            promptText.outlineWidth = .08f;

            feedbackText = CreateText(board.transform, "MatchFeedbackText", "Level 1: find 2 matching pairs.", 22f, FontStyles.Bold,
                Hex("4E2D76"), new Vector2(.05f, .815f), new Vector2(.95f, .875f));
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.textWrappingMode = TextWrappingModes.Normal;
            var feedbackBg = CreatePanel(board.transform, "FeedbackPill", new Vector2(.05f, .812f), new Vector2(.95f, .875f), Hex("F7F0FF"));
            feedbackBg.transform.SetSiblingIndex(feedbackText.transform.GetSiblingIndex());
            feedbackText.transform.SetAsLastSibling();

            var grid = new GameObject("CardGrid", typeof(RectTransform));
            grid.transform.SetParent(board.transform, false);
            var gridRect = (RectTransform)grid.transform;
            gridRect.anchorMin = new Vector2(.035f, .035f);
            gridRect.anchorMax = new Vector2(.965f, .80f);
            gridRect.offsetMin = gridRect.offsetMax = Vector2.zero;

            buttons = new Button[8];
            backs = new GameObject[8];
            fronts = new GameObject[8];
            letters = new TMP_Text[8];
            pictures = new Image[8];
            words = new TMP_Text[8];
            rects = new RectTransform[8];

            for (int i = 0; i < 8; i++)
                BuildCard(grid.transform, i, out buttons[i], out backs[i], out fronts[i], out letters[i], out pictures[i], out words[i], out rects[i]);
        }

        static void BuildCard(Transform parent, int index, out Button button, out GameObject back, out GameObject front,
            out TMP_Text letterText, out Image pictureImage, out TMP_Text wordText, out RectTransform rect)
        {
            var shadow = CreatePanel(parent, "MatchCardShadow" + (index + 1), new Vector2(.05f, .05f), new Vector2(.45f, .35f), Hex("301854", .42f));
            shadow.rectTransform.offsetMin = new Vector2(0f, -6f);
            shadow.rectTransform.offsetMax = new Vector2(0f, -6f);
            shadow.raycastTarget = false;

            var go = new GameObject("MatchCard" + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(.05f, .05f);
            rect.anchorMax = new Vector2(.45f, .35f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var rootImage = go.GetComponent<Image>();
            rootImage.sprite = rounded;
            rootImage.type = Image.Type.Sliced;
            rootImage.color = new Color(1f, 1f, 1f, .01f);
            rootImage.raycastTarget = true;
            button = go.GetComponent<Button>();
            button.targetGraphic = rootImage;
            button.transition = Selectable.Transition.ColorTint;

            var backPanel = CreatePanel(go.transform, "Back", Vector2.zero, Vector2.one, Hex("7047C2"));
            AddOutline(backPanel.gameObject, Hex("E6D9FF"), new Vector2(3f, -3f));
            var backGloss = CreatePanel(backPanel.transform, "Gloss", new Vector2(.05f, .66f), new Vector2(.95f, .94f), new Color(1f, 1f, 1f, .13f));
            backGloss.raycastTarget = false;
            var mark = CreateText(backPanel.transform, "Question", "?", 68f, FontStyles.Bold, Color.white, new Vector2(.12f, .22f), new Vector2(.88f, .82f));
            mark.alignment = TextAlignmentOptions.Center;
            mark.outlineColor = Hex("3B226D");
            mark.outlineWidth = .12f;
            var match = CreateText(backPanel.transform, "MatchLabel", "MATCH", 17f, FontStyles.Bold, Hex("FFF1A6"), new Vector2(.20f, .08f), new Vector2(.80f, .25f));
            match.alignment = TextAlignmentOptions.Center;
            back = backPanel.gameObject;

            var frontPanel = CreatePanel(go.transform, "Front", Vector2.zero, Vector2.one, Hex("FFF9FE"));
            AddOutline(frontPanel.gameObject, Hex("CAB4F2"), new Vector2(3f, -3f));
            front = frontPanel.gameObject;

            letterText = CreateText(frontPanel.transform, "Letter", "A", 76f, FontStyles.Bold, Hex("6732B0"), new Vector2(.10f, .10f), new Vector2(.90f, .90f));
            letterText.alignment = TextAlignmentOptions.Center;
            letterText.enableAutoSizing = true;
            letterText.fontSizeMin = 38f;
            letterText.fontSizeMax = 80f;
            letterText.outlineColor = Color.white;
            letterText.outlineWidth = .08f;

            var pictureGo = new GameObject("Picture", typeof(RectTransform), typeof(Image));
            pictureGo.transform.SetParent(frontPanel.transform, false);
            var pictureRect = (RectTransform)pictureGo.transform;
            pictureRect.anchorMin = new Vector2(.10f, .22f);
            pictureRect.anchorMax = new Vector2(.90f, .91f);
            pictureRect.offsetMin = pictureRect.offsetMax = Vector2.zero;
            pictureImage = pictureGo.GetComponent<Image>();
            pictureImage.preserveAspect = true;
            pictureImage.raycastTarget = false;

            wordText = CreateText(frontPanel.transform, "Word", "Apple", 18f, FontStyles.Bold, Hex("4E2E71"), new Vector2(.05f, .035f), new Vector2(.95f, .23f));
            wordText.alignment = TextAlignmentOptions.Center;
            wordText.enableAutoSizing = true;
            wordText.fontSizeMin = 13f;
            wordText.fontSizeMax = 22f;
            wordText.textWrappingMode = TextWrappingModes.Normal;

            frontPanel.gameObject.SetActive(false);
        }

        static void BuildProgressStrip(Transform parent, out TMP_Text pairText, out TMP_Text moveText, out TMP_Text roundText)
        {
            var strip = CreatePanel(parent, "MatchProgressStrip", new Vector2(.12f, .175f), new Vector2(.89f, .245f), Hex("F6EEFF", .96f));
            AddShadow(strip.gameObject, new Vector2(0f, -6f), Hex("48256F", .28f));
            AddOutline(strip.gameObject, Hex("D9C8F6"), new Vector2(3f, -3f));

            pairText = CreateText(strip.transform, "PairProgressText", "PAIRS 0 / 2", 22f, FontStyles.Bold, Hex("573080"), new Vector2(.03f, .10f), new Vector2(.34f, .90f));
            pairText.alignment = TextAlignmentOptions.Center;
            moveText = CreateText(strip.transform, "MoveText", "MOVES 0", 22f, FontStyles.Bold, Hex("573080"), new Vector2(.34f, .10f), new Vector2(.66f, .90f));
            moveText.alignment = TextAlignmentOptions.Center;
            roundText = CreateText(strip.transform, "MatchRoundText", "ROUND 1 / 5", 22f, FontStyles.Bold, Hex("573080"), new Vector2(.66f, .10f), new Vector2(.97f, .90f));
            roundText.alignment = TextAlignmentOptions.Center;
        }

        static TMP_Text BuildPointsPill(Transform parent)
        {
            var pill = CreatePanel(parent, "PointsPill", new Vector2(.035f, .935f), new Vector2(.30f, .982f), Hex("EF3F94"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("73134E", .55f));
            AddOutline(pill.gameObject, Hex("FFD6EC"), new Vector2(3f, -3f));
            var label = CreateText(pill.transform, "Label", "POINTS", 15f, FontStyles.Bold, Color.white, new Vector2(.08f, .48f), new Vector2(.62f, .88f));
            label.alignment = TextAlignmentOptions.Left;
            var count = CreateText(pill.transform, "Count", "0", 27f, FontStyles.Bold, Color.white, new Vector2(.62f, .12f), new Vector2(.94f, .88f));
            count.alignment = TextAlignmentOptions.Center;
            return count;
        }

        static TMP_Text BuildLevelPill(Transform parent)
        {
            var pill = CreatePanel(parent, "LevelPill", new Vector2(.70f, .935f), new Vector2(.965f, .982f), Hex("6844BC"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("2D175C", .58f));
            AddOutline(pill.gameObject, Hex("E7D9FF"), new Vector2(3f, -3f));
            var level = CreateText(pill.transform, "Level", "LEVEL 1 / 10", 19f, FontStyles.Bold, Color.white, new Vector2(.05f, .10f), new Vector2(.95f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            return level;
        }

        static Sprite[] LoadApprovedPictures(out int found)
        {
            var result = new Sprite[26];
            found = 0;
            if (!Directory.Exists(ApprovedFolder)) return result;

            AssetDatabase.Refresh();
            for (int i = 0; i < PictureFiles.Length; i++)
            {
                string path = ApprovedFolder + "/" + PictureFiles[i];
                if (!File.Exists(path)) continue;
                ConfigurePictureImporter(path);
                result[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (result[i] != null) found++;
            }
            return result;
        }

        static void ConfigurePictureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }
            if (importer.maxTextureSize < 256) { importer.maxTextureSize = 256; changed = true; }
            if (changed) importer.SaveAndReimport();
        }

        static Texture2D FindJourneyTexture(out Rect uv)
        {
            uv = new Rect(0f, 0f, 1f, 1f);

            var clean = AssetDatabase.LoadAssetAtPath<Texture2D>(CleanJourneyPath);
            if (clean != null) return clean;

            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(JourneyAtlasPath);
            if (atlas != null)
            {
                // First frame of the 5 x 3 Journey menu atlas (top-left frame).
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

        static void WireSpeechAssets(JourneyAlphabetMatchSpeech speech, AudioSource audio)
        {
            var so = new SerializedObject(speech);
            so.FindProperty("audioSource").objectReferenceValue = audio;
            var letters = new Object[26];
            var words = new Object[26];
            var phrases = new Object[26];

            for (int i = 0; i < 26; i++)
            {
                string letter = Alphabet[i].ToString();
                string word = Words[i];
                letters[i] = FindAudioClip(new[] { "Journey Letter " + letter, "Letter " + letter, "Journey_" + letter });
                words[i] = FindAudioClip(new[] { "Journey " + word, "Journey_" + SafeName(word), word });
                phrases[i] = FindAudioClip(new[]
                {
                    "Journey " + letter + " is for " + word,
                    "Journey_" + letter + "_is_for_" + SafeName(word),
                    letter + " is for " + word
                });
            }

            SetObjectArray(so.FindProperty("letterClips"), letters);
            SetObjectArray(so.FindProperty("wordClips"), words);
            SetObjectArray(so.FindProperty("phraseClips"), phrases);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static AudioClip FindAudioClip(IEnumerable<string> searches)
        {
            foreach (string search in searches)
            {
                string[] guids = AssetDatabase.FindAssets(search + " t:AudioClip");
                foreach (string guid in guids)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
                    if (clip != null) return clip;
                }
            }
            return null;
        }

        static string SafeName(string value) => value.Replace(" ", "_").Replace("-", "_");

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        static Image CreateRect(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

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
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = false;
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
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
            return text;
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
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText(go.transform, "Label", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.08f, .08f), new Vector2(.92f, .92f));
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            var shadow = go.GetComponent<Shadow>();
            if (shadow == null) shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = go.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            color.a = alpha;
            return color;
        }

        static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static void EnsureGeneratedSprites()
        {
            Directory.CreateDirectory(GeneratedPath);
            if (!File.Exists(RoundedPath))
                WriteShape(RoundedPath, true);
            if (!File.Exists(CirclePath))
                WriteShape(CirclePath, false);
            AssetDatabase.Refresh();
            ConfigureGeneratedSprite(RoundedPath, new Vector4(20f, 20f, 20f, 20f));
            ConfigureGeneratedSprite(CirclePath, Vector4.zero);
        }

        static void WriteShape(string path, bool roundedRect)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside;
                    if (roundedRect)
                    {
                        const float radius = 14f;
                        float px = Mathf.Clamp(x, radius, size - 1 - radius);
                        float py = Mathf.Clamp(y, radius, size - 1 - radius);
                        float dx = x - px;
                        float dy = y - py;
                        inside = dx * dx + dy * dy <= radius * radius;
                    }
                    else
                    {
                        float dx = x - (size - 1) * .5f;
                        float dy = y - (size - 1) * .5f;
                        float r = (size - 2) * .5f;
                        inside = dx * dx + dy * dy <= r * r;
                    }
                    pixels[y * size + x] = inside ? Color.white : new Color(1f, 1f, 1f, 0f);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void ConfigureGeneratedSprite(string path, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteBorder = border;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            int existing = scenes.FindIndex(s => s.path == ScenePath);
            if (existing >= 0)
            {
                if (!scenes[existing].enabled)
                    scenes[existing] = new EditorBuildSettingsScene(ScenePath, true);
            }
            else
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
