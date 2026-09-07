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
    public static class LWJChildNameSetupBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/NameSetup.unity";
        const string CleanJourneyPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";
        const string JourneyAtlasPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuAtlas.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Build Child Name Setup V1")]
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
                "Child Name Setup V1 is ready. On first launch, the child enters a first name before reaching the Main Menu. The name is stored locally in the existing progress save and can be changed later in Parent Zone.",
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

            var controllerGo = new GameObject("ChildNameSetupController");
            var controller = controllerGo.AddComponent<ChildNameSetupControllerV1>();

            BuildTitle(canvasGo.transform);
            BuildJourney(canvasGo.transform, journeyTexture, journeyUv, out TMP_Text speech);
            BuildNameCard(canvasGo.transform, controller, out TMP_InputField nameInput, out TMP_Text status);

            var so = new SerializedObject(controller);
            so.FindProperty("nameInput").objectReferenceValue = nameInput;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("journeySpeechText").objectReferenceValue = speech;
            so.FindProperty("skipIfProfileAlreadyExists").boolValue = true;
            so.FindProperty("nextScene").stringValue = "MainMenu";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateImage(parent, "WelcomeWall", Vector2.zero, Vector2.one, Hex("F7DCEC"));
            CreateImage(parent, "TopGlow", new Vector2(0f, .48f), Vector2.one, Hex("FFF7DE", .50f));
            CreateImage(parent, "Floor", new Vector2(0f, 0f), new Vector2(1f, .33f), Hex("C98A67"));
            CreateImage(parent, "Rug", new Vector2(.06f, .06f), new Vector2(.94f, .34f), Hex("8B58D0", .90f));

            var arch = CreatePanel(parent, "WelcomeArch", new Vector2(.045f, .14f), new Vector2(.955f, .80f), Hex("FFF9F3", .55f));
            AddOutline(arch.gameObject, Hex("D7A7C8", .82f), new Vector2(3f, -3f));
            arch.raycastTarget = false;

            CreateImage(parent, "GoldRail", new Vector2(.05f, .795f), new Vector2(.95f, .808f), Hex("D9A23D"));
            CreateImage(parent, "GoldGlow", new Vector2(.07f, .808f), new Vector2(.93f, .815f), Hex("FFF0A0", .9f));

            // Friendly preschool accents.
            CreatePanel(parent, "StarAccent", new Vector2(.08f, .72f), new Vector2(.14f, .76f), Hex("F5BD38", .85f));
            CreatePanel(parent, "HeartAccent", new Vector2(.85f, .69f), new Vector2(.92f, .74f), Hex("F04AA4", .80f));
            CreatePanel(parent, "BlueAccent", new Vector2(.78f, .60f), new Vector2(.84f, .64f), Hex("35B8D7", .78f));
        }

        static void BuildTitle(Transform parent)
        {
            var brand = CreateText(parent, "Brand", "LEARNING WITH JOURNEY", 56f, FontStyles.Bold, Color.white, new Vector2(.08f, .86f), new Vector2(.92f, .94f));
            brand.alignment = TextAlignmentOptions.Center;
            brand.enableAutoSizing = true;
            brand.fontSizeMin = 38f;
            brand.fontSizeMax = 58f;
            brand.outlineColor = Hex("54228E");
            brand.outlineWidth = .24f;

            var ribbon = CreatePanel(parent, "BrandRibbon", new Vector2(.25f, .815f), new Vector2(.75f, .855f), Hex("31BFC5"));
            AddOutline(ribbon.gameObject, Hex("167F8C"), new Vector2(3f, -3f));
            var subtitle = CreateText(ribbon.transform, "Subtitle", "LEARN  •  GROW  •  SHINE", 21f, FontStyles.Bold, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f));
            subtitle.alignment = TextAlignmentOptions.Center;
        }

        static void BuildJourney(Transform parent, Texture2D texture, Rect uv, out TMP_Text speechText)
        {
            var journeyGo = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            journeyGo.transform.SetParent(parent, false);
            var rect = (RectTransform)journeyGo.transform;
            rect.anchorMin = new Vector2(.06f, .29f);
            rect.anchorMax = new Vector2(.48f, .68f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var raw = journeyGo.GetComponent<RawImage>();
            raw.texture = texture;
            raw.uvRect = uv;
            raw.raycastTarget = false;
            raw.color = texture != null ? Color.white : new Color(1f, 1f, 1f, 0f);

            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.285f, .335f), new Vector2(.385f, .425f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .5f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            var j = CreateText(bag.transform, "J", "J", 26f, FontStyles.Bold, Color.white, new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;

            var bubble = CreatePanel(parent, "JourneyWelcomeBubble", new Vector2(.08f, .675f), new Vector2(.52f, .785f), Color.white);
            AddShadow(bubble.gameObject, new Vector2(0f, -6f), Hex("67357C", .22f));
            AddOutline(bubble.gameObject, Hex("8D4CC3"), new Vector2(3f, -3f));

            var tail = CreatePanel(bubble.transform, "SpeechTail", new Vector2(.18f, -.13f), new Vector2(.30f, .08f), Color.white);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.raycastTarget = false;
            tail.transform.SetAsFirstSibling();

            speechText = CreateText(bubble.transform, "Speech", "Hi! I'm Journey! What's your name?", 24f, FontStyles.Bold, Hex("593078"), new Vector2(.07f, .09f), new Vector2(.93f, .91f));
            speechText.enableAutoSizing = true;
            speechText.fontSizeMin = 17f;
            speechText.fontSizeMax = 25f;
            speechText.textWrappingMode = TextWrappingModes.Normal;
            speechText.alignment = TextAlignmentOptions.Center;
        }

        static void BuildNameCard(Transform parent, ChildNameSetupControllerV1 controller, out TMP_InputField input, out TMP_Text status)
        {
            var panel = CreatePanel(parent, "NameCard", new Vector2(.46f, .31f), new Vector2(.94f, .68f), Hex("FFFFFF", .98f));
            AddShadow(panel.gameObject, new Vector2(8f, -10f), Hex("57346E", .25f));
            AddOutline(panel.gameObject, Hex("B985D9"), new Vector2(3f, -3f));

            var title = CreateText(panel.transform, "Title", "WHAT'S YOUR NAME?", 34f, FontStyles.Bold, Hex("6031A3"), new Vector2(.07f, .78f), new Vector2(.93f, .94f));
            title.enableAutoSizing = true;
            title.fontSizeMin = 25f;
            title.fontSizeMax = 36f;
            title.alignment = TextAlignmentOptions.Center;

            var note = CreateText(panel.transform, "Note", "Type your first name so Journey can cheer for you!", 20f, FontStyles.Normal, Hex("5A3B72"), new Vector2(.09f, .63f), new Vector2(.91f, .78f));
            note.enableAutoSizing = true;
            note.fontSizeMin = 15f;
            note.fontSizeMax = 21f;
            note.textWrappingMode = TextWrappingModes.Normal;
            note.alignment = TextAlignmentOptions.Center;

            input = CreateInputField(panel.transform, new Vector2(.10f, .43f), new Vector2(.90f, .61f));

            var start = CreateButton(panel.transform, "StartLearning", "LET'S LEARN!", new Vector2(.16f, .20f), new Vector2(.84f, .39f), Hex("E83F9C"), Hex("9B256E"), 27f);
            UnityEventTools.AddPersistentListener(start.onClick, controller.SaveNameAndStart);

            status = CreateText(panel.transform, "Status", "Type your first name, then tap LET'S LEARN!", 16f, FontStyles.Bold, Hex("6B4A7E"), new Vector2(.08f, .06f), new Vector2(.92f, .18f));
            status.enableAutoSizing = true;
            status.fontSizeMin = 12f;
            status.fontSizeMax = 17f;
            status.textWrappingMode = TextWrappingModes.Normal;
            status.alignment = TextAlignmentOptions.Center;

            var privacy = CreateText(parent, "LocalNameNote", "Parents: the child name is saved on this device and can be changed in Parent Zone.", 15f, FontStyles.Normal, Hex("FFF7FF"), new Vector2(.14f, .15f), new Vector2(.86f, .20f));
            privacy.enableAutoSizing = true;
            privacy.fontSizeMin = 11f;
            privacy.fontSizeMax = 16f;
            privacy.textWrappingMode = TextWrappingModes.Normal;
            privacy.alignment = TextAlignmentOptions.Center;
        }

        static TMP_InputField CreateInputField(Transform parent, Vector2 min, Vector2 max)
        {
            var root = CreatePanel(parent, "NameInput", min, max, Hex("FFF7FC"));
            AddOutline(root.gameObject, Hex("D78DC0"), new Vector2(2f, -2f));
            root.raycastTarget = true;

            var input = root.gameObject.AddComponent<TMP_InputField>();
            input.characterLimit = 20;
            input.contentType = TMP_InputField.ContentType.Name;
            input.lineType = TMP_InputField.LineType.SingleLine;

            var viewport = CreateRect(root.transform, "Text Area", new Vector2(.06f, .10f), new Vector2(.94f, .90f));
            var text = CreateText(viewport, "Text", "", 30f, FontStyles.Bold, Hex("593078"), Vector2.zero, Vector2.one);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.enableAutoSizing = true;
            text.fontSizeMin = 21f;
            text.fontSizeMax = 31f;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            var placeholder = CreateText(viewport, "Placeholder", "Type your first name", 26f, FontStyles.Italic, Hex("A48CAF", .72f), Vector2.zero, Vector2.one);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.enableAutoSizing = true;
            placeholder.fontSizeMin = 18f;
            placeholder.fontSizeMax = 27f;
            placeholder.textWrappingMode = TextWrappingModes.NoWrap;

            input.textViewport = (RectTransform)viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return rect;
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
            var shadow = CreateImage(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), shadowColor);
            shadow.raycastTarget = false;

            var image = CreateImage(parent, name, min, max, top);
            image.raycastTarget = true;
            AddOutline(image.gameObject, Hex("FFD8EE"), new Vector2(3f, -3f));
            var button = image.gameObject.AddComponent<Button>();

            var text = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f, .06f), new Vector2(.96f, .94f));
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(15f, fontSize * .60f);
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
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
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
