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
    public static class LWJCountingWorldBuilderV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/MainMenu/Circle.png";
        const string JourneyCleanPath = "Assets/LearningWithJourney/Art/Journey/JourneyMenuCleanFixed.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Build Counting World V1")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/LearningWithJourney/Scenes");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            Scene scene;
            if (File.Exists(ScenePath))
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            else
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
                Object.DestroyImmediate(root);

            BuildScene(scene);
            EnsureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World V1 is ready. It supports numbers 1-20, animated counting objects, three answer choices, points/levels, Journey feedback, and a working Home button.",
                "OK");
        }

        static void BuildScene(Scene scene)
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<GameProgressService>();

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("2B1749");
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
            var controller = controllerGo.AddComponent<CountingWorldPlayController>();

            var back = CreateButton(canvasGo.transform, "BackButton", "<",
                new Vector2(.03f, .845f), new Vector2(.13f, .905f), Hex("6C22AF"), Hex("3A0E66"), 40f);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);

            var pointsCount = BuildPointsPill(canvasGo.transform);
            var levelText = BuildLevelPill(canvasGo.transform);

            var title = CreateText(canvasGo.transform, "CountingTitle", "COUNT WITH JOURNEY", 66f, FontStyles.Bold,
                Color.white, new Vector2(.17f, .835f), new Vector2(.90f, .905f));
            title.alignment = TextAlignmentOptions.Center;
            title.outlineColor = Hex("53117D");
            title.outlineWidth = .18f;

            var subtitle = CreateText(canvasGo.transform, "CountingSubtitle", "NUMBERS 1-20", 24f, FontStyles.Bold,
                Hex("6B238F"), new Vector2(.28f, .80f), new Vector2(.72f, .835f));
            subtitle.alignment = TextAlignmentOptions.Center;

            var journey = BuildJourney(canvasGo.transform);
            BuildBackpack(canvasGo.transform);

            var speechBubble = CreatePanel(canvasGo.transform, "JourneyCountingBubble",
                new Vector2(.12f, .65f), new Vector2(.49f, .745f), Color.white);
            AddShadow(speechBubble.gameObject, new Vector2(0f, -7f), Hex("652276", .28f));
            AddOutline(speechBubble.gameObject, Hex("8A3AB6"), new Vector2(3f, -3f));
            var speechText = CreateText(speechBubble.transform, "SpeechText", "Let's count together!", 26f,
                FontStyles.Bold, Hex("55207B"), new Vector2(.07f, .10f), new Vector2(.93f, .90f));
            speechText.alignment = TextAlignmentOptions.Center;

            var activity = CreatePanel(canvasGo.transform, "CountingActivityCard",
                new Vector2(.405f, .355f), new Vector2(.97f, .765f), Hex("6720A9", .97f));
            AddShadow(activity.gameObject, new Vector2(0f, -13f), Hex("280543", .60f));
            AddOutline(activity.gameObject, Hex("E4B9FF"), new Vector2(4f, -4f));

            var activityGloss = CreatePanel(activity.transform, "TopGloss",
                new Vector2(.025f, .86f), new Vector2(.975f, .98f), new Color(1f, 1f, 1f, .10f));
            activityGloss.raycastTarget = false;

            var prompt = CreateText(activity.transform, "PromptText", "How many apples do you see?", 29f,
                FontStyles.Bold, Color.white, new Vector2(.06f, .84f), new Vector2(.94f, .965f));
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.outlineColor = Hex("3B0A62");
            prompt.outlineWidth = .08f;

            var objectBoard = CreatePanel(activity.transform, "ObjectBoard",
                new Vector2(.055f, .08f), new Vector2(.945f, .82f), Hex("FFF7FB"));
            AddShadow(objectBoard.gameObject, new Vector2(0f, -7f), Hex("3A0E5F", .26f));
            AddOutline(objectBoard.gameObject, Color.white, new Vector2(3f, -3f));

            var gridGo = new GameObject("ObjectGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(objectBoard.transform, false);
            var gridRect = (RectTransform)gridGo.transform;
            gridRect.anchorMin = new Vector2(.05f, .07f);
            gridRect.anchorMax = new Vector2(.95f, .93f);
            gridRect.offsetMin = gridRect.offsetMax = Vector2.zero;
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(78f, 78f);
            grid.spacing = new Vector2(8f, 10f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            var apples = new GameObject[20];
            for (int i = 0; i < apples.Length; i++)
            {
                apples[i] = BuildApple(gridGo.transform, i);
                apples[i].SetActive(false);
            }

            var instruction = CreateText(canvasGo.transform, "AnswerInstruction", "Tap the correct number", 28f,
                FontStyles.Bold, Hex("5E1B86"), new Vector2(.20f, .302f), new Vector2(.90f, .345f));
            instruction.alignment = TextAlignmentOptions.Center;

            var answerA = CreateButton(canvasGo.transform, "AnswerA", "1",
                new Vector2(.15f, .205f), new Vector2(.385f, .295f), Hex("F23B98"), Hex("AE175F"), 48f);
            var answerB = CreateButton(canvasGo.transform, "AnswerB", "2",
                new Vector2(.40f, .205f), new Vector2(.635f, .295f), Hex("FFAF27"), Hex("C66B04"), 48f);
            var answerC = CreateButton(canvasGo.transform, "AnswerC", "3",
                new Vector2(.65f, .205f), new Vector2(.885f, .295f), Hex("23BDE4"), Hex("087B9B"), 48f);

            var feedback = CreateText(canvasGo.transform, "FeedbackText", "", 27f, FontStyles.Bold,
                Hex("62208B"), new Vector2(.14f, .155f), new Vector2(.86f, .20f));
            feedback.alignment = TextAlignmentOptions.Center;

            var roundText = CreateText(canvasGo.transform, "RoundText", "ROUND 1 / 5", 20f, FontStyles.Bold,
                Hex("7D389C"), new Vector2(.35f, .12f), new Vector2(.65f, .153f));
            roundText.alignment = TextAlignmentOptions.Center;

            var so = new SerializedObject(controller);
            SetObjectArray(so.FindProperty("countObjects"), apples.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("answerButtons"), new Object[] { answerA, answerB, answerC });
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("speechText").objectReferenceValue = speechText;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.FindProperty("roundText").objectReferenceValue = roundText;
            so.FindProperty("pointsText").objectReferenceValue = pointsCount;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("journeyRect").objectReferenceValue = journey;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBackground(Transform parent)
        {
            CreateRect(parent, "Wall", Vector2.zero, Vector2.one, Hex("F39AC3"));
            CreateRect(parent, "Floor", new Vector2(0f, 0f), new Vector2(1f, .39f), Hex("B95A2D"));
            CreateRect(parent, "Baseboard", new Vector2(0f, .385f), new Vector2(1f, .405f), Hex("FFE8F4"));

            var window = CreatePanel(parent, "Window", new Vector2(.025f, .47f), new Vector2(.34f, .72f), Hex("FFF0F8"));
            AddShadow(window.gameObject, new Vector2(7f, -8f), Hex("6C274D", .25f));
            var sky = CreatePanel(window.transform, "Sky", new Vector2(.07f, .07f), new Vector2(.93f, .93f), Hex("56C5F2"));
            CreateRect(sky.transform, "BarV", new Vector2(.485f, .04f), new Vector2(.515f, .96f), Color.white);
            CreateRect(sky.transform, "BarH", new Vector2(.04f, .485f), new Vector2(.96f, .515f), Color.white);
            var cloud1 = CreateImage(sky.transform, "Cloud1", circle, new Vector2(.10f, .64f), new Vector2(.45f, .80f), new Color(1f, 1f, 1f, .90f));
            cloud1.raycastTarget = false;
            var cloud2 = CreateImage(sky.transform, "Cloud2", circle, new Vector2(.55f, .34f), new Vector2(.85f, .48f), new Color(1f, 1f, 1f, .78f));
            cloud2.raycastTarget = false;

            // Simple floor bands give depth without clutter.
            for (int i = 0; i < 4; i++)
            {
                float y = .04f + i * .075f;
                CreateRect(parent, "FloorBand" + i, new Vector2(.02f, y), new Vector2(.98f, y + .012f),
                    i % 2 == 0 ? Hex("7C351F", .20f) : Hex("F1A16B", .13f));
            }
        }

        static RectTransform BuildJourney(Transform parent)
        {
            var go = new GameObject("Journey", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(.035f, .245f);
            rect.anchorMax = new Vector2(.405f, .655f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<RawImage>();
            image.raycastTarget = false;
            image.color = Color.white;
            image.uvRect = new Rect(0f, 0f, 1f, 1f);

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(JourneyCleanPath);
            if (texture != null)
                image.texture = texture;
            else
            {
                image.color = new Color(1f, 1f, 1f, 0f);
                var note = CreateText(parent, "JourneyMissing", "Journey art", 24f, FontStyles.Bold,
                    Hex("6B238F"), new Vector2(.08f, .38f), new Vector2(.36f, .44f));
                note.alignment = TextAlignmentOptions.Center;
            }
            return rect;
        }

        static void BuildBackpack(Transform parent)
        {
            var bag = CreatePanel(parent, "JourneyBackpack", new Vector2(.285f, .315f), new Vector2(.385f, .405f), Hex("D83BA7"));
            AddShadow(bag.gameObject, new Vector2(0f, -6f), Hex("4B125B", .55f));
            AddOutline(bag.gameObject, Hex("FFD55A"), new Vector2(3f, -3f));
            var flap = CreatePanel(bag.transform, "Flap", new Vector2(.08f, .52f), new Vector2(.92f, .87f), Hex("F25DB8"));
            flap.raycastTarget = false;
            var pocket = CreatePanel(bag.transform, "Pocket", new Vector2(.15f, .08f), new Vector2(.85f, .42f), Hex("A92A91"));
            pocket.raycastTarget = false;
            var j = CreateText(bag.transform, "J", "J", 28f, FontStyles.Bold, Color.white,
                new Vector2(.34f, .12f), new Vector2(.66f, .40f));
            j.alignment = TextAlignmentOptions.Center;
        }

        static TMP_Text BuildPointsPill(Transform parent)
        {
            var pill = CreatePanel(parent, "PointsPill", new Vector2(.035f, .93f), new Vector2(.315f, .982f), Hex("F02F8E"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("73134E", .58f));
            AddOutline(pill.gameObject, Hex("FFD6EC"), new Vector2(3f, -3f));
            var label = CreateText(pill.transform, "Label", "POINTS", 17f, FontStyles.Bold, Color.white,
                new Vector2(.08f, .49f), new Vector2(.60f, .88f));
            label.alignment = TextAlignmentOptions.Left;
            var count = CreateText(pill.transform, "Count", "0", 29f, FontStyles.Bold, Color.white,
                new Vector2(.60f, .12f), new Vector2(.94f, .88f));
            count.alignment = TextAlignmentOptions.Center;
            return count;
        }

        static TMP_Text BuildLevelPill(Transform parent)
        {
            var pill = CreatePanel(parent, "LevelPill", new Vector2(.69f, .93f), new Vector2(.965f, .982f), Hex("7025B8"));
            AddShadow(pill.gameObject, new Vector2(0f, -6f), Hex("2D0A53", .60f));
            AddOutline(pill.gameObject, Hex("E7C8FF"), new Vector2(3f, -3f));
            var level = CreateText(pill.transform, "Level", "LEVEL 1", 23f, FontStyles.Bold, Color.white,
                new Vector2(.08f, .10f), new Vector2(.92f, .90f));
            level.alignment = TextAlignmentOptions.Center;
            return level;
        }

        static GameObject BuildApple(Transform parent, int index)
        {
            var go = new GameObject("Apple" + (index + 1), typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var body = go.GetComponent<Image>();
            body.sprite = circle;
            body.color = index % 3 == 0 ? Hex("F0447D") : index % 3 == 1 ? Hex("F15A97") : Hex("E93B69");
            body.raycastTarget = false;

            var leaf = CreateImage(go.transform, "Leaf", rounded, new Vector2(.56f, .68f), new Vector2(.80f, .88f), Hex("65B94A"));
            leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            leaf.raycastTarget = false;
            var shine = CreateImage(go.transform, "Shine", circle, new Vector2(.20f, .60f), new Vector2(.39f, .79f), new Color(1f, 1f, 1f, .35f));
            shine.raycastTarget = false;
            return go;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max,
            Color main, Color depth, float fontSize)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0f, -.009f), max + new Vector2(0f, -.009f), depth);
            shadow.raycastTarget = false;

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = main;
            AddOutline(go, Color.white, new Vector2(3f, -3f));
            AddShadow(go, new Vector2(0f, -5f), new Color(depth.r, depth.g, depth.b, .35f));

            var gloss = CreatePanel(go.transform, "Gloss", new Vector2(.05f, .65f), new Vector2(.95f, .93f), new Color(1f, 1f, 1f, .22f));
            gloss.raycastTarget = false;
            var text = CreateText(go.transform, "Label", label, fontSize, FontStyles.Bold, Color.white,
                new Vector2(.06f, .08f), new Vector2(.94f, .92f));
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

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, FontStyles style,
            Color color, Vector2 min, Vector2 max)
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
