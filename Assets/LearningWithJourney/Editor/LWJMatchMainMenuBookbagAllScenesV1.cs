#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    /// <summary>
    /// Applies the approved Main Menu backpack artwork to every scene that has
    /// a Journey backpack, while preserving each scene's existing position,
    /// parent, scale, and gameplay listeners.
    /// </summary>
    public static class LWJMatchMainMenuBookbagAllScenesV1
    {
        const string SceneFolder = "Assets/LearningWithJourney/Scenes/";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        static Sprite rounded;

        static readonly string[] SceneNames =
        {
            "MainMenu", "AccessGate", "NameSetup", "Library", "BookReader",
            "ParentZone", "RewardsRoom", "ABCWorld", "CountingWorld", "AlphabetMatchWorld"
        };

        [MenuItem("Learning with Journey/Match Main Menu Bookbag Across All Scenes V1")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode first.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            if (rounded == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey",
                    "The approved Main Menu RoundedPanel sprite is missing. No scenes were changed.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string backup = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "LWJSceneBackups",
                "MainMenuBookbagAllScenes-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);

            var changed = new List<string>();
            var skipped = new List<string>();

            foreach (string sceneName in SceneNames)
            {
                string path = SceneFolder + sceneName + ".unity";
                if (!File.Exists(path))
                {
                    skipped.Add(sceneName + " (scene missing)");
                    continue;
                }

                File.Copy(path, Path.Combine(backup, sceneName + ".unity"), false);
                Scene scene = SceneManager.GetSceneByPath(path);
                bool wasOpen = scene.IsValid() && scene.isLoaded;
                if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

                try
                {
                    Image bag = FindBag(scene);
                    if (bag == null)
                    {
                        skipped.Add(sceneName + " (no Journey backpack found)");
                        continue;
                    }

                    Undo.RegisterFullObjectHierarchyUndo(bag.gameObject, "Match Main Menu bookbag artwork");
                    ApplyArtwork(bag);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene, path))
                        throw new IOException("Could not save " + path);
                    changed.Add(sceneName);
                }
                finally
                {
                    if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
                }
            }

            string message = "Main Menu bookbag artwork applied to: " +
                (changed.Count == 0 ? "none" : string.Join(", ", changed.ToArray())) +
                "\n\nEach scene's current bookbag position and size were preserved." +
                "\n\nBackup: " + backup;
            if (skipped.Count > 0)
                message += "\n\nSkipped: " + string.Join(", ", skipped.ToArray());
            message += "\n\nThe scenes were saved. Test the scenes in Game view.";
            EditorUtility.DisplayDialog("Learning with Journey", message, "OK");
        }

        static Image FindBag(Scene scene)
        {
            Image fallback = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                {
                    if (image.name == "JourneyVoiceButton") return image;
                    if (image.name == "JourneyBackpack") fallback = image;
                }
            }
            return fallback;
        }

        static void ApplyArtwork(Image bag)
        {
            foreach (string name in new[]
            {
                "Flap", "Pocket", "J", "Handle", "LeftStrap", "RightStrap", "Shine",
                "BagArtV3", "V7BagHandle", "V7BagStrapL", "V7BagStrapR", "V7BagFlap",
                "V7BagPocket", "V7BagBadge", "V7BagShine", "MainMenuBagArtV1"
            })
            {
                Transform child = bag.transform.Find(name);
                if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
            }

            bag.sprite = rounded;
            bag.type = Image.Type.Sliced;
            bag.pixelsPerUnitMultiplier = 1f;
            bag.color = Hex("D839A5");
            bag.raycastTarget = bag.GetComponent<Button>() != null;
            ClearEffects(bag.gameObject);

            var leftStrap = CreatePanel(bag.transform, "V7BagStrapL",
                new Vector2(-.05f, .18f), new Vector2(.18f, .76f), Hex("74228D"));
            leftStrap.transform.SetAsFirstSibling();
            var rightStrap = CreatePanel(bag.transform, "V7BagStrapR",
                new Vector2(.82f, .18f), new Vector2(1.05f, .76f), Hex("74228D"));
            rightStrap.transform.SetAsFirstSibling();

            var handle = CreatePanel(bag.transform, "V7BagHandle",
                new Vector2(.30f, .82f), new Vector2(.70f, 1.10f), Hex("6D208C"));
            handle.transform.SetAsFirstSibling();

            var flap = CreatePanel(bag.transform, "V7BagFlap",
                new Vector2(.08f, .49f), new Vector2(.92f, .86f), Hex("F15BB7"));
            EnsureShadow(flap.gameObject, new Vector2(0f, -4f), Hex("5C155F", .50f));
            EnsureOutline(flap.gameObject, Hex("FFC1EC"), new Vector2(2f, -2f));

            var pocket = CreatePanel(bag.transform, "V7BagPocket",
                new Vector2(.15f, .08f), new Vector2(.85f, .43f), Hex("B62D9D"));
            EnsureShadow(pocket.gameObject, new Vector2(0f, -3f), Hex("541058", .48f));
            EnsureOutline(pocket.gameObject, Hex("FA8BD4"), new Vector2(2f, -2f));

            var badge = CreateText(bag.transform, "V7BagBadge", "J", 32f,
                new Vector2(.36f, .13f), new Vector2(.64f, .40f));
            badge.outlineColor = Hex("64166C");
            badge.outlineWidth = .12f;

            var shine = CreatePanel(bag.transform, "V7BagShine",
                new Vector2(.14f, .67f), new Vector2(.86f, .87f), new Color(1f, 1f, 1f, .21f));
            shine.raycastTarget = false;

            EnsureShadow(bag.gameObject, new Vector2(0f, -8f), Hex("43104F", .68f));
            EnsureOutline(bag.gameObject, Hex("FFD24A"), new Vector2(4f, -4f));
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, Vector2 min, Vector2 max)
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
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            return text;
        }

        static void ClearEffects(GameObject go)
        {
            foreach (Shadow shadow in go.GetComponents<Shadow>()) Undo.DestroyObjectImmediate(shadow);
            foreach (Outline outline in go.GetComponents<Outline>()) Undo.DestroyObjectImmediate(outline);
        }

        static void EnsureShadow(GameObject go, Vector2 distance, Color color)
        {
            var shadow = Undo.AddComponent<Shadow>(go);
            shadow.effectDistance = distance;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        static void EnsureOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = Undo.AddComponent<Outline>(go);
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out Color color);
            color.a = alpha;
            return color;
        }
    }
}
#endif
