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
    public static class LWJBookReaderBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Book Reader V1")]
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
                "Book Reader V1 is ready. ABC, Numbers & Counting, Colors & Shapes, and Story Time all use one polished reader. Every spoken/read-aloud line routes only through JourneyVoicePlayerV1; there is no system TTS fallback.",
                "OK");
        }

        static void BuildScene(Texture2D journeyTexture, Rect journeyUv)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("4D2870");
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

            var voiceGo = new GameObject("JourneyVoice", typeof(AudioSource), typeof(JourneyVoicePlayerV1));
            var voice = voiceGo.GetComponent<JourneyVoicePlayerV1>();

            var controllerGo = new GameObject("BookReaderController");
            var controller = controllerGo.AddComponent<BookReaderControllerV1>();

            BuildHeader(canvasGo.transform, controller, out TMP_Text bookTitle, out TMP_Text pageNumber);
            BuildJourney(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text journeySpeech);
            BuildOpenBook(canvasGo.transform,
                out TMP_Text pageHeading,
                out TMP_Text pageBody,
                out BookPageArtworkV1 artwork);
            BuildControls(canvasGo.transform, controller,
                out Button previous,
                out Button readAgain,
                out Button next,
                out TMP_Text nextText);

            var so = new SerializedObject(controller);
            so.FindProperty("bookTitleText").objectReferenceValue = bookTitle;
            so.FindProperty("pageHeadingText").objectReferenceValue = pageHeading;
            so.FindProperty("pageBodyText").objectReferenceValue = pageBody;
            so.FindProperty("pageNumberText").objectReferenceValue = pageNumber;
            so.FindProperty("journeySpeechText").objectReferenceValue = journeySpeech;
            so.FindProperty("pageArtwork").objectReferenceValue = artwork;
            so.FindProperty("previousButton").objectReferenceValue = previous;
            so.FindProperty("nextButton").objectReferenceValue = next;
            so.FindProperty("readAgainButton").objectReferenceValue = readAgain;
            so.FindProperty("nextButtonText").objectReferenceValue = nextText;
            so.FindProperty("journeyVoice").objectReferenceValue = voice;
            so.FindProperty("autoReadOnPageTurn").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "ReaderWall", Vector2.zero, Vector2.one, Hex("F8DDEA"));
            CreateImage(parent, "ReaderGlow", new Vector2(0f, .43f), new Vector2(1f, 1f), Hex("FFF8D9", .45f));
            CreateImage(parent, "ReaderFloor", new Vector2(0f, 0f), new Vector2(1f, .31f), Hex("C78C66"));
            CreateImage(parent, "ReaderRug", new Vector2(.025f, .07f), new Vector2(.975f, .35f), Hex("8B58D0", .92f));

            var arch = CreatePanel(parent, "ReadingArch", new Vector2(.02f, .17f), new Vector2(.98f, .88f), Hex("FFF8F1", .43f));
            AddOutline(arch.gameObject, Hex("E1B45A", .75f), new Vector2(3f, -3f));
            arch.raycastTarget = false;

            CreateImage(parent, "TopGoldRail", new Vector2(.03f, .875f), new Vector2(.97f, .889f), Hex("E3B143"));
            CreateImage(parent, "TopGoldShine", new Vector2(.05f, .889f), new Vector2(.95f, .896f), Hex("FFF0A2", .9f));

            // Simple side stacks keep this visibly connected to the Library without crowding the book.
            BuildBookStack(parent, "LeftBookStack", new Vector2(.02f, .18f), new Vector2(.16f, .32f));
            BuildBookStack(parent, "RightBookStack", new Vector2(.87f, .18f), new Vector2(.99f, .32f));
        }

        static void BuildBookStack(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var root = CreateRect(parent, name, min, max);
            Color[] colors = { Hex("F04AA4"), Hex("35B8D7"), Hex("F4A62A"), Hex("7642C7") };
            for (int i = 0; i < 4; i++)
            {
                float y = .08f + i * .20f;
                float inset = (i % 2 == 0) ? .02f : .10f;
                var book = CreatePanel(root, "Book" + i, new Vector2(inset, y), new Vector2(.96f - inset, y + .15f), colors[i]);
                AddOutline(book.gameObject, Hex("FFF4C7"), new Vector2(1.5f, -1.5f));
                book.raycastTarget = false;
            }
        }

        static void BuildHeader(Transform parent, BookReaderControllerV1 controller, out TMP_Text bookTitle, out TMP_Text pageNumber)
        {
            var back = CreateButton(parent, "BackToLibrary", "<  LIBRARY", new Vector2(.035f, .925f), new Vector2(.24f, .978f), Hex("6A35C2"), Hex("3F1D7C"), 22f);
            UnityEventTools.AddPersistentListener(back.onClick, controller.BackToLibrary);

            bookTitle = CreateText(parent, "BookTitle", "ABC BOOK", 48f, FontStyles.Bold, Color.white, new Vector2(.245f, .92f), new Vector2(.755f, .982f));
            bookTitle.alignment = TextAlignmentOptions.Center;
            bookTitle.enableAutoSizing = true;
            bookTitle.fontSizeMin = 30f;
            bookTitle.fontSizeMax = 50f;
            bookTitle.outlineColor = Hex("53218B");
            bookTitle.outlineWidth = .20f;

            var pagePill = CreatePanel(parent, "PagePill", new Vector2(.76f, .93f), new Vector2(.965f, .972f), Hex("FFFFFF", .96f));
            AddOutline(pagePill.gameObject, Hex("B387D9"), new Vector2(2f, -2f));
            pageNumber = CreateText(pagePill.transform, "PageNumber", "PAGE 1 / 6", 18f, FontStyles.Bold, Hex("6031A3"), new Vector2(.05f, .08f), new Vector2(.95f, .92f));
            pageNumber.alignment = TextAlignmentOptions.Center;
        }

        static void BuildJourney(Transform parent, Texture2D journeyTexture, Rect journeyUv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.015f, .19f);
            rect.anchorMax = new Vector2(.37f, .54f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.texture = journeyTexture;
            raw.uvRect = journeyUv;
            raw.color = journeyTexture != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            if (journeyTexture == null)
            {
                var placeholder = CreatePanel(parent, "JourneyPlaceholder", new Vector2(.06f, .28f), new Vector2(.31f, .46f), Hex("E84FA0", .92f));
                var t = CreateText(placeholder.transform, "Text", "JOURNEY", 30f, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
                t.alignment = TextAlignmentOptions.Center;
            }

            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.255f, .255f), new Vector2(.355f, .345f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .5f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Handle", new Vector2(.29f, .83f), new Vector2(.71f, 1.08f), Hex("79248F"));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 27f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;
            bag.transform.SetSiblingIndex(Mathf.Min(journeyGo.transform.GetSiblingIndex() + 1, bag.transform.parent.childCount - 1));

            var bubble = CreatePanel(parent, "JourneyReaderBubble", new Vector2(.035f, .535f), new Vector2(.37f, .655f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -6f), Hex("67357C", .22f));
            AddOutline(bubble.gameObject, Hex("8D4CC3"), new Vector2(3f, -3f));
            var tail = CreatePanel(bubble.transform, "SpeechTail", new Vector2(.18f, -.13f), new Vector2(.31f, .09f), Color.white);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.raycastTarget = false;
            tail.transform.SetAsFirstSibling();

            speechText = CreateText(bubble.transform, "Speech", "Let's read together!", 22f, FontStyles.Bold, Hex("593078"), new Vector2(.07f, .08f), new Vector2(.93f, .92f));
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.enableAutoSizing = true;
            speechText.fontSizeMin = 16f;
            speechText.fontSizeMax = 23f;
            speechText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildOpenBook(Transform parent, out TMP_Text heading, out TMP_Text body, out BookPageArtworkV1 artwork)
        {
            var shadow = CreatePanel(parent, "BookShadow", new Vector2(.355f, .175f), new Vector2(.965f, .865f), Hex("4B2866", .26f));
            shadow.rectTransform.anchoredPosition = new Vector2(12f, -12f);
            shadow.raycastTarget = false;

            var book = CreatePanel(parent, "OpenBook", new Vector2(.33f, .19f), new Vector2(.955f, .87f), Hex("FFFDF7"));
            AddOutline(book.gameObject, Hex("E2B94F"), new Vector2(5f, -5f));
            AddShadow(book.gameObject, new Vector2(10f, -12f), Hex("4B2866", .22f));
            book.raycastTarget = false;

            CreateImage(book.transform, "InnerPage", new Vector2(.035f, .035f), new Vector2(.965f, .965f), Hex("FFF9EC")).raycastTarget = false;
            CreateImage(book.transform, "Spine", new Vector2(.485f, .035f), new Vector2(.515f, .965f), Hex("E9D7B7", .70f)).raycastTarget = false;
            CreateImage(book.transform, "TopHighlight", new Vector2(.06f, .93f), new Vector2(.94f, .955f), Hex("FFFFFF", .85f)).raycastTarget = false;

            var artPanel = CreatePanel(book.transform, "ArtworkPanel", new Vector2(.09f, .46f), new Vector2(.91f, .91f), Hex("F7E9FF", .52f));
            AddOutline(artPanel.gameObject, Hex("C69AE8", .75f), new Vector2(2f, -2f));
            artPanel.raycastTarget = false;

            var artGo = new GameObject("PageArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(BookPageArtworkV1));
            artGo.transform.SetParent(artPanel.transform, false);
            var artRect = artGo.GetComponent<RectTransform>();
            artRect.anchorMin = new Vector2(.08f, .06f);
            artRect.anchorMax = new Vector2(.92f, .94f);
            artRect.offsetMin = artRect.offsetMax = Vector2.zero;
            artwork = artGo.GetComponent<BookPageArtworkV1>();
            artwork.raycastTarget = false;

            heading = CreateText(book.transform, "PageHeading", "A is for Apple", 38f, FontStyles.Bold, Hex("6330A4"), new Vector2(.08f, .365f), new Vector2(.92f, .455f));
            heading.alignment = TextAlignmentOptions.Center;
            heading.enableAutoSizing = true;
            heading.fontSizeMin = 26f;
            heading.fontSizeMax = 40f;

            body = CreateText(book.transform, "PageBody", "A is for apple.", 27f, FontStyles.Normal, Hex("4A3153"), new Vector2(.09f, .115f), new Vector2(.91f, .36f));
            body.textWrappingMode = TextWrappingModes.Normal;
            body.enableAutoSizing = true;
            body.fontSizeMin = 20f;
            body.fontSizeMax = 29f;
            body.alignment = TextAlignmentOptions.Center;

            var tip = CreateText(book.transform, "ReadTip", "Tap READ AGAIN to hear Journey read the page.", 17f, FontStyles.Bold, Hex("8C5A9F"), new Vector2(.09f, .055f), new Vector2(.91f, .11f));
            tip.alignment = TextAlignmentOptions.Center;
        }

        static void BuildControls(Transform parent, BookReaderControllerV1 controller, out Button previous, out Button readAgain, out Button next, out TMP_Text nextText)
        {
            previous = CreateButton(parent, "PreviousPage", "<  PREVIOUS", new Vector2(.34f, .085f), new Vector2(.535f, .155f), Hex("6A42C6"), Hex("3F278A"), 22f);
            readAgain = CreateButton(parent, "ReadAgain", "READ AGAIN", new Vector2(.545f, .085f), new Vector2(.72f, .155f), Hex("EF4B9F"), Hex("A32170"), 22f);
            next = CreateButton(parent, "NextPage", "NEXT PAGE  >", new Vector2(.73f, .085f), new Vector2(.965f, .155f), Hex("2EB8D8"), Hex("147D9B"), 22f);
            nextText = next.GetComponentInChildren<TMP_Text>();

            UnityEventTools.AddPersistentListener(previous.onClick, controller.PreviousPage);
            UnityEventTools.AddPersistentListener(readAgain.onClick, controller.ReadAgain);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextPage);
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
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
            colors.highlightedColor = new Color(1f, 1f, 1f, .95f);
            colors.pressedColor = new Color(.90f, .90f, .90f, 1f);
            colors.disabledColor = new Color(.58f, .58f, .58f, .65f);
            button.colors = colors;
            var text = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f, .06f), new Vector2(.96f, .94f));
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14f;
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
