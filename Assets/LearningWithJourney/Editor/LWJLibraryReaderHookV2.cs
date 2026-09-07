#if UNITY_EDITOR
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJLibraryReaderHookV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/Library.unity";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Connect Library to Book Reader V2")]
        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Library.unity was not found. Build Library V1 first.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/LearningWithJourney/Scenes/BookReader.unity") == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "BookReader.unity was not found. Run Build Book Reader V1 first.", "OK");
                return;
            }

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = Object.FindFirstObjectByType<LibraryScreenControllerV1>(FindObjectsInactive.Include);
            var panelGo = GameObject.Find("LibrarySelectionPanel");

            if (controller == null || panelGo == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Library controller or selection panel was not found.", "OK");
                return;
            }

            var old = panelGo.transform.Find("OpenBookButton");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var oldShadow = panelGo.transform.Find("OpenBookButtonShadow");
            if (oldShadow != null) Object.DestroyImmediate(oldShadow.gameObject);

            var message = panelGo.transform.Find("SelectionMessage") as RectTransform;
            if (message != null)
            {
                message.anchorMin = new Vector2(.07f, .34f);
                message.anchorMax = new Vector2(.93f, .61f);
                message.offsetMin = message.offsetMax = Vector2.zero;
                var text = message.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.fontSize = 18f;
                    text.fontSizeMin = 13f;
                    text.fontSizeMax = 19f;
                }
            }

            var button = CreateButton(panelGo.transform, "OpenBookButton", "OPEN BOOK", new Vector2(.19f, .07f), new Vector2(.81f, .30f), Hex("EF4B9F"), Hex("A32170"), 21f);
            UnityEventTools.AddPersistentListener(button.onClick, controller.OpenSelectedBook);

            var so = new SerializedObject(controller);
            var prop = so.FindProperty("openBookButton");
            if (prop != null) prop.objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Library V2 is connected to the book reader. Choose ABC, Numbers, Colors + Shapes, or Story Time, then tap OPEN BOOK. The approved Library layout remains intact.",
                "OK");
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color top, Color shadowColor, float fontSize)
        {
            var shadow = CreateImage(parent, name + "Shadow", min + new Vector2(0f, -.015f), max + new Vector2(0f, -.015f), shadowColor);
            shadow.raycastTarget = false;
            var image = CreateImage(parent, name, min, max, top);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, .95f);
            colors.pressedColor = new Color(.90f, .90f, .90f, 1f);
            colors.disabledColor = new Color(.60f, .60f, .60f, .65f);
            button.colors = colors;

            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(image.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(.04f, .05f);
            rect.anchorMax = new Vector2(.96f, .95f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14f;
            text.fontSizeMax = fontSize;
            text.raycastTarget = false;
            return button;
        }

        static Image CreateImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
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
            return image;
        }

        static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;
        }
    }
}
#endif
