#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJRewardsPolishV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/RewardsRoom.unity";
        const string RootName = "RewardsPolishV3";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Polish Rewards Layout + Speech Bubble V3")]
        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "RewardsRoom.unity was not found. Build Rewards V1 first.", "OK");
                return;
            }

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;

            DestroyIfExists(RootName);
            var root = new GameObject(RootName, typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            root.transform.SetSiblingIndex(6);

            CleanBackground();
            BuildRewardWall(root.transform);
            PolishSpeechBubble();
            PolishPrizeReveal();
            PolishChestPose();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Rewards V3 applied. The room is cleaner, the old sign/clutter is reduced, Journey now has a real speech-bubble shape with a tail, the prize reveal is easier to read, and the treasure chest sits more naturally when opened.",
                "OK");
        }

        static void CleanBackground()
        {
            DestroyIfExists("SuccessSign");
            DestroyIfExists("WindowFrame");

            var decorV2 = GameObject.Find("RewardsRoomDecorV2");
            if (decorV2 != null)
            {
                foreach (Transform child in decorV2.transform)
                {
                    if (child.name.Contains("Shelf") || child.name.Contains("Case") || child.name.Contains("Card"))
                        child.gameObject.SetActive(false);
                }
            }
        }

        static void BuildRewardWall(Transform parent)
        {
            // Calm trophy-wall treatment behind the active UI. Keeps the room visually rich
            // without competing with Journey, the chest, or the reward text.
            var wallPanel = AddImage(parent, "AwardWallPanel", new Vector2(.035f, .43f), new Vector2(.965f, .79f), Hex("FBE7F4", .72f));
            AddOutline(wallPanel.gameObject, Hex("D6A0E4", .75f), new Vector2(2f, -2f));
            wallPanel.transform.SetAsFirstSibling();

            var railTop = AddImage(parent, "GoldRailTop", new Vector2(.05f, .755f), new Vector2(.95f, .768f), Hex("E6B53A", .92f));
            railTop.transform.SetAsFirstSibling();
            var railBottom = AddImage(parent, "GoldRailBottom", new Vector2(.05f, .445f), new Vector2(.95f, .458f), Hex("BE8430", .78f));
            railBottom.transform.SetAsFirstSibling();

            AddAwardPlaque(parent, "PlaqueStar", new Vector2(.055f, .585f), new Vector2(.17f, .69f), "★", "STAR");
            AddAwardPlaque(parent, "PlaqueCrown", new Vector2(.80f, .59f), new Vector2(.915f, .695f), "♛", "CROWN");
            AddAwardPlaque(parent, "PlaqueTrophy", new Vector2(.80f, .465f), new Vector2(.915f, .57f), "★", "TROPHY");
        }

        static void AddAwardPlaque(Transform parent, string name, Vector2 min, Vector2 max, string icon, string caption)
        {
            var panel = AddImage(parent, name, min, max, Hex("6E39B8", .90f));
            AddShadow(panel.gameObject, new Vector2(0f, -5f), Hex("4B256A", .28f));
            AddOutline(panel.gameObject, Hex("F4CC59"), new Vector2(3f, -3f));
            var t = AddText(panel.transform, "Icon", icon, 38f, FontStyles.Bold, Hex("FFD54C"), new Vector2(.08f, .34f), new Vector2(.92f, .93f));
            t.alignment = TextAlignmentOptions.Center;
            var c = AddText(panel.transform, "Caption", caption, 15f, FontStyles.Bold, Color.white, new Vector2(.08f, .05f), new Vector2(.92f, .34f));
            c.alignment = TextAlignmentOptions.Center;
        }

        static void PolishSpeechBubble()
        {
            var bubble = GameObject.Find("JourneyRewardsBubble");
            if (bubble == null) return;

            var rect = bubble.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.07f, .665f);
                rect.anchorMax = new Vector2(.44f, .765f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }

            var image = bubble.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = Color.white;
            }

            var oldTail = bubble.transform.Find("SpeechTailV3");
            if (oldTail != null) Object.DestroyImmediate(oldTail.gameObject);

            var tail = AddImage(bubble.transform, "SpeechTailV3", new Vector2(.12f, -.19f), new Vector2(.29f, .11f), Color.white);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            AddOutline(tail.gameObject, Hex("8D4CC3"), new Vector2(2f, -2f));
            tail.transform.SetAsFirstSibling();

            var tailTip = AddImage(bubble.transform, "SpeechTailTipV3", new Vector2(.11f, -.22f), new Vector2(.20f, -.09f), Color.white);
            tailTip.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tailTip.transform.SetSiblingIndex(1);

            var speech = bubble.transform.Find("Speech");
            if (speech != null)
            {
                var text = speech.GetComponent<TMP_Text>();
                var tr = speech.GetComponent<RectTransform>();
                if (tr != null)
                {
                    tr.anchorMin = new Vector2(.075f, .10f);
                    tr.anchorMax = new Vector2(.925f, .90f);
                    tr.offsetMin = tr.offsetMax = Vector2.zero;
                }
                if (text != null)
                {
                    text.fontSize = 23f;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 18f;
                    text.fontSizeMax = 24f;
                    text.textWrappingMode = TextWrappingModes.Normal;
                    text.alignment = TextAlignmentOptions.Center;
                    text.margin = new Vector4(10f, 5f, 10f, 5f);
                }
            }
        }

        static void PolishPrizeReveal()
        {
            var prize = GameObject.Find("PrizeReveal");
            if (prize == null) return;

            var rect = prize.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.54f, .465f);
                rect.anchorMax = new Vector2(.82f, .605f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }

            var gift = prize.transform.Find("GiftBox") as RectTransform;
            if (gift != null)
            {
                gift.anchorMin = new Vector2(.30f, .08f);
                gift.anchorMax = new Vector2(.70f, .54f);
                gift.offsetMin = gift.offsetMax = Vector2.zero;
            }

            var title = prize.transform.Find("PrizeTitle") as RectTransform;
            if (title != null)
            {
                title.anchorMin = new Vector2(.00f, .65f);
                title.anchorMax = new Vector2(1f, .82f);
                title.offsetMin = title.offsetMax = Vector2.zero;
                var t = title.GetComponent<TMP_Text>();
                if (t != null)
                {
                    t.fontSize = 21f;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 15f;
                    t.fontSizeMax = 23f;
                }
            }

            var amount = prize.transform.Find("PrizeAmount") as RectTransform;
            if (amount != null)
            {
                amount.anchorMin = new Vector2(.00f, .83f);
                amount.anchorMax = new Vector2(1f, .99f);
                amount.offsetMin = amount.offsetMax = Vector2.zero;
                var t = amount.GetComponent<TMP_Text>();
                if (t != null)
                {
                    t.fontSize = 16f;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 12f;
                    t.fontSizeMax = 18f;
                }
            }
        }

        static void PolishChestPose()
        {
            var chest = GameObject.Find("TreasureCase");
            if (chest == null) return;

            var rect = chest.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.40f, .34f);
                rect.anchorMax = new Vector2(.94f, .61f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }

            var lid = chest.transform.Find("ChestLid") as RectTransform;
            if (lid != null)
            {
                lid.localRotation = Quaternion.identity;
                lid.localScale = Vector3.one;
            }

            var body = chest.transform.Find("ChestBody");
            if (body != null)
            {
                var rightFace = body.Find("RightDepthV3");
                if (rightFace == null)
                {
                    var panel = AddImage(body, "RightDepthV3", new Vector2(.93f, .10f), new Vector2(1.04f, .88f), Hex("4A177D", .78f));
                    panel.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -2f);
                    panel.transform.SetAsFirstSibling();
                }
                var bottomFace = body.Find("BottomDepthV3");
                if (bottomFace == null)
                {
                    var panel = AddImage(body, "BottomDepthV3", new Vector2(.08f, -.08f), new Vector2(.94f, .08f), Hex("4C1B72", .82f));
                    panel.transform.SetAsFirstSibling();
                }
            }
        }

        static Image AddImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
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
            image.raycastTarget = false;
            return image;
        }

        static TMP_Text AddText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
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

        static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                c.a = alpha;
                return c;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
#endif
