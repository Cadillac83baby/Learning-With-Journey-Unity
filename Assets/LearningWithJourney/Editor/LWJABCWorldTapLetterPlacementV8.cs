#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldTapLetterPlacementV8
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";

        [MenuItem("Learning with Journey/Fix ABC Tap Letter Inside Oval V8")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "ABCWorld.unity was not found.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var tapLetter = FindText("TapLetterHint");

            if (tapLetter == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "TapLetterHint was not found. Apply the ABC picture/voice pass first.",
                    "OK");
                return;
            }

            // Keep the label fully inside the lower portion of the letter oval.
            // The previous anchors sat in the transparent bottom edge of the circle sprite,
            // which made the text appear below the visible oval.
            var rect = tapLetter.rectTransform;
            rect.anchorMin = new Vector2(.10f, .115f);
            rect.anchorMax = new Vector2(.90f, .255f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            tapLetter.text = "TAP LETTER";
            tapLetter.alignment = TextAlignmentOptions.Center;
            tapLetter.fontStyle = FontStyles.Bold;
            tapLetter.enableAutoSizing = true;
            tapLetter.fontSize = 19f;
            tapLetter.fontSizeMin = 16f;
            tapLetter.fontSizeMax = 20f;
            tapLetter.textWrappingMode = TextWrappingModes.NoWrap;
            tapLetter.margin = Vector4.zero;
            tapLetter.color = Hex("5A2D91");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V8 applied. TAP LETTER now sits completely inside the letter oval. No other ABC layout elements were moved.",
                "OK");
        }

        static TMP_Text FindText(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                var direct = go.GetComponent<TMP_Text>();
                if (direct != null) return direct;
            }

            foreach (var text in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (text == null || text.gameObject == null) continue;
                if (text.gameObject.scene.path != ScenePath) continue;
                if (text.gameObject.name == objectName) return text;
            }

            return null;
        }

        static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
                return color;
            return Color.white;
        }
    }
}
#endif
