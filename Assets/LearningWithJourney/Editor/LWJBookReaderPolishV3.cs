#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    /// <summary>
    /// Non-destructive visual polish for the completed V2 Book Reader.
    /// Keeps the approved Library intact and only adjusts BookReader.unity.
    /// </summary>
    public static class LWJBookReaderPolishV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";

        [MenuItem("Learning with Journey/Polish Complete Book Reader V3")]
        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "BookReader.unity was not found. Run Build Book Reader V1 and Build Complete Book Reader V2 first.",
                    "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera camera = Camera.main;
            if (camera != null && Object.FindFirstObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            SetRect("BackToLibrary", new Vector2(.035f, .925f), new Vector2(.245f, .978f));
            SetRect("BookTitle", new Vector2(.245f, .915f), new Vector2(.755f, .982f));
            ConfigureText("BookTitle", 52f, 34f, 54f, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
            SetRect("PagePill", new Vector2(.755f, .927f), new Vector2(.965f, .974f));
            ConfigureText("PageNumber", 20f, 16f, 21f, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);

            SetRect("Journey", new Vector2(.02f, .205f), new Vector2(.345f, .525f));
            // Approved reader-specific bag position: covers the screen-right shorts/leg cleanly.
            SetRect("JourneyBackpack", new Vector2(.255f, .255f), new Vector2(.355f, .345f));
            GameObject backpack = Find("JourneyBackpack");
            if (backpack != null)
            {
                backpack.transform.localRotation = Quaternion.identity;
                backpack.transform.localScale = Vector3.one;
                GameObject journey = Find("Journey");
                if (journey != null)
                    backpack.transform.SetSiblingIndex(Mathf.Min(journey.transform.GetSiblingIndex() + 1, backpack.transform.parent.childCount - 1));
            }

            GameObject bubble = Find("JourneyReaderBubble");
            if (bubble != null)
            {
                SetRect(bubble.GetComponent<RectTransform>(), new Vector2(.025f, .545f), new Vector2(.405f, .675f));
                bubble.transform.SetAsLastSibling();
            }
            ConfigureText("Speech", 23f, 17f, 24f, TextAlignmentOptions.Center, TextWrappingModes.Normal);

            SetRect("BookShadow", new Vector2(.315f, .175f), new Vector2(.975f, .875f));
            SetRect("OpenBook", new Vector2(.295f, .19f), new Vector2(.965f, .885f));
            SetRect("ArtworkPanel", new Vector2(.075f, .47f), new Vector2(.925f, .915f));
            SetRect("PageArtwork", new Vector2(.06f, .05f), new Vector2(.94f, .95f));
            SetRect("PageHeading", new Vector2(.07f, .365f), new Vector2(.93f, .465f));
            ConfigureText("PageHeading", 42f, 28f, 44f, TextAlignmentOptions.Center, TextWrappingModes.Normal);
            SetRect("PageBody", new Vector2(.075f, .135f), new Vector2(.925f, .355f));
            ConfigureText("PageBody", 31f, 23f, 32f, TextAlignmentOptions.Center, TextWrappingModes.Normal);

            TMP_Text tip = FindComponent<TMP_Text>("ReadTip");
            if (tip != null)
            {
                SetRect(tip.rectTransform, new Vector2(.10f, .065f), new Vector2(.90f, .125f));
                tip.text = "Tap READ AGAIN to hear Journey read this page.";
                tip.fontSize = 18f;
                tip.enableAutoSizing = true;
                tip.fontSizeMin = 15f;
                tip.fontSizeMax = 19f;
                tip.textWrappingMode = TextWrappingModes.Normal;
                tip.alignment = TextAlignmentOptions.Center;
            }

            SetRect("PreviousPage", new Vector2(.31f, .075f), new Vector2(.525f, .155f));
            SetRect("ReadAgain", new Vector2(.535f, .075f), new Vector2(.745f, .155f));
            SetRect("NextPage", new Vector2(.755f, .075f), new Vector2(.97f, .155f));
            ConfigureButtonText("PreviousPage", 22f);
            ConfigureButtonText("ReadAgain", 22f);
            ConfigureButtonText("NextPage", 22f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Book Reader V3 polish applied. The speech bubble is fully visible, page text is larger, the book is wider, controls are cleaner, and the backpack is restored to its approved reader position.",
                "OK");
        }

        static void ConfigureButtonText(string buttonName, float maxSize)
        {
            GameObject button = Find(buttonName);
            if (button == null) return;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text == null) return;
            text.enableAutoSizing = true;
            text.fontSize = maxSize;
            text.fontSizeMin = 15f;
            text.fontSizeMax = maxSize;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        static void ConfigureText(string objectName, float size, float minSize, float maxSize,
            TextAlignmentOptions alignment, TextWrappingModes wrapping)
        {
            TMP_Text text = FindComponent<TMP_Text>(objectName);
            if (text == null) return;
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = minSize;
            text.fontSizeMax = maxSize;
            text.alignment = alignment;
            text.textWrappingMode = wrapping;
        }

        static void SetRect(string objectName, Vector2 min, Vector2 max)
        {
            GameObject go = Find(objectName);
            if (go == null) return;
            SetRect(go.GetComponent<RectTransform>(), min, max);
        }

        static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            if (rect == null) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        static T FindComponent<T>(string name) where T : Component
        {
            GameObject go = Find(name);
            return go != null ? go.GetComponent<T>() : null;
        }

        static GameObject Find(string name)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go.name == name) return go;
            }
            return null;
        }
    }
}
#endif
