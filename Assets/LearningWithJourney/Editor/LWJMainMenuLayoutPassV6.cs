#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuLayoutPassV6
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Apply Layout Cleanup V6")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            MakeGameButtonsSmaller();
            ConvertJourneyButtonToBackpack();
            CompactJourneySpeechBubble();
            ReplaceUnsupportedSparkles(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Layout Cleanup V6 applied: smaller game buttons, Journey's pink control is now a backpack button, and her speech bubble is smaller with cleaner text spacing.",
                "OK");
        }

        static void MakeGameButtonsSmaller()
        {
            // Slightly smaller panel so Journey and the classroom have more breathing room.
            SetRect("GamePanel", new Vector2(.50f, .315f), new Vector2(.945f, .655f));

            var choose = FindTMP("GamePanel/Choose");
            if (choose != null)
            {
                choose.fontSize = 27f;
                choose.rectTransform.anchorMin = new Vector2(.08f, .84f);
                choose.rectTransform.anchorMax = new Vector2(.92f, .96f);
                choose.rectTransform.offsetMin = Vector2.zero;
                choose.rectTransform.offsetMax = Vector2.zero;
            }

            SetTile("Counting", "CountingShadow", new Vector2(.10f, .61f), new Vector2(.90f, .79f));
            SetTile("ABC", "ABCShadow", new Vector2(.10f, .385f), new Vector2(.90f, .565f));
            SetTile("Match", "MatchShadow", new Vector2(.10f, .16f), new Vector2(.90f, .34f));

            PolishTileText("Counting", 25f, 16f, 35f);
            PolishTileText("ABC", 25f, 16f, 35f);
            PolishTileText("Match", 21f, 14f, 32f);
        }

        static void SetTile(string tileName, string shadowName, Vector2 min, Vector2 max)
        {
            var tile = GameObject.Find(tileName);
            if (tile != null && tile.transform is RectTransform tileRect)
            {
                tileRect.anchorMin = min;
                tileRect.anchorMax = max;
                tileRect.offsetMin = Vector2.zero;
                tileRect.offsetMax = Vector2.zero;
            }

            var shadow = GameObject.Find(shadowName);
            if (shadow != null && shadow.transform is RectTransform shadowRect)
            {
                shadowRect.anchorMin = min + new Vector2(0f, -.012f);
                shadowRect.anchorMax = max + new Vector2(0f, -.012f);
                shadowRect.offsetMin = Vector2.zero;
                shadowRect.offsetMax = Vector2.zero;
            }
        }

        static void PolishTileText(string tileName, float titleSize, float subtitleSize, float iconSize)
        {
            var tile = GameObject.Find(tileName);
            if (tile == null) return;

            var title = tile.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.fontSize = titleSize;
                title.enableWordWrapping = false;
            }

            var subtitle = tile.transform.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
            if (subtitle != null)
            {
                subtitle.fontSize = subtitleSize;
                subtitle.enableWordWrapping = false;
            }

            var icon = tile.transform.Find("IconBack/Icon")?.GetComponent<TextMeshProUGUI>();
            if (icon != null)
                icon.fontSize = iconSize;
        }

        static void ConvertJourneyButtonToBackpack()
        {
            var bag = GameObject.Find("JourneyVoiceButton");
            if (bag == null) return;

            // Keep the existing Button component and its greeting click event.
            SetRect("JourneyVoiceButton", new Vector2(.215f, .335f), new Vector2(.335f, .415f));

            var body = bag.GetComponent<Image>();
            if (body != null)
            {
                body.color = Hex("9C35D5");
                body.raycastTarget = true;
            }

            RemoveChild(bag.transform, "Label");
            RemoveChild(bag.transform, "BagHandle");
            RemoveChild(bag.transform, "BagFlap");
            RemoveChild(bag.transform, "BagPocket");
            RemoveChild(bag.transform, "BagBadge");
            RemoveChild(bag.transform, "BagHighlight");

            Sprite rounded = body != null ? body.sprite : null;

            var handle = CreateBagPart(bag.transform, "BagHandle", rounded, Hex("7A23B7"),
                new Vector2(.31f, .82f), new Vector2(.69f, 1.08f));
            handle.transform.SetAsFirstSibling();

            var flap = CreateBagPart(bag.transform, "BagFlap", rounded, Hex("C755ED"),
                new Vector2(.10f, .52f), new Vector2(.90f, .87f));
            AddShadow(flap.gameObject, new Vector2(0f, -3f), Hex("4C1767", .35f));

            var highlight = CreateBagPart(bag.transform, "BagHighlight", rounded, new Color(1f, 1f, 1f, .20f),
                new Vector2(.15f, .69f), new Vector2(.85f, .84f));
            highlight.raycastTarget = false;

            var pocket = CreateBagPart(bag.transform, "BagPocket", rounded, Hex("E147A5"),
                new Vector2(.20f, .10f), new Vector2(.80f, .45f));
            AddShadow(pocket.gameObject, new Vector2(0f, -2f), Hex("4C1767", .28f));

            var badgeGO = new GameObject("BagBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeGO.transform.SetParent(bag.transform, false);
            var badgeRect = (RectTransform)badgeGO.transform;
            badgeRect.anchorMin = new Vector2(.31f, .15f);
            badgeRect.anchorMax = new Vector2(.69f, .40f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            var badge = badgeGO.GetComponent<TextMeshProUGUI>();
            badge.text = "J";
            badge.fontSize = 25f;
            badge.fontStyle = FontStyles.Bold;
            badge.alignment = TextAlignmentOptions.Center;
            badge.color = Color.white;
            badge.raycastTarget = false;
            badge.outlineColor = Hex("6D1D8E");
            badge.outlineWidth = .12f;
        }

        static Image CreateBagPart(Transform parent, string name, Sprite sprite, Color color, Vector2 min, Vector2 max)
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
            image.type = sprite != null ? Image.Type.Simple : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void CompactJourneySpeechBubble()
        {
            SetRect("JourneySpeechBubble", new Vector2(.285f, .555f), new Vector2(.505f, .625f));

            var bubble = GameObject.Find("JourneySpeechBubble");
            if (bubble != null)
            {
                var img = bubble.GetComponent<Image>();
                if (img != null) img.color = Color.white;
            }

            var speech = FindTMP("JourneySpeechBubble/SpeechText");
            if (speech != null)
            {
                speech.text = "Hi Friend!\nLet's learn\ntogether!";
                speech.fontSize = 18f;
                speech.enableWordWrapping = true;
                speech.alignment = TextAlignmentOptions.Center;
                speech.lineSpacing = 3f;
                speech.margin = new Vector4(8f, 6f, 8f, 6f);
                speech.overflowMode = TextOverflowModes.Overflow;
                speech.rectTransform.anchorMin = new Vector2(.06f, .08f);
                speech.rectTransform.anchorMax = new Vector2(.94f, .92f);
                speech.rectTransform.offsetMin = Vector2.zero;
                speech.rectTransform.offsetMax = Vector2.zero;
            }
        }

        static void ReplaceUnsupportedSparkles(Transform root)
        {
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.text != null && tmp.text.Contains("✦"))
                    tmp.text = tmp.text.Replace("✦", "★");
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

        static TMP_Text FindTMP(string path)
        {
            var go = GameObject.Find(path);
            return go != null ? go.GetComponent<TMP_Text>() : null;
        }

        static void RemoveChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
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
