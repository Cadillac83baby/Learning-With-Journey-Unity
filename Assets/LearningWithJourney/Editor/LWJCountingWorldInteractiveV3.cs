#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LearningWithJourney.Games;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldInteractiveV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/Counting/Rounded.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/Counting/Circle.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Upgrade Counting World Interactive V3")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
                LWJCountingWorldBuilderV2.Build();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Counting World Canvas was not found. Run Rebuild Counting World V2 first.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            BuildSunnyGardenBackground(canvas.transform);
            var controller = InstallInteractiveController();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "CountingWorldController was not found.", "OK");
                return;
            }

            WireInteractiveApples(controller);
            RewireHomeButton(controller);
            UpdateInstructions();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World Interactive V3 is ready. The bookshelf/classroom window are removed, the game now uses a sunny garden background, and every visible apple is touchable. Each first tap counts 1, 2, 3 and so on; tapped apples receive a number badge and the answer buttons unlock only after all apples are counted.",
                "OK");
        }

        static CountingWorldPlayControllerV3 InstallInteractiveController()
        {
            var controllerGo = GameObject.Find("CountingWorldController");
            if (controllerGo == null) return null;

            var oldV2 = controllerGo.GetComponent<CountingWorldPlayControllerV2>();
            if (oldV2 != null) Object.DestroyImmediate(oldV2);

            var oldV1 = controllerGo.GetComponent<CountingWorldPlayController>();
            if (oldV1 != null) Object.DestroyImmediate(oldV1);

            var v3 = controllerGo.GetComponent<CountingWorldPlayControllerV3>();
            if (v3 == null) v3 = controllerGo.AddComponent<CountingWorldPlayControllerV3>();

            var audio = controllerGo.GetComponent<AudioSource>();
            if (audio == null) audio = controllerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0f;

            return v3;
        }

        static void WireInteractiveApples(CountingWorldPlayControllerV3 controller)
        {
            var gridGo = GameObject.Find("ObjectGrid");
            if (gridGo == null) return;

            var apples = gridGo.transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Take(20)
                .ToArray();

            var badges = new TMP_Text[apples.Length];
            for (int i = 0; i < apples.Length; i++)
            {
                var apple = apples[i];
                if (apple == null) continue;

                var image = apple.GetComponent<Image>();
                if (image != null) image.raycastTarget = true;

                var button = apple.GetComponent<Button>();
                if (button == null) button = apple.AddComponent<Button>();
                button.targetGraphic = image;
                button.navigation = new Navigation { mode = Navigation.Mode.None };

                var oldBadge = apple.transform.Find("V3CountBadge");
                if (oldBadge != null) Object.DestroyImmediate(oldBadge.gameObject);

                var badge = CreateImage(apple.transform, "V3CountBadge", circle,
                    new Vector2(.58f, .03f), new Vector2(.98f, .43f), Hex("6A22A8"));
                AddShadow(badge.gameObject, new Vector2(0f, -3f), Hex("351052", .42f));
                AddOutline(badge.gameObject, Color.white, new Vector2(2f, -2f));

                var number = CreateText(badge.transform, "Number", "", 27f, FontStyles.Bold, Color.white,
                    new Vector2(.05f, .04f), new Vector2(.95f, .96f));
                number.alignment = TextAlignmentOptions.Center;
                number.outlineColor = Hex("3B0B62");
                number.outlineWidth = .07f;
                badge.gameObject.SetActive(false);
                badges[i] = number;
            }

            var answers = new[]
            {
                GameObject.Find("AnswerA")?.GetComponent<Button>(),
                GameObject.Find("AnswerB")?.GetComponent<Button>(),
                GameObject.Find("AnswerC")?.GetComponent<Button>()
            }.Where(b => b != null).ToArray();

            var speech = GameObject.Find("JourneyCountingBubble")?.transform.Find("SpeechText")?.GetComponent<TMP_Text>();
            var prompt = GameObject.Find("PromptText")?.GetComponent<TMP_Text>();
            var feedback = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
            var round = GameObject.Find("RoundText")?.GetComponent<TMP_Text>();
            var points = GameObject.Find("PointsPill")?.transform.Find("Count")?.GetComponent<TMP_Text>();
            var level = GameObject.Find("LevelPill")?.transform.Find("Level")?.GetComponent<TMP_Text>();
            var journey = GameObject.Find("Journey")?.GetComponent<RectTransform>();
            var audio = controller.GetComponent<AudioSource>();

            var so = new SerializedObject(controller);
            SetObjectArray(so.FindProperty("countObjects"), apples.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("countBadges"), badges.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("answerButtons"), answers.Cast<Object>().ToArray());
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("speechText").objectReferenceValue = speech;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.FindProperty("roundText").objectReferenceValue = round;
            so.FindProperty("pointsText").objectReferenceValue = points;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("journeyRect").objectReferenceValue = journey;
            so.FindProperty("numberAudioSource").objectReferenceValue = audio;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void RewireHomeButton(CountingWorldPlayControllerV3 controller)
        {
            var back = GameObject.Find("BackButton")?.GetComponent<Button>();
            if (back == null) return;

            back.onClick.RemoveAllListeners();
            while (back.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(back.onClick, 0);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);
        }

        static void UpdateInstructions()
        {
            var instruction = GameObject.Find("AnswerInstruction")?.GetComponent<TMP_Text>();
            if (instruction != null)
            {
                instruction.text = "TOUCH EACH APPLE TO COUNT";
                instruction.fontSize = 24f;
                instruction.color = Hex("5A2486");
            }

            var prompt = GameObject.Find("PromptText")?.GetComponent<TMP_Text>();
            if (prompt != null)
                prompt.text = "Touch the apples. How many are there?";
        }

        static void BuildSunnyGardenBackground(Transform canvas)
        {
            DestroyIfFound("CountingShelf");
            DestroyIfFound("Window");
            DestroyIfFound("WindowShadow");
            DestroyIfFound("V3GardenRoot");
            for (int i = 0; i < 6; i++) DestroyIfFound("FloorBand" + i);

            var wall = GameObject.Find("Wall")?.GetComponent<Image>();
            if (wall != null) wall.color = Hex("72D3F6");

            var glow = GameObject.Find("WallGlow")?.GetComponent<Image>();
            if (glow != null) glow.color = new Color(1f, 1f, 1f, .16f);

            var floor = GameObject.Find("Floor")?.GetComponent<Image>();
            if (floor != null) floor.color = Hex("61B957");

            var baseboard = GameObject.Find("Baseboard")?.GetComponent<Image>();
            if (baseboard != null) baseboard.color = Hex("D8F49A");

            var root = new GameObject("V3GardenRoot", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = Vector2.one;
            rr.offsetMin = Vector2.zero;
            rr.offsetMax = Vector2.zero;

            var baseboardGo = GameObject.Find("Baseboard");
            if (baseboardGo != null)
                root.transform.SetSiblingIndex(baseboardGo.transform.GetSiblingIndex() + 1);
            else
                root.transform.SetAsFirstSibling();

            // Sunny park / learning garden: intentionally different from the Main Menu classroom.
            var sun = CreateImage(root.transform, "Sun", circle,
                new Vector2(.80f, .73f), new Vector2(.94f, .82f), Hex("FFD64A"));
            AddShadow(sun.gameObject, new Vector2(4f, -5f), Hex("D99513", .20f));

            BuildCloud(root.transform, "CloudLeft", new Vector2(.04f, .76f), new Vector2(.25f, .82f));
            BuildCloud(root.transform, "CloudRight", new Vector2(.61f, .70f), new Vector2(.79f, .755f));

            var hillBack = CreateImage(root.transform, "HillBack", circle,
                new Vector2(-.10f, .27f), new Vector2(.62f, .53f), Hex("8FD46C"));
            hillBack.raycastTarget = false;
            var hillFront = CreateImage(root.transform, "HillFront", circle,
                new Vector2(.35f, .25f), new Vector2(1.10f, .50f), Hex("73C45F"));
            hillFront.raycastTarget = false;

            // White picket fence gives the game its own outdoor identity without adding countable objects.
            var rail1 = CreateImage(root.transform, "FenceRail1", rounded,
                new Vector2(.02f, .395f), new Vector2(.98f, .412f), Hex("FFF8EF", .92f));
            var rail2 = CreateImage(root.transform, "FenceRail2", rounded,
                new Vector2(.02f, .355f), new Vector2(.98f, .372f), Hex("FFF8EF", .92f));
            rail1.raycastTarget = rail2.raycastTarget = false;

            for (int i = 0; i < 10; i++)
            {
                float x = .035f + i * .103f;
                var post = CreateImage(root.transform, "FencePost" + i, rounded,
                    new Vector2(x, .335f), new Vector2(x + .035f, .445f), Hex("FFFDF8", .96f));
                post.raycastTarget = false;
            }

            // Soft bushes at the edges only; no decorative apples, so children count only the apples on the activity board.
            CreateImage(root.transform, "BushLeftA", circle, new Vector2(-.04f, .30f), new Vector2(.15f, .39f), Hex("47A94E"));
            CreateImage(root.transform, "BushLeftB", circle, new Vector2(.08f, .30f), new Vector2(.24f, .39f), Hex("5CBD58"));
            CreateImage(root.transform, "BushRightA", circle, new Vector2(.80f, .30f), new Vector2(.96f, .39f), Hex("5CBD58"));
            CreateImage(root.transform, "BushRightB", circle, new Vector2(.91f, .30f), new Vector2(1.06f, .39f), Hex("47A94E"));
        }

        static void BuildCloud(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = min;
            rr.anchorMax = max;
            rr.offsetMin = Vector2.zero;
            rr.offsetMax = Vector2.zero;

            CreateImage(root.transform, "Puff1", circle, new Vector2(.00f, .08f), new Vector2(.48f, .88f), new Color(1f, 1f, 1f, .88f));
            CreateImage(root.transform, "Puff2", circle, new Vector2(.28f, .20f), new Vector2(.72f, 1.00f), new Color(1f, 1f, 1f, .93f));
            CreateImage(root.transform, "Puff3", circle, new Vector2(.55f, .07f), new Vector2(1.00f, .86f), new Color(1f, 1f, 1f, .88f));
        }

        static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == rounded && rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size,
            FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

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

        static void DestroyIfFound(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
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
