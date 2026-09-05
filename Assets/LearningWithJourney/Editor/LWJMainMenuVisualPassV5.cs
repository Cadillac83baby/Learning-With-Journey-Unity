#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuVisualPassV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Apply Glossy Reference Main Menu V5")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            RemoveOldReferenceObjects();
            PolishBackground();
            PolishHud();
            BuildReferenceLogo(canvas.transform);
            PolishJourney();
            PolishGamePanel();
            PolishStartButton();
            PolishBottomNav();
            AddReferenceDecor(canvas.transform);
            SharpenText(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Glossy Reference Main Menu V5 is ready. Journey stays animated, the round voice/play button covers the damaged shorts area, and the menu now uses the brighter glossy game styling from the approved reference.",
                "OK");
        }

        static void RemoveOldReferenceObjects()
        {
            string[] names =
            {
                "LogoShadow", "LogoOutline", "LogoTitle",
                "LearningLogoCrisp", "WithLogoCrisp", "JourneyLogoCrisp",
                "LogoV4LearningShadow", "LogoV4Learning", "LogoV4WithRibbon",
                "LogoV4With", "LogoV4JourneyShadow", "LogoV4Journey", "LogoV4Crown",
                "V5LogoLearningShadow", "V5LogoLearning", "V5WithRibbon", "V5With",
                "V5LogoJourneyShadow", "V5LogoJourney", "V5Crown", "V5HeartLeft", "V5HeartRight",
                "V5Spark1", "V5Spark2", "V5Spark3", "V5Spark4"
            };

            foreach (var name in names)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static void PolishBackground()
        {
            Tint("ClassroomWall", Hex("F9A2C7"));
            Tint("Floor", Hex("D88756"));
            Tint("Sky", Hex("46BDF3"));
            Tint("Bookshelf", Hex("B96A51"));
            Tint("JourneyRug", Hex("CB58BD"));
            Tint("RugInner", Hex("EC96D6"));

            var window = GameObject.Find("Window");
            if (window != null)
            {
                var img = window.GetComponent<Image>();
                if (img != null) img.color = Hex("FFD7EF");
                EnsureShadow(window, new Vector2(8f, -10f), Hex("662B57", .28f));
            }
        }

        static void PolishHud()
        {
            SetRect("TopHUD", new Vector2(.02f, .922f), new Vector2(.98f, .99f));
            SetColor("TopHUD", Hex("57148B", .97f));
            EnsureShadow(GameObject.Find("TopHUD"), new Vector2(0f, -8f), Hex("210737", .62f));
            EnsureOutline(GameObject.Find("TopHUD"), Hex("E8A5FF", .9f), new Vector2(3f, -3f));

            SetRect("AvatarRing", new Vector2(.025f, .905f), new Vector2(.145f, .993f));
            SetColor("AvatarRing", Hex("9B32DB"));
            EnsureOutline(GameObject.Find("AvatarRing"), Hex("FFD642"), new Vector2(4f, -4f));

            SetRect("StarPill", new Vector2(.15f, .935f), new Vector2(.39f, .982f));
            SetColor("StarPill", Hex("D91A87"));
            SetRect("LevelPill", new Vector2(.405f, .935f), new Vector2(.56f, .982f));
            SetColor("LevelPill", Hex("7021B0"));
            SetRect("CoinPill", new Vector2(.575f, .935f), new Vector2(.82f, .982f));
            SetColor("CoinPill", Hex("5A1497"));

            var avatarText = FindTMP("AvatarRing/AvatarInner/AvatarText");
            if (avatarText != null)
            {
                avatarText.text = "J";
                avatarText.fontSize = 62;
                avatarText.color = Color.white;
                avatarText.outlineColor = Hex("5D167D");
                avatarText.outlineWidth = .14f;
            }
        }

        static void BuildReferenceLogo(Transform parent)
        {
            var learningShadow = CreateText(parent, "V5LogoLearningShadow", "Learning", 104f, FontStyles.Bold,
                Hex("30104B"), new Vector2(.17f, .84f), new Vector2(.93f, .91f));
            Offset(learningShadow.rectTransform, new Vector2(0f, -7f));

            var learning = CreateText(parent, "V5LogoLearning", "Learning", 104f, FontStyles.Bold,
                Color.white, new Vector2(.17f, .845f), new Vector2(.93f, .915f));
            learning.outlineColor = Hex("4F117B");
            learning.outlineWidth = .22f;
            learning.characterSpacing = -1.5f;

            var ribbon = CreatePanel(parent, "V5WithRibbon", new Vector2(.37f, .805f), new Vector2(.69f, .844f), Hex("6720A5"));
            EnsureShadow(ribbon.gameObject, new Vector2(0f, -6f), Hex("2E0A48", .6f));
            EnsureOutline(ribbon.gameObject, Hex("B75AF1"), new Vector2(2f, -2f));

            var with = CreateText(ribbon.transform, "V5With", "with", 36f, FontStyles.Bold,
                Color.white, new Vector2(.08f, .05f), new Vector2(.92f, .95f));
            with.outlineColor = Hex("4E126E");
            with.outlineWidth = .12f;

            var journeyShadow = CreateText(parent, "V5LogoJourneyShadow", "Journey", 126f, FontStyles.Bold | FontStyles.Italic,
                Hex("4C1054"), new Vector2(.13f, .735f), new Vector2(.96f, .82f));
            Offset(journeyShadow.rectTransform, new Vector2(0f, -8f));

            var journey = CreateText(parent, "V5LogoJourney", "Journey", 126f, FontStyles.Bold | FontStyles.Italic,
                Hex("F12D90"), new Vector2(.13f, .742f), new Vector2(.96f, .827f));
            journey.outlineColor = Color.white;
            journey.outlineWidth = .20f;
            journey.characterSpacing = -2f;

            var crown = CreateText(parent, "V5Crown", "♛", 56f, FontStyles.Bold,
                Hex("FFD13A"), new Vector2(.76f, .86f), new Vector2(.88f, .925f));
            crown.outlineColor = Hex("8A3518");
            crown.outlineWidth = .12f;

            var heartL = CreateText(parent, "V5HeartLeft", "♥", 48f, FontStyles.Bold,
                Hex("F23C98"), new Vector2(.23f, .79f), new Vector2(.31f, .84f));
            heartL.outlineColor = Color.white;
            heartL.outlineWidth = .08f;

            var heartR = CreateText(parent, "V5HeartRight", "♥", 42f, FontStyles.Bold,
                Hex("F23C98"), new Vector2(.84f, .79f), new Vector2(.91f, .84f));
            heartR.outlineColor = Color.white;
            heartR.outlineWidth = .08f;

            SetRect("TaglineRibbon", new Vector2(.34f, .695f), new Vector2(.76f, .735f));
            SetColor("TaglineRibbon", Hex("14B8B1"));
            var tag = FindTMP("TaglineRibbon/Tagline");
            if (tag != null)
            {
                tag.text = "LEARN  ♥  GROW  ♥  SHINE";
                tag.fontSize = 25f;
                tag.outlineColor = Hex("087A77");
                tag.outlineWidth = .10f;
            }
        }

        static void PolishJourney()
        {
            // Larger character presence, close to the approved reference composition.
            SetRect("JourneyCharacter", new Vector2(.01f, .285f), new Vector2(.46f, .69f));
            SetRect("JourneyRug", new Vector2(.01f, .245f), new Vector2(.47f, .53f));
            SetRect("JourneySpeechBubble", new Vector2(.27f, .545f), new Vector2(.55f, .655f));

            var bubble = GameObject.Find("JourneySpeechBubble");
            if (bubble != null)
            {
                SetColor("JourneySpeechBubble", Color.white);
                EnsureShadow(bubble, new Vector2(0f, -7f), Hex("55206D", .35f));
                EnsureOutline(bubble, Hex("6F259B"), new Vector2(3f, -3f));
            }

            var speech = FindTMP("JourneySpeechBubble/SpeechText");
            if (speech != null)
            {
                speech.text = "Hi Friend!\nLet's learn together!";
                speech.fontSize = 24f;
                speech.color = Hex("532079");
                speech.outlineWidth = 0f;
            }

            // Re-use the already-wired Journey voice button as a large glossy play control.
            // It intentionally overlaps the right side of the shorts so the damaged source pixels
            // are hidden while keeping Journey's current character and animation controller.
            SetRect("JourneyVoiceButton", new Vector2(.205f, .335f), new Vector2(.345f, .425f));
            SetColor("JourneyVoiceButton", Hex("E933A4"));
            var voice = GameObject.Find("JourneyVoiceButton");
            if (voice != null)
            {
                EnsureShadow(voice, new Vector2(0f, -8f), Hex("4C145E", .6f));
                EnsureOutline(voice, Hex("FFD24B"), new Vector2(4f, -4f));
            }
            var label = FindTMP("JourneyVoiceButton/Label");
            if (label != null)
            {
                label.text = "▶";
                label.fontSize = 44f;
                label.color = Color.white;
                label.outlineColor = Hex("6A1B75");
                label.outlineWidth = .10f;
            }
        }

        static void PolishGamePanel()
        {
            SetRect("GamePanel", new Vector2(.46f, .29f), new Vector2(.97f, .675f));
            SetColor("GamePanel", Hex("6C21B0", .98f));
            var panel = GameObject.Find("GamePanel");
            if (panel != null)
            {
                EnsureShadow(panel, new Vector2(0f, -13f), Hex("2D0A4E", .65f));
                EnsureOutline(panel, Hex("E5B8FF"), new Vector2(4f, -4f));
            }

            var choose = FindTMP("GamePanel/Choose");
            if (choose != null)
            {
                choose.text = "CHOOSE A GAME";
                choose.fontSize = 34f;
                choose.outlineColor = Hex("40105F");
                choose.outlineWidth = .10f;
            }

            StyleGameTile("Counting", new Vector2(.055f, .61f), new Vector2(.945f, .84f), Hex("FFB426"), "123", "COUNTING", "Numbers are fun!");
            StyleGameTile("ABC", new Vector2(.055f, .34f), new Vector2(.945f, .57f), Hex("F33394"), "ABC", "LETTERS", "Explore the alphabet!");
            StyleGameTile("Match", new Vector2(.055f, .07f), new Vector2(.945f, .30f), Hex("1EBBE6"), "A+", "ALPHABET MATCH", "Match letters & pictures!");
        }

        static void StyleGameTile(string name, Vector2 min, Vector2 max, Color color, string icon, string title, string subtitle)
        {
            var go = GameObject.Find(name);
            if (go == null) return;

            if (go.transform is RectTransform rect)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
            EnsureShadow(go, new Vector2(0f, -7f), Hex("27073F", .48f));
            EnsureOutline(go, Color.white, new Vector2(3f, -3f));

            var gloss = go.transform.Find("Gloss")?.GetComponent<Image>();
            if (gloss != null) gloss.color = new Color(1f, 1f, 1f, .22f);

            var iconText = go.transform.Find("IconBack/Icon")?.GetComponent<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = icon;
                iconText.fontSize = 43f;
                iconText.color = color;
            }

            var titleText = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = title;
                titleText.fontSize = title.Length > 12 ? 25f : 31f;
                titleText.outlineColor = Hex("5A1C5F", .65f);
                titleText.outlineWidth = .06f;
            }

            var sub = go.transform.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            if (sub != null)
            {
                sub.text = subtitle;
                sub.fontSize = 19f;
                sub.color = Color.white;
            }
        }

        static void PolishStartButton()
        {
            SetRect("StartBanner", new Vector2(.16f, .18f), new Vector2(.84f, .265f));
            SetColor("StartBanner", Hex("28CD18"));
            var start = GameObject.Find("StartBanner");
            if (start != null)
            {
                EnsureShadow(start, new Vector2(0f, -11f), Hex("145E0B", .72f));
                EnsureOutline(start, Hex("F2FF84"), new Vector2(4f, -4f));
            }

            var text = FindTMP("StartBanner/StartText");
            if (text != null)
            {
                text.text = "▶   PICK A GAME & PLAY!";
                text.fontSize = 37f;
                text.outlineColor = Hex("17610F");
                text.outlineWidth = .08f;
            }
        }

        static void PolishBottomNav()
        {
            SetRect("BottomNavBack", new Vector2(.012f, .012f), new Vector2(.988f, .16f));
            SetColor("BottomNavBack", Hex("4D0D89", .99f));
            var back = GameObject.Find("BottomNavBack");
            if (back != null)
            {
                EnsureShadow(back, new Vector2(0f, -8f), Hex("1B032B", .75f));
                EnsureOutline(back, Hex("8D32D2"), new Vector2(3f, -3f));
            }

            StyleNav("HomeTile", new Vector2(.018f, .08f), new Vector2(.245f, .92f), Hex("EF3A96"), "HOME", "⌂");
            StyleNav("LibraryTile", new Vector2(.26f, .08f), new Vector2(.49f, .92f), Hex("20B6E8"), "LIBRARY", "ABC");
            StyleNav("RewardsTile", new Vector2(.505f, .08f), new Vector2(.735f, .92f), Hex("FFA51B"), "REWARDS", "★");
            StyleNav("ParentsTile", new Vector2(.75f, .08f), new Vector2(.982f, .92f), Hex("9B42E3"), "PARENTS", "⚙");
        }

        static void StyleNav(string name, Vector2 min, Vector2 max, Color color, string label, string icon)
        {
            var go = GameObject.Find(name);
            if (go == null) return;

            if (go.transform is RectTransform rect)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
            EnsureShadow(go, new Vector2(0f, -8f), Hex("220638", .65f));
            EnsureOutline(go, Color.white, new Vector2(2f, -2f));

            var iconText = go.transform.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = icon;
                iconText.fontSize = 38f;
            }

            var title = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = label;
                title.fontSize = 18f;
                title.outlineColor = Hex("45145E", .65f);
                title.outlineWidth = .06f;
            }
        }

        static void AddReferenceDecor(Transform parent)
        {
            CreateDecor(parent, "V5Spark1", "✦", 34f, Hex("FFD743"), new Vector2(.22f, .86f));
            CreateDecor(parent, "V5Spark2", "✦", 28f, Color.white, new Vector2(.73f, .87f));
            CreateDecor(parent, "V5Spark3", "♥", 32f, Hex("EF3693"), new Vector2(.88f, .72f));
            CreateDecor(parent, "V5Spark4", "★", 32f, Hex("FFD743"), new Vector2(.10f, .73f));
        }

        static void CreateDecor(Transform parent, string name, string value, float size, Color color, Vector2 center)
        {
            var text = CreateText(parent, name, value, size, FontStyles.Bold, color,
                center - new Vector2(.03f, .025f), center + new Vector2(.03f, .025f));
            text.outlineColor = Color.white;
            text.outlineWidth = .05f;
        }

        static void SharpenText(Transform root)
        {
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.enableAutoSizing = false;
                tmp.enableWordWrapping = false;
                tmp.extraPadding = true;
                tmp.raycastTarget = false;
                if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
                    tmp.font = TMP_Settings.defaultFontAsset;
                tmp.UpdateMeshPadding();
            }
        }

        static void SetRect(string name, Vector2 min, Vector2 max)
        {
            var go = GameObject.Find(name);
            if (go == null || go.transform is not RectTransform rect) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetColor(string name, Color color)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        static void Tint(string name, Color color)
        {
            SetColor(name, color);
        }

        static TextMeshProUGUI FindTMP(string path)
        {
            var parts = path.Split('/');
            GameObject current = GameObject.Find(parts[0]);
            if (current == null) return null;
            Transform t = current.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                t = t.Find(parts[i]);
                if (t == null) return null;
            }
            return t.GetComponent<TextMeshProUGUI>();
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float fontSize,
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
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.enableAutoSizing = false;
            text.extraPadding = true;
            text.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
            text.UpdateMeshPadding();
            return text;
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
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void Offset(RectTransform rect, Vector2 delta)
        {
            rect.offsetMin += delta;
            rect.offsetMax += delta;
        }

        static void EnsureShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var shadow = go.GetComponent<Shadow>();
            if (shadow == null) shadow = go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void EnsureOutline(GameObject go, Color color, Vector2 distance)
        {
            if (go == null) return;
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
    }
}
#endif
