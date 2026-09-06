#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJRewardsCleanupV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/RewardsRoom.unity";
        const string RootName = "RewardsCleanupV4";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Polish Rewards V4 Clean + Readable")]
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
            root.transform.SetSiblingIndex(Mathf.Min(7, canvas.transform.childCount - 1));

            FixSpeechBubble();
            CleanUnsupportedPlaques();
            AddCleanRewardRoomAccents(root.transform);
            TightenChestDepth();
            ImproveRewardProgress();
            ImproveHeaderReadability();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Rewards V4 applied. Journey's speech bubble now has an attached comic-style tail, the stray award plaques were removed, the chest side-depth was tightened, reward progress is easier to see, the level text is more readable, and unsupported star/crown text glyphs were removed to prevent TMP fallback warnings from this screen.",
                "OK");
        }

        static void FixSpeechBubble()
        {
            var bubble = GameObject.Find("JourneyRewardsBubble");
            if (bubble == null) return;

            var rect = bubble.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.065f, .665f);
                rect.anchorMax = new Vector2(.445f, .765f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }

            var image = bubble.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = Color.white;
            }

            DestroyChild(bubble.transform, "SpeechTailV3");
            DestroyChild(bubble.transform, "SpeechTailTipV3");
            DestroyChild(bubble.transform, "SpeechTailV4Shadow");
            DestroyChild(bubble.transform, "SpeechTailV4");

            // Tail is deliberately tucked into the bubble so it reads as one connected shape,
            // not a detached diamond over Journey's hair.
            var shadow = AddImage(bubble.transform, "SpeechTailV4Shadow",
                new Vector2(.115f, -.105f), new Vector2(.235f, .085f), Hex("70409E", .36f));
            shadow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 43f);
            shadow.transform.SetAsFirstSibling();

            var tail = AddImage(bubble.transform, "SpeechTailV4",
                new Vector2(.105f, -.085f), new Vector2(.225f, .105f), Color.white);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 43f);
            tail.transform.SetSiblingIndex(1);

            var speech = bubble.transform.Find("Speech");
            if (speech != null)
            {
                var tr = speech.GetComponent<RectTransform>();
                var text = speech.GetComponent<TMP_Text>();
                if (tr != null)
                {
                    tr.anchorMin = new Vector2(.07f, .10f);
                    tr.anchorMax = new Vector2(.93f, .90f);
                    tr.offsetMin = tr.offsetMax = Vector2.zero;
                }
                if (text != null)
                {
                    text.fontSize = 24f;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 18f;
                    text.fontSizeMax = 25f;
                    text.textWrappingMode = TextWrappingModes.Normal;
                    text.alignment = TextAlignmentOptions.Center;
                    text.margin = new Vector4(14f, 8f, 14f, 8f);
                }
            }
        }

        static void CleanUnsupportedPlaques()
        {
            // V3 used star/crown Unicode symbols that are not in LiberationSans SDF.
            // Remove those decorative text objects and replace them with sprite-free UI accents.
            DestroyIfExists("PlaqueStar");
            DestroyIfExists("PlaqueCrown");
            DestroyIfExists("PlaqueTrophy");
        }

        static void AddCleanRewardRoomAccents(Transform parent)
        {
            // Simple dimensional medal tiles, built only from Images so there are no font glyph issues.
            AddMedal(parent, "MedalGold", new Vector2(.62f, .700f), new Vector2(.685f, .755f), Hex("F4BF33"), Hex("FFF1A6"));
            AddMedal(parent, "MedalPink", new Vector2(.705f, .700f), new Vector2(.770f, .755f), Hex("ED5BA7"), Hex("FFD6EB"));
            AddMedal(parent, "MedalBlue", new Vector2(.790f, .700f), new Vector2(.855f, .755f), Hex("43C8DF"), Hex("DDF9FF"));

            // Small gold shelf beneath the medals helps the wall read as a reward display.
            var shelfShadow = AddImage(parent, "MedalShelfShadow", new Vector2(.595f, .684f), new Vector2(.875f, .697f), Hex("7A4B1E", .28f));
            shelfShadow.transform.SetAsFirstSibling();
            var shelf = AddImage(parent, "MedalShelf", new Vector2(.59f, .690f), new Vector2(.87f, .703f), Hex("D9A02B"));
            shelf.transform.SetAsFirstSibling();
        }

        static void AddMedal(Transform parent, string name, Vector2 min, Vector2 max, Color body, Color shine)
        {
            var shadow = AddImage(parent, name + "Shadow", min + new Vector2(.008f, -.006f), max + new Vector2(.008f, -.006f), Hex("4E246C", .24f));
            shadow.transform.SetAsFirstSibling();

            var medal = AddImage(parent, name, min, max, body);
            AddOutline(medal.gameObject, Hex("FFF4C1", .85f), new Vector2(2f, -2f));
            var inner = AddImage(medal.transform, "Inner", new Vector2(.18f, .18f), new Vector2(.82f, .82f), Hex("6B32A5", .88f));
            AddImage(inner.transform, "Shine", new Vector2(.18f, .58f), new Vector2(.82f, .82f), shine);
            medal.transform.SetAsFirstSibling();
        }

        static void TightenChestDepth()
        {
            var chest = GameObject.Find("TreasureCase");
            if (chest == null) return;

            var body = chest.transform.Find("ChestBody");
            if (body != null)
            {
                SetAnchors(body.Find("ChestDetailsV2/RightDepth") as RectTransform, .90f, .10f, .985f, .90f);
                SetAnchors(body.Find("ChestDetailsV2/RightGoldEdge") as RectTransform, .915f, .11f, .975f, .90f);
                SetAnchors(body.Find("RightDepthV3") as RectTransform, .945f, .12f, 1.005f, .86f);
                SetAnchors(body.Find("BottomDepthV3") as RectTransform, .10f, -.045f, .92f, .055f);
            }

            var lid = chest.transform.Find("ChestLid");
            if (lid != null)
            {
                SetAnchors(lid.Find("LidDetailsV2/LidRightDepth") as RectTransform, .91f, .10f, .985f, .87f);
            }

            var chestRect = chest.GetComponent<RectTransform>();
            if (chestRect != null)
            {
                chestRect.anchorMin = new Vector2(.405f, .345f);
                chestRect.anchorMax = new Vector2(.925f, .605f);
                chestRect.offsetMin = chestRect.offsetMax = Vector2.zero;
            }
        }

        static void ImproveRewardProgress()
        {
            var panel = GameObject.Find("NextRewardPanel");
            if (panel == null) return;

            var rect = panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(.165f, .235f);
                rect.anchorMax = new Vector2(.835f, .335f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }

            var title = panel.transform.Find("Title")?.GetComponent<TMP_Text>();
            if (title != null)
            {
                title.fontSize = 27f;
                title.enableAutoSizing = true;
                title.fontSizeMin = 20f;
                title.fontSizeMax = 28f;
            }

            for (int i = 1; i <= 5; i++)
            {
                var marker = panel.transform.Find("RewardMarker" + i)?.GetComponent<Image>();
                if (marker == null) continue;
                AddOutline(marker.gameObject, Hex("B88BE4", .75f), new Vector2(2f, -2f));
            }

            var counterCard = panel.transform.Find("CounterCard");
            if (counterCard != null)
            {
                var small = counterCard.Find("Small")?.GetComponent<TMP_Text>();
                if (small != null)
                {
                    small.fontSize = 18f;
                    small.fontStyle = FontStyles.Bold;
                }
                var progress = counterCard.Find("Progress")?.GetComponent<TMP_Text>();
                if (progress != null)
                {
                    progress.fontSize = 38f;
                    progress.enableAutoSizing = true;
                    progress.fontSizeMin = 26f;
                    progress.fontSizeMax = 40f;
                }
            }
        }

        static void ImproveHeaderReadability()
        {
            var level = GameObject.Find("LevelPill");
            if (level != null)
            {
                var rect = level.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(.385f, .885f);
                    rect.anchorMax = new Vector2(.615f, .925f);
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                }
                var text = level.transform.Find("Level")?.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.fontSize = 23f;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 18f;
                    text.fontSizeMax = 24f;
                }
            }
        }

        static void SetAnchors(RectTransform rect, float x1, float y1, float x2, float y2)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(x1, y1);
            rect.anchorMax = new Vector2(x2, y2);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static Image AddImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
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
            image.raycastTarget = false;
            return image;
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        static void DestroyChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
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
