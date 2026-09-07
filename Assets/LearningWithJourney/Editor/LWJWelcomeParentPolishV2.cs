#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJWelcomeParentPolishV2
    {
        const string Folder = "Assets/LearningWithJourney/Scenes/";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Update Welcome and Parent Screens V2")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode first.", "OK");
                return;
            }
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png");
            if (rounded == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The approved Main Menu RoundedPanel sprite is missing. No changes made.", "OK");
                return;
            }
            string[] names = { "AccessGate", "NameSetup", "ParentZone" };
            foreach (string name in names)
                if (!File.Exists(Folder + name + ".unity"))
                {
                    EditorUtility.DisplayDialog("Learning with Journey", name + " scene is missing. No scenes were changed.", "OK");
                    return;
                }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string backup = Path.Combine("LWJSceneBackups", "WelcomeParent-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            foreach (string name in names)
            {
                string path = Folder + name + ".unity";
                File.Copy(path, Path.Combine(backup, name + ".unity"), false);
                Scene scene = SceneManager.GetSceneByPath(path);
                bool wasOpen = scene.IsValid() && scene.isLoaded;
                if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    Style(scene);
                    if (name == "ParentZone") FixPortrait(scene);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save " + path);
                }
                finally { if (!wasOpen) EditorSceneManager.CloseScene(scene, true); }
            }
            EditorUtility.DisplayDialog("Learning with Journey",
                "Access Gate, Name Setup, and Parent Zone updated and saved. Original scenes: " + backup +
                "\nTest each scene in Game view. Trial, purchases, narration, names, and progress logic were not changed.", "OK");
        }

        static void Style(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                {
                    string n = image.name;
                    if (n == "Background" || n == "WelcomeWall") { image.color = Hex("EF78AF"); continue; }
                    if (n == "TopGlow") { image.color = new Color(1f, 1f, 1f, .03f); continue; }
                    if (n == "Floor") { image.color = Hex("A34219"); continue; }
                    // Preserve progress-fill rendering, masks, and their existing behavior.
                    if (image.type == Image.Type.Filled || n == "Fill" || n == "SpeechTail") continue;
                    if (image.GetComponent<Button>() != null ||
                        Is(n, "AccessCard", "TrialBox", "PurchaseBox", "WelcomeArch", "NameCard",
                        "BrandRibbon", "JourneyWelcomeBubble", "Rug", "NameInput", "ChildNameInput",
                        "ProfileArea", "ProfileCard", "Games", "Streak", "Bubble", "JourneyBackpack", "Flap", "Pocket"))
                    {
                        image.sprite = rounded;
                        image.type = Image.Type.Sliced;
                        foreach (Shadow effect in image.GetComponents<Shadow>()) effect.enabled = false;
                        if (n == "WelcomeArch" || n == "Rug") image.color = Hex("8E44D4");
                        if (n == "AccessCard") image.color = Hex("6020A3");
                        if (n == "TrialBox" || n == "NameCard") image.color = Hex("FFF7FC");
                        if (n == "PurchaseBox") image.color = Hex("F5EDFF");
                    }
                    if (image.GetComponent<Button>() != null)
                    {
                        Outline outline = image.GetComponent<Outline>();
                        if (outline == null) outline = image.gameObject.AddComponent<Outline>();
                        outline.enabled = true;
                        outline.effectColor = Color.white;
                        outline.effectDistance = new Vector2(3f, -3f);
                        Transform shadow = image.transform.parent.Find(n + "Shadow");
                        if (shadow != null && shadow.TryGetComponent<Image>(out var oldShadow))
                        {
                            oldShadow.sprite = rounded;
                            oldShadow.type = Image.Type.Sliced;
                        }
                        if (image.transform.Find("WelcomeGlossV2") == null && image.transform.Find("MenuThemeGloss") == null)
                        {
                            var gloss = new GameObject("WelcomeGlossV2", typeof(RectTransform), typeof(Image));
                            gloss.transform.SetParent(image.transform, false);
                            Rect((RectTransform)gloss.transform, .04f, .65f, .96f, .92f);
                            var g = gloss.GetComponent<Image>();
                            g.sprite = rounded;
                            g.type = Image.Type.Sliced;
                            g.color = new Color(1f, 1f, 1f, .23f);
                            g.raycastTarget = false;
                            gloss.transform.SetAsFirstSibling();
                        }
                    }
                }
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    bool top = text.transform.parent != null && text.transform.parent.GetComponent<Canvas>() != null;
                    if (top && Is(text.name, "Title", "Brand")) text.color = Color.white;
                    if (text.name == "Heading" && text.transform.parent.name == "AccessCard") text.color = Color.white;
                    if (top && Is(text.name, "Status", "Privacy", "LocalNameNote", "Subtitle"))
                    {
                        text.color = Hex("48206D");
                        text.enableAutoSizing = true;
                        text.fontSizeMin = 18f;
                        text.fontSizeMax = 24f;
                        text.textWrappingMode = TextWrappingModes.Normal;
                    }
                }
            }
        }

        static void FixPortrait(Scene scene)
        {
            RectTransform frame = Find(scene, "ProfileArea");
            if (frame == null) throw new InvalidOperationException("ParentZone ProfileArea not found; restore its backup if needed.");
            RectTransform journey = frame.Find("Journey") as RectTransform;
            RectTransform bubble = frame.Find("Bubble") as RectTransform;
            RectTransform bag = Find(scene, "JourneyBackpack");
            if (journey == null || bubble == null || bag == null)
                throw new InvalidOperationException("ParentZone portrait components not found.");
            // Reserve a separate speech row above the full figure. V3 placed
            // the bubble at .70 while Journey extended to .86, hiding her hair.
            Rect(bubble, .015f, .77f, .31f, .98f);
            Canvas.ForceUpdateCanvases();
            // Match the full-length NameSetup figure's approximately 0.606 aspect,
            // instead of stretching the portrait across the wide profile column.
            float width = frame.rect.width > 0f ? .68f * frame.rect.height * .606f / frame.rect.width : .17f;
            width = Mathf.Clamp(width, .08f, .27f);
            Rect(journey, .16f - width / 2f, .05f, .16f + width / 2f, .73f);
            // Attach the bag to the portrait so both scale and move together.
            // The shorts spot is on the screen-right side of the lower figure.
            bag.SetParent(journey, false);
            Rect(bag, .58f, .11f, .96f, .38f);
            bag.SetAsLastSibling();
        }

        static RectTransform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                    if (rect.name == name) return rect;
            return null;
        }
        static void Rect(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
        static bool Is(string value, params string[] names) => Array.IndexOf(names, value) >= 0;
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color c); return c; }
    }
}
#endif
