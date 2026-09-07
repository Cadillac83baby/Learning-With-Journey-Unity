#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    // Visual-only pass: never rebuilds scenes, replaces buttons, or changes controller data.
    public static class LWJSharedMenuThemeV1
    {
        const string Folder = "Assets/LearningWithJourney/Scenes/";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        static readonly string[] Scenes = {
            "Library", "BookReader", "ParentZone", "RewardsRoom",
            "ABCWorld", "CountingWorld", "AlphabetMatchWorld"
        };
        static Sprite rounded;
        static readonly Color Pink = Hex("EF78AF");
        static readonly Color Purple = Hex("6020A3");

        [MenuItem("Learning with Journey/Match All Game Screens to Menu V1")]
        public static void ApplyAll()
        {
            if (!Ready()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string backup = Path.Combine("LWJSceneBackups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            var changed = new List<string>();
            var missing = new List<string>();
            foreach (string name in Scenes)
            {
                string path = Folder + name + ".unity";
                if (!File.Exists(path)) { missing.Add(name); continue; }
                // Copy the saved scene before editing, including scenes already open.
                File.Copy(path, Path.Combine(backup, name + ".unity"), false);
                Scene scene = SceneManager.GetSceneByPath(path);
                bool wasOpen = scene.IsValid() && scene.isLoaded;
                if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    Theme(scene);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene, path))
                        throw new IOException("Could not save " + path);
                    changed.Add(name);
                }
                finally
                {
                    if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Learning with Journey",
                "Menu styling saved for: " + string.Join(", ", changed) +
                ".\n\nOriginal scene copies: " + backup +
                (missing.Count > 0 ? "\n\nNot present (not rebuilt): " + string.Join(", ", missing) : "") +
                "\n\nTest each screen in Play mode. MainMenu and the black loading screen were not edited.", "OK");
        }

        [MenuItem("Learning with Journey/Match Current Screen to Menu V1")]
        public static void ApplyCurrent()
        {
            if (!Ready()) return;
            Scene scene = SceneManager.GetActiveScene();
            if (Array.IndexOf(Scenes, scene.name) < 0 || scene.path != Folder + scene.name + ".unity")
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Open a Library, BookReader, ParentZone, RewardsRoom or game scene first.", "OK");
                return;
            }
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Match screen to menu");
            foreach (GameObject root in scene.GetRootGameObjects())
                Undo.RegisterFullObjectHierarchyUndo(root, "Match screen to menu");
            Theme(scene);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Learning with Journey", "Menu styling applied. Preview the screen, then Ctrl+S to save. Undo restores the previous styling.", "OK");
        }

        static bool Ready()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode first.", "OK");
                return false;
            }
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            if (rounded == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey",
                    "The menu's RoundedPanel sprite is missing at " + RoundedPath +
                    ". No scenes were changed. Use the project containing your approved Main Menu artwork.", "OK");
                return false;
            }
            return true;
        }

        static void Theme(Scene scene)
        {
            var images = new List<Image>();
            foreach (GameObject root in scene.GetRootGameObjects())
                images.AddRange(root.GetComponentsInChildren<Image>(true));

            foreach (Image image in images)
            {
                string n = image.name;
                bool canvasChild = image.transform.parent != null &&
                    image.transform.parent.GetComponent<Canvas>() != null;

                if (canvasChild && Is(n, "Background", "LibraryWall", "ReaderWall", "Wall", "MatchSky", "MatchSkyTop"))
                    image.color = Pink;
                else if (canvasChild && Is(n, "LibraryFloor", "ReaderFloor", "Floor", "MatchFloor"))
                    image.color = Hex("A34219");
                else if (canvasChild && Is(n, "TopGlow", "BottomGlow", "LibraryWallGlow", "ReaderGlow", "WallGlow", "MatchGlow", "MatchFloorGlow"))
                    image.color = new Color(1f, .85f, .94f, .06f);
                else if (Is(n, "BottomNav", "LibrarySelectionPanel", "ABCActivityCard", "CountingActivityCard", "MatchActivityCard"))
                    Panel(image, Purple, true);
                else if (Is(n, "ReaderRug", "LibraryRug", "Rug", "ReadingArch", "ReadingNook"))
                    Panel(image, Hex("8E44D4"), false);
                else if (Is(n, "ProfileArea", "ProfileCard", "ParentTools", "ABCCard", "CountingCard", "MatchCard", "GateCard", "PrivacyNote"))
                    Panel(image, Hex("FFF7FC"), false);
                else if (Is(n, "OpenBook", "ArtworkPanel", "ArtInnerCard"))
                    Panel(image, Hex("FFF9F1"), false);
                else if (Is(n, "StarPill", "StarsPill", "PointsPill"))
                    Panel(image, Hex("F02F8E"), true);
                else if (Is(n, "CoinPill", "CoinsPill"))
                    Panel(image, Hex("7025B8"), true);
                else if (n == "LevelPill")
                    Panel(image, image.color, true);
                else if (Is(n, "ProgressHeading", "Ribbon", "LibraryRibbon", "TitleRibbon", "Header", "PagePill") ||
                         n.StartsWith("Journey") && n.EndsWith("Bubble") || n == "Bubble")
                    Panel(image, image.color, false);

                // Explicit controls only: do not reskin match cards, letter artwork,
                // input fields, progress fills, or other runtime-painted images.
                Button button = image.GetComponent<Button>();
                if (button != null && Is(n, "Home", "Library", "Rewards", "Parents",
                    "PreviousPage", "ReadAgain", "NextPage", "BackToLibrary", "BackButton",
                    "EditName", "SaveName", "ParentGate", "Reset", "OpenTreasureButton",
                    "AnswerA", "AnswerB", "AnswerC", "Six", "Seven", "Eight", "Close"))
                {
                    Color color = image.color;
                    if (n == "Home" || n == "ReadAgain") color = Hex("F33D98");
                    if (n == "Library" || n == "NextPage") color = Hex("20BFE7");
                    if (n == "Rewards") color = Hex("FFB52C");
                    if (n == "Parents" || n == "PreviousPage") color = Hex("8E44D4");
                    Panel(image, color, true);
                    Gloss(image);
                    // Existing sibling shadows need the same rounded silhouette.
                    Transform shadow = image.transform.parent.Find(n + "Shadow");
                    if (shadow != null && shadow.TryGetComponent<Image>(out var shadowImage))
                    {
                        shadowImage.sprite = rounded;
                        shadowImage.type = Image.Type.Sliced;
                    }
                }
            }

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    bool topTitle = text.transform.parent != null &&
                        text.transform.parent.GetComponent<Canvas>() != null &&
                        Is(text.name, "Title", "BookTitle", "LibraryTitle", "RewardsTitle", "ABCTitle", "CountingTitle", "MatchTitle");
                    if (topTitle)
                    {
                        text.color = Color.white;
                        text.fontStyle |= FontStyles.Bold;
                    }
                }
        }

        static void Panel(Image image, Color color, bool raised)
        {
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            // Disable obsolete outline stacks, retaining components for Undo.
            foreach (Shadow effect in image.GetComponents<Shadow>()) effect.enabled = false;
            if (raised)
            {
                Outline outline = image.GetComponent<Outline>();
                if (outline == null) outline = Undo.AddComponent<Outline>(image.gameObject);
                outline.enabled = true;
                outline.effectColor = Color.white;
                outline.effectDistance = new Vector2(3f, -3f);
                outline.useGraphicAlpha = true;
                Shadow shadow = null;
                foreach (Shadow effect in image.GetComponents<Shadow>())
                    if (!(effect is Outline)) { shadow = effect; break; }
                if (shadow == null) shadow = Undo.AddComponent<Shadow>(image.gameObject);
                shadow.enabled = true;
                shadow.effectColor = new Color(.16f, .02f, .25f, .45f);
                shadow.effectDistance = new Vector2(0f, -7f);
            }
        }

        static void Gloss(Image image)
        {
            // Reuse existing gloss, avoiding stacked overlays on repeated runs.
            Transform old = image.transform.Find("MenuThemeGloss");
            if (old == null) old = image.transform.Find("Gloss");
            Image gloss = old != null ? old.GetComponent<Image>() : null;
            if (old != null && gloss == null) return;
            if (gloss == null)
            {
                var go = new GameObject("MenuThemeGloss", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(image.transform, false);
                Undo.RegisterCreatedObjectUndo(go, "Menu button gloss");
                gloss = go.GetComponent<Image>();
            }
            gloss.sprite = rounded;
            gloss.type = Image.Type.Sliced;
            gloss.color = new Color(1f, 1f, 1f, .22f);
            gloss.raycastTarget = false;
            RectTransform rect = gloss.rectTransform;
            rect.anchorMin = new Vector2(.04f, .64f);
            rect.anchorMax = new Vector2(.96f, .93f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();
        }

        static bool Is(string name, params string[] names) => Array.IndexOf(names, name) >= 0;
        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }
    }
}
#endif
