#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.Character;
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
        const string GeneratedPath = "Assets/LearningWithJourney/Generated/MainMenu";
        const string ArtPath = "Assets/LearningWithJourney/Art/Journey";
        const string JourneyAtlasPath = ArtPath + "/JourneyMenuAtlas.png";
        const string GradientPath = GeneratedPath + "/ClassroomGradient.png";
        const string FloorPath = GeneratedPath + "/FloorGradient.png";
        const string RoundedPath = GeneratedPath + "/RoundedPanel.png";
        const string CirclePath = GeneratedPath + "/Circle.png";

        static Sprite roundedSprite;
        static Sprite circleSprite;
        static Sprite wallGradient;
        static Sprite floorGradient;

        [MenuItem("Learning with Journey/Build Polished Main Menu")]
        public static void BuildPolishedMainMenu()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "MainMenu scene is missing. Run Build Starter Scenes first.", "OK");
                return;
            }

            EnsureGeneratedArt();
            Directory.CreateDirectory(ArtPath);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearScene(scene);

            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();
            var router = systems.AddComponent<SceneRouter>();

            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("2B1749");
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

            BuildClassroomBackground(canvasGO.transform);

            // TOP HUD — styled after the approved visual reference.
            var hudBack = CreatePanel(canvasGO.transform, "TopHUD", new Vector2(.025f, .917f), new Vector2(.975f, .985f), Hex("5A168B", .88f), true);
            AddShadow(hudBack.gameObject, new Vector2(0, -7), Hex("1E0B35", .55f));
            AddOutline(hudBack.gameObject, Hex("FFB9ED", .85f), new Vector2(3, -3));

            var avatar = CreatePanel(canvasGO.transform, "AvatarRing", new Vector2(.028f, .902f), new Vector2(.154f, .992f), Hex("7F21B4"), true);
            AddOutline(avatar.gameObject, Hex("FFD94D"), new Vector2(4, -4));
            AddShadow(avatar.gameObject, new Vector2(0, -7), Hex("2B0E47", .5f));
            var avatarInner = CreatePanel(avatar.transform, "AvatarInner", new Vector2(.08f, .08f), new Vector2(.92f, .92f), Hex("F7A7CE"), true);
            var avatarText = CreateText(avatarInner.transform, "AvatarText", "J", 64, FontStyles.Bold, Color.white, Vector2.zero, Vector2.one);
            avatarText.alignment = TextAlignmentOptions.Center;

            var starPill = CreateStatPill(canvasGO.transform, "StarPill", "★", "0", new Vector2(.165f, .936f), new Vector2(.385f, .982f), Hex("E91E86"), Hex("FFD84D"));
            var coinPill = CreateStatPill(canvasGO.transform, "CoinPill", "$", "0", new Vector2(.57f, .936f), new Vector2(.79f, .982f), Hex("5A168B"), Hex("FFD84D"));
            var levelText = CreatePill(canvasGO.transform, "LevelPill", "Level 1", new Vector2(.405f, .936f), new Vector2(.55f, .982f), Hex("8E35C8"), 22);

            // Brand title — stacked layers create the glossy 3D logo effect.
            CreateLayeredTitle(canvasGO.transform);

            var taglineRibbon = CreatePanel(canvasGO.transform, "TaglineRibbon", new Vector2(.31f, .747f), new Vector2(.78f, .793f), Hex("16B7B0"), true);
            AddShadow(taglineRibbon.gameObject, new Vector2(0, -8), Hex("43125E", .55f));
            AddOutline(taglineRibbon.gameObject, Hex("E9FFF9", .85f), new Vector2(2, -2));
            var tagline = CreateText(taglineRibbon.transform, "Tagline", "LEARN  •  GROW  •  SHINE", 25, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            tagline.alignment = TextAlignmentOptions.Center;

            // JOURNEY CHARACTER STAGE.
            var rug = CreatePanel(canvasGO.transform, "JourneyRug", new Vector2(.02f, .285f), new Vector2(.48f, .55f), Hex("CF54B7", .82f), true);
            AddShadow(rug.gameObject, new Vector2(0, -10), Hex("5B255E", .34f));
            var rugInner = CreatePanel(rug.transform, "RugInner", new Vector2(.05f, .08f), new Vector2(.95f, .92f), Hex("E989CC", .58f), true);
            AddOutline(rugInner.gameObject, Hex("FFDBF3", .55f), new Vector2(2, -2));

            var characterStage = new GameObject("JourneyCharacter", typeof(RectTransform), typeof(RawImage), typeof(AudioSource));
            characterStage.transform.SetParent(canvasGO.transform, false);
            var characterRect = (RectTransform)characterStage.transform;
            characterRect.anchorMin = new Vector2(.035f, .335f);
            characterRect.anchorMax = new Vector2(.47f, .72f);
            characterRect.offsetMin = characterRect.offsetMax = Vector2.zero;
            var characterRaw = characterStage.GetComponent<RawImage>();
            characterRaw.color = Color.white;
            characterRaw.raycastTarget = false;

            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(JourneyAtlasPath);
            if (atlas != null)
            {
                characterRaw.texture = atlas;
            }
            else
            {
                characterRaw.color = new Color(1f, 1f, 1f, 0f);
                var missing = CreatePanel(canvasGO.transform, "JourneyArtMissing", new Vector2(.055f, .42f), new Vector2(.44f, .625f), Hex("6E249C", .84f), true);
                AddOutline(missing.gameObject, Hex("FFD3F0", .85f), new Vector2(2, -2));
                var missingText = CreateText(missing.transform, "MissingText", "JOURNEY\nANIMATION ART\nREADY TO IMPORT", 27, FontStyles.Bold, Color.white, new Vector2(.08f, .12f), new Vector2(.92f, .88f));
                missingText.alignment = TextAlignmentOptions.Center;
            }

            var speechBubble = CreatePanel(canvasGO.transform, "JourneySpeechBubble", new Vector2(.255f, .575f), new Vector2(.59f, .69f), Color.white, true);
            AddShadow(speechBubble.gameObject, new Vector2(0, -7), Hex("5C2476", .38f));
            AddOutline(speechBubble.gameObject, Hex("7E2FA7"), new Vector2(3, -3));
            var speechText = CreateText(speechBubble.transform, "SpeechText", "Hi! I’m Journey!", 25, FontStyles.Bold, Hex("54207F"), new Vector2(.08f, .12f), new Vector2(.92f, .88f));
            speechText.alignment = TextAlignmentOptions.Center;

            var journeyController = characterStage.AddComponent<JourneyMainMenuCharacter>();
            var journeySO = new SerializedObject(journeyController);
            journeySO.FindProperty("characterImage").objectReferenceValue = characterRaw;
            journeySO.FindProperty("atlas").objectReferenceValue = atlas;
            journeySO.FindProperty("speechText").objectReferenceValue = speechText;
            journeySO.FindProperty("speechBubble").objectReferenceValue = speechBubble.gameObject;
            journeySO.FindProperty("voiceSource").objectReferenceValue = characterStage.GetComponent<AudioSource>();
            journeySO.ApplyModifiedPropertiesWithoutUndo();

            // Re-play Journey button.
            var voiceButton = CreateRoundButton(canvasGO.transform, "JourneyVoiceButton", "♪", new Vector2(.50f, .595f), new Vector2(.585f, .655f), Hex("EF3A95"));
            UnityEventTools.AddPersistentListener(voiceButton.onClick, journeyController.PlayGreeting);

            // GAME SELECT PANEL.
            var gamePanel = CreatePanel(canvasGO.transform, "GamePanel", new Vector2(.49f, .285f), new Vector2(.97f, .695f), Hex("6B25A8", .96f), true);
            AddShadow(gamePanel.gameObject, new Vector2(0, -13), Hex("2B0C4C", .55f));
            AddOutline(gamePanel.gameObject, Hex("E5B6FF"), new Vector2(4, -4));

            var choose = CreateText(gamePanel.transform, "Choose", "CHOOSE A GAME", 30, FontStyles.Bold, Color.white, new Vector2(.08f, .87f), new Vector2(.92f, .97f));
            choose.alignment = TextAlignmentOptions.Center;

            CreateGameTile(gamePanel.transform, "Counting", "123", "COUNTING", "Numbers 1–20", Hex("FFB12E"), new Vector2(.07f, .61f), new Vector2(.93f, .84f), router.OpenCounting);
            CreateGameTile(gamePanel.transform, "ABC", "ABC", "ABC ADVENTURE", "Letters & sounds", Hex("F23B91"), new Vector2(.07f, .34f), new Vector2(.93f, .57f), router.OpenABC);
            CreateGameTile(gamePanel.transform, "Match", "A+", "ALPHABET MATCH", "Match letters & pictures", Hex("23BDE2"), new Vector2(.07f, .07f), new Vector2(.93f, .30f), router.OpenAlphabetMatch);

            // Big CTA treatment like the approved reference.
            var startBanner = CreatePanel(canvasGO.transform, "StartBanner", new Vector2(.18f, .205f), new Vector2(.82f, .275f), Hex("28C928"), true);
            AddShadow(startBanner.gameObject, new Vector2(0, -10), Hex("17600F", .7f));
            AddOutline(startBanner.gameObject, Hex("E7FF83"), new Vector2(4, -4));
            var startText = CreateText(startBanner.transform, "StartText", "PICK A GAME & PLAY!", 35, FontStyles.Bold, Color.white, new Vector2(.06f, .08f), new Vector2(.94f, .92f));
            startText.alignment = TextAlignmentOptions.Center;

            // Bottom navigation tiles.
            var navBack = CreatePanel(canvasGO.transform, "BottomNavBack", new Vector2(.015f, .018f), new Vector2(.985f, .178f), Hex("4B0D86", .98f), true);
            AddShadow(navBack.gameObject, new Vector2(0, -8), Hex("170527", .72f));
            AddOutline(navBack.gameObject, Hex("8D38D2"), new Vector2(3, -3));

            CreateNavTile(navBack.transform, "HomeTile", "HOME", "●", Hex("E63A93"), new Vector2(.02f, .08f), new Vector2(.245f, .92f), null, true);
            CreateNavTile(navBack.transform, "LibraryTile", "LIBRARY", "ABC", Hex("1FAFE0"), new Vector2(.26f, .08f), new Vector2(.49f, .92f), router.OpenLibrary, false);
            CreateNavTile(navBack.transform, "RewardsTile", "REWARDS", "★", Hex("F59F24"), new Vector2(.505f, .08f), new Vector2(.735f, .92f), router.OpenRewards, false);
            CreateNavTile(navBack.transform, "ParentsTile", "PARENT ZONE", "⚙", Hex("8D3AD0"), new Vector2(.75f, .08f), new Vector2(.98f, .92f), router.OpenParentZone, false);

            // HUD binding.
            var hud = canvasGO.AddComponent<MainMenuHud>();
            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("starText").objectReferenceValue = starPill.count;
            hudSO.FindProperty("coinText").objectReferenceValue = coinPill.count;
            hudSO.FindProperty("playerText").objectReferenceValue = null;
            hudSO.FindProperty("levelText").objectReferenceValue = levelText;
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = canvasGO;

            string message = atlas != null
                ? "Reference-style Main Menu v2 is ready with animated Journey. Press Play to preview her movement."
                : "Reference-style Main Menu v2 is ready. Add JourneyMenuAtlas.png to Assets/LearningWithJourney/Art/Journey, then run this builder again to activate Journey's animation.";
            EditorUtility.DisplayDialog("Learning with Journey", message, "OK");
        }

        static void BuildClassroomBackground(Transform parent)
        {
            var wall = CreateImage(parent, "ClassroomWall", wallGradient, Color.white, Vector2.zero, Vector2.one);
            wall.type = Image.Type.Simple;

            // Warm wood floor.
            var floor = CreateImage(parent, "Floor", floorGradient, Color.white, new Vector2(0, 0), new Vector2(1, .42f));
            floor.type = Image.Type.Simple;

            // Window with layered sky.
            var window = CreatePanel(parent, "Window", new Vector2(.015f, .48f), new Vector2(.39f, .86f), Hex("F9D7EE"), true);
            AddShadow(window.gameObject, new Vector2(7, -8), Hex("7A355A", .3f));
            var sky = CreatePanel(window.transform, "Sky", new Vector2(.07f, .07f), new Vector2(.93f, .93f), Hex("49B8F2"), true);
            CreateBubble(sky.transform, "Cloud1", new Vector2(.08f, .66f), new Vector2(.48f, .84f), Hex("FFFFFF", .85f));
            CreateBubble(sky.transform, "Cloud2", new Vector2(.55f, .48f), new Vector2(.90f, .65f), Hex("FFFFFF", .72f));
            CreateRect(sky.transform, "WindowBarV", new Vector2(.485f, 0), new Vector2(.515f, 1), Hex("FFFFFF", .85f));
            CreateRect(sky.transform, "WindowBarH", new Vector2(0, .48f), new Vector2(1, .52f), Hex("FFFFFF", .85f));

            // Shelves / books on the right.
            var shelf = CreatePanel(parent, "Bookshelf", new Vector2(.78f, .39f), new Vector2(.985f, .82f), Hex("B86A4E"), true);
            AddShadow(shelf.gameObject, new Vector2(-8, -7), Hex("6E2D32", .32f));
            for (int i = 0; i < 3; i++)
            {
                float y = .18f + i * .27f;
                CreateRect(shelf.transform, "ShelfBoard" + i, new Vector2(.04f, y), new Vector2(.96f, y + .035f), Hex("7A382D"));
                for (int b = 0; b < 4; b++)
                {
                    Color c = b % 4 == 0 ? Hex("F24793") : b % 4 == 1 ? Hex("35B8D8") : b % 4 == 2 ? Hex("F8B32B") : Hex("7FCB4B");
                    float x = .10f + b * .20f;
                    CreatePanel(shelf.transform, "Book" + i + "_" + b, new Vector2(x, y + .05f), new Vector2(x + .13f, y + .20f), c, true);
                }
            }

            // Rainbow wall decoration.
            CreateBubble(parent, "RainbowPurple", new Vector2(.63f, .67f), new Vector2(.93f, .86f), Hex("A64DDA", .35f));
            CreateBubble(parent, "RainbowBlue", new Vector2(.66f, .69f), new Vector2(.91f, .84f), Hex("3BC4E8", .42f));
            CreateBubble(parent, "RainbowYellow", new Vector2(.69f, .71f), new Vector2(.89f, .82f), Hex("FFD153", .48f));
            CreateBubble(parent, "RainbowPink", new Vector2(.72f, .73f), new Vector2(.87f, .80f), Hex("F76DB0", .55f));

            // Decorative sparkles/hearts.
            CreateDecorativeText(parent, "Spark1", "★", 42, Hex("FFD83D"), new Vector2(.18f, .82f));
            CreateDecorativeText(parent, "Spark2", "✦", 28, Color.white, new Vector2(.72f, .84f));
            CreateDecorativeText(parent, "Heart1", "♥", 39, Hex("F13B8C"), new Vector2(.83f, .77f));
            CreateDecorativeText(parent, "Spark3", "★", 32, Hex("FFD83D"), new Vector2(.08f, .60f));
            CreateDecorativeText(parent, "Heart2", "♥", 34, Hex("B251E1"), new Vector2(.16f, .25f));
        }

        static void CreateLayeredTitle(Transform parent)
        {
            var shadow = CreateText(parent, "LogoShadow", "Learning\nwith Journey", 76, FontStyles.Bold, Hex("3D0C68"), new Vector2(.235f, .795f), new Vector2(.86f, .92f));
            shadow.alignment = TextAlignmentOptions.Center;
            shadow.lineSpacing = -18f;

            var outline = CreateText(parent, "LogoOutline", "Learning\nwith Journey", 73, FontStyles.Bold, Hex("FFFFFF"), new Vector2(.225f, .802f), new Vector2(.85f, .927f));
            outline.alignment = TextAlignmentOptions.Center;
            outline.lineSpacing = -18f;
            AddOutline(outline.gameObject, Hex("5A168B"), new Vector2(4, -4));

            var title = CreateText(parent, "LogoTitle", "Learning\nwith Journey", 70, FontStyles.Bold, Color.white, new Vector2(.225f, .806f), new Vector2(.85f, .931f));
            title.alignment = TextAlignmentOptions.Center;
            title.lineSpacing = -18f;
            title.colorGradient = new VertexGradient(Color.white, Color.white, Hex("FFD6EC"), Hex("F55AA9"));
        }

        static void EnsureGeneratedArt()
        {
            Directory.CreateDirectory(GeneratedPath);

            MakeVerticalGradient(GradientPath, Hex("F7A4C8"), Hex("F58DB8"), Hex("C96BB4"));
            MakeVerticalGradient(FloorPath, Hex("D68A52"), Hex("C57645"), Hex("9A5037"));
            MakeRoundedTexture(RoundedPath, 128, 28);
            MakeCircleTexture(CirclePath, 128);

            AssetDatabase.Refresh();
            wallGradient = AssetDatabase.LoadAssetAtPath<Sprite>(GradientPath);
            floorGradient = AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath);
            roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
        }

        static void MakeVerticalGradient(string path, Color top, Color middle, Color bottom)
        {
            const int width = 16;
            const int height = 256;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color c = t < .55f ? Color.Lerp(bottom, middle, t / .55f) : Color.Lerp(middle, top, (t - .55f) / .45f);
                for (int x = 0; x < width; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            ConfigureSprite(path, Vector4.zero);
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
                    tex.SetPixel(x, y, inside ? Color.white : new Color(1, 1, 1, 0));
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
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
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

        static void ClearScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color, bool sliced)
        {
            var image = CreateImage(parent, name, roundedSprite, color, min, max);
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            return image;
        }

        static Image CreateBubble(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            return CreateImage(parent, name, circleSprite, color, min, max);
        }

        static Image CreateRect(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            return CreateImage(parent, name, null, color, min, max);
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
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        static TMP_Text CreatePill(Transform parent, string name, string value, Vector2 min, Vector2 max, Color color, float fontSize)
        {
            var pill = CreatePanel(parent, name, min, max, color, true);
            AddShadow(pill.gameObject, new Vector2(0, -5), Hex("2D0E4C", .45f));
            var text = CreateText(pill.transform, "Text", value, fontSize, FontStyles.Bold, Color.white, new Vector2(.07f, .08f), new Vector2(.93f, .92f));
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        struct StatPill { public TMP_Text count; }

        static StatPill CreateStatPill(Transform parent, string name, string icon, string value, Vector2 min, Vector2 max, Color color, Color iconColor)
        {
            var pill = CreatePanel(parent, name, min, max, color, true);
            AddShadow(pill.gameObject, new Vector2(0, -6), Hex("2B0C48", .55f));
            AddOutline(pill.gameObject, Hex("FFB7EC", .75f), new Vector2(2, -2));
            var iconText = CreateText(pill.transform, "Icon", icon, 34, FontStyles.Bold, iconColor, new Vector2(.03f, .05f), new Vector2(.34f, .95f));
            iconText.alignment = TextAlignmentOptions.Center;
            var count = CreateText(pill.transform, "Count", value, 31, FontStyles.Bold, Color.white, new Vector2(.30f, .05f), new Vector2(.95f, .95f));
            count.alignment = TextAlignmentOptions.Center;
            return new StatPill { count = count };
        }

        static void CreateGameTile(Transform parent, string name, string icon, string title, string subtitle, Color color, Vector2 min, Vector2 max, UnityAction action)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0, -.012f), max + new Vector2(0, -.012f), Hex("2A0A49", .62f), true);
            shadow.raycastTarget = false;

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            AddOutline(go, Hex("FFFFFF", .82f), new Vector2(3, -3));

            var highlight = CreatePanel(go.transform, "Gloss", new Vector2(.035f, .63f), new Vector2(.965f, .94f), new Color(1, 1, 1, .18f), true);
            highlight.raycastTarget = false;
            var iconBack = CreatePanel(go.transform, "IconBack", new Vector2(.035f, .12f), new Vector2(.27f, .88f), Color.white, true);
            AddShadow(iconBack.gameObject, new Vector2(0, -4), Hex("5F244F", .27f));
            var iconText = CreateText(iconBack.transform, "Icon", icon, 42, FontStyles.Bold, color, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            iconText.alignment = TextAlignmentOptions.Center;
            var titleText = CreateText(go.transform, "Title", title, 29, FontStyles.Bold, Color.white, new Vector2(.31f, .47f), new Vector2(.95f, .88f));
            titleText.alignment = TextAlignmentOptions.Left;
            var sub = CreateText(go.transform, "Subtitle", subtitle, 20, FontStyles.Normal, Color.white, new Vector2(.31f, .15f), new Vector2(.95f, .49f));
            sub.alignment = TextAlignmentOptions.Left;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, .94f);
            colors.pressedColor = new Color(.87f, .87f, .87f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = .08f;
            button.colors = colors;
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        static Button CreateRoundButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = circleSprite;
            image.color = color;
            AddShadow(go, new Vector2(0, -6), Hex("52145D", .45f));
            AddOutline(go, Color.white, new Vector2(2, -2));
            var text = CreateText(go.transform, "Label", label, 38, FontStyles.Bold, Color.white, new Vector2(.12f, .12f), new Vector2(.88f, .88f));
            text.alignment = TextAlignmentOptions.Center;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        static void CreateNavTile(Transform parent, string name, string title, string icon, Color color, Vector2 min, Vector2 max, UnityAction action, bool selected)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0, -.035f), max + new Vector2(0, -.035f), Hex("210635", .72f), true);
            shadow.raycastTarget = false;

            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            AddOutline(go, selected ? Hex("FFF28A") : Hex("FFFFFF", .55f), new Vector2(selected ? 4 : 2, selected ? -4 : -2));

            var gloss = CreatePanel(go.transform, "Gloss", new Vector2(.05f, .66f), new Vector2(.95f, .94f), new Color(1, 1, 1, .18f), true);
            gloss.raycastTarget = false;
            var iconText = CreateText(go.transform, "Icon", icon, 34, FontStyles.Bold, Color.white, new Vector2(.08f, .40f), new Vector2(.92f, .88f));
            iconText.alignment = TextAlignmentOptions.Center;
            var titleText = CreateText(go.transform, "Title", title, 17, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .38f));
            titleText.alignment = TextAlignmentOptions.Center;

            if (action != null)
            {
                var button = go.AddComponent<Button>();
                button.targetGraphic = image;
                UnityEventTools.AddPersistentListener(button.onClick, action);
            }
        }

        static void CreateDecorativeText(Transform parent, string name, string value, float size, Color color, Vector2 center)
        {
            var text = CreateText(parent, name, value, size, FontStyles.Bold, color, center - new Vector2(.035f, .025f), center + new Vector2(.035f, .025f));
            text.alignment = TextAlignmentOptions.Center;
            var floating = text.gameObject.AddComponent<UIFloat>();
            var so = new SerializedObject(floating);
            if (so.FindProperty("amplitude") != null) so.FindProperty("amplitude").floatValue = 5f;
            if (so.FindProperty("speed") != null) so.FindProperty("speed").floatValue = 1.2f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
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
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var c);
            c.a = alpha;
            return c;
        }
    }
}
#endif
