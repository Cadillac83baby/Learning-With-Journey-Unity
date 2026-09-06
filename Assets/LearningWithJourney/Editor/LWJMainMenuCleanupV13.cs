#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuCleanupV13
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Clean Background + Fix Title V13")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);

            RemoveBackgroundClutter();
            RepositionUsefulBackgroundPieces();
            RebuildCleanTitle(canvas.transform);
            CleanTagline();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V13 applied: unnecessary classroom clutter was removed, the useful window/bookshelf were kept and repositioned, and the title was rebuilt with cleaner spacing and alignment.",
                "OK");
        }

        static void RemoveBackgroundClutter()
        {
            // Remove decorative pieces that are making the classroom feel busy instead of polished.
            string[] remove =
            {
                "V11WallBoard", "V11BoardText",
                "V11StarL", "V11HeartL", "V11StarR",
                "V11ToyBench", "V11BenchTop",
                "V11WindowLight",
                "V11CurtainHiL", "V11CurtainHiR",
                "V11FloorBlock0", "V11FloorBlock1", "V11FloorBlock2", "V11FloorBlock3",
                "V11Block0", "V11Block1", "V11Block2", "V11Block3",
                "Spark1", "Spark2", "Spark3", "Spark4", "Heart1", "Heart2",
                "V5Spark1", "V5Spark2", "V5Spark3", "V5Spark4",
                "V5HeartLeft", "V5HeartRight", "V5Crown"
            };

            foreach (var name in remove)
                DestroyIfFound(name);

            // Reduce floor visual noise while preserving some perspective/depth.
            for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 1) DestroyIfFound("V11FloorPlank" + i);
            }
            for (int i = 0; i < 7; i++)
            {
                if (i != 1 && i != 5) DestroyIfFound("V11FloorSeam" + i);
            }
        }

        static void RepositionUsefulBackgroundPieces()
        {
            // Keep the window because it gives the classroom depth, but simplify its footprint.
            SetRect("V11WindowShadow", new Vector2(.025f, .485f), new Vector2(.33f, .705f));
            SetRect("V11WindowFrame", new Vector2(.018f, .495f), new Vector2(.32f, .72f));
            SetRect("V11SillShadow", new Vector2(.008f, .475f), new Vector2(.335f, .50f));
            SetRect("V11Sill", new Vector2(.008f, .483f), new Vector2(.335f, .507f));
            SetRect("V11CurtainL", new Vector2(.003f, .49f), new Vector2(.055f, .725f));
            SetRect("V11CurtainR", new Vector2(.285f, .49f), new Vector2(.337f, .725f));

            // Keep the bookshelf the user wanted, but move it away from the title and make it slimmer.
            SetRect("V11ShelfShadow", new Vector2(.79f, .57f), new Vector2(.985f, .76f));
            SetRect("V11Shelf", new Vector2(.785f, .58f), new Vector2(.98f, .77f));

            // Keep a clean baseboard but soften it slightly.
            var board = GameObject.Find("V11Baseboard");
            if (board != null)
            {
                var img = board.GetComponent<Image>();
                if (img != null) img.color = Hex("FBE9F4", .82f);
            }
        }

        static void RebuildCleanTitle(Transform canvas)
        {
            string[] oldTitleObjects =
            {
                "LogoShadow", "LogoOutline", "LogoTitle",
                "LearningLogoCrisp", "WithLogoCrisp", "JourneyLogoCrisp",
                "LogoV4LearningShadow", "LogoV4Learning", "LogoV4WithRibbon", "LogoV4With",
                "LogoV4JourneyShadow", "LogoV4Journey", "LogoV4Crown",
                "V5LogoLearningShadow", "V5LogoLearning", "V5WithRibbon", "V5With",
                "V5LogoJourneyShadow", "V5LogoJourney", "V5Crown",
                "V13TitleRoot"
            };

            foreach (var name in oldTitleObjects)
                DestroyIfFound(name);

            var root = new GameObject("V13TitleRoot", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = Vector2.one;
            rr.offsetMin = Vector2.zero;
            rr.offsetMax = Vector2.zero;
            root.transform.SetAsLastSibling();

            // Learning: centered and intentionally smaller so it no longer crowds the header.
            var learningShadow = CreateText(root.transform, "V13LearningShadow", "Learning", 86f,
                FontStyles.Bold, Hex("3E0D66"), new Vector2(.20f, .825f), new Vector2(.80f, .885f));
            Offset(learningShadow.rectTransform, new Vector2(0f, -5f));

            var learning = CreateText(root.transform, "V13Learning", "Learning", 86f,
                FontStyles.Bold, Color.white, new Vector2(.20f, .831f), new Vector2(.80f, .891f));
            learning.outlineColor = Hex("54137F");
            learning.outlineWidth = .18f;
            learning.alignment = TextAlignmentOptions.Center;

            // Small ribbon keeps the word 'with' readable without overlapping either line.
            var ribbon = CreatePanel(root.transform, "V13WithRibbon",
                new Vector2(.405f, .795f), new Vector2(.595f, .826f), Hex("6A22A8"));
            AddShadow(ribbon.gameObject, new Vector2(0f, -4f), Hex("351052", .45f));

            var with = CreateText(ribbon.transform, "V13With", "with", 27f,
                FontStyles.Bold, Color.white, new Vector2(.05f, .04f), new Vector2(.95f, .96f));
            with.alignment = TextAlignmentOptions.Center;

            // Journey gets its own clear line with enough vertical separation from Learning.
            var journeyShadow = CreateText(root.transform, "V13JourneyShadow", "Journey", 96f,
                FontStyles.Bold | FontStyles.Italic, Hex("5C1457"), new Vector2(.18f, .735f), new Vector2(.82f, .805f));
            Offset(journeyShadow.rectTransform, new Vector2(0f, -6f));

            var journey = CreateText(root.transform, "V13Journey", "Journey", 96f,
                FontStyles.Bold | FontStyles.Italic, Hex("F23893"), new Vector2(.18f, .742f), new Vector2(.82f, .812f));
            journey.outlineColor = Color.white;
            journey.outlineWidth = .14f;
            journey.alignment = TextAlignmentOptions.Center;
        }

        static void CleanTagline()
        {
            var ribbon = GameObject.Find("TaglineRibbon");
            if (ribbon != null)
            {
                SetRect(ribbon, new Vector2(.335f, .695f), new Vector2(.665f, .727f));
                var img = ribbon.GetComponent<Image>();
                if (img != null) img.color = Hex("10B9B4");
            }

            var tag = GameObject.Find("Tagline")?.GetComponent<TextMeshProUGUI>();
            if (tag == null)
                tag = GameObject.Find("TaglineRibbon")?.transform.Find("Tagline")?.GetComponent<TextMeshProUGUI>();

            if (tag != null)
            {
                tag.text = "LEARN  |  GROW  |  SHINE";
                tag.fontSize = 20f;
                tag.fontStyle = FontStyles.Bold;
                tag.color = Color.white;
                tag.alignment = TextAlignmentOptions.Center;
                tag.outlineWidth = 0f;
            }
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = rounded;
            img.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            img.raycastTarget = false;
            return img;
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
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.extraPadding = true;
            text.raycastTarget = false;
            return text;
        }

        static void SetRect(string name, Vector2 min, Vector2 max)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            SetRect(go, min, max);
        }

        static void SetRect(GameObject go, Vector2 min, Vector2 max)
        {
            if (go == null || go.transform is not RectTransform rect) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void Offset(RectTransform rect, Vector2 delta)
        {
            if (rect != null) rect.anchoredPosition += delta;
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var s = go.AddComponent<Shadow>();
            s.effectDistance = distance;
            s.effectColor = color;
            s.useGraphicAlpha = true;
        }

        static void DestroyIfFound(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
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
