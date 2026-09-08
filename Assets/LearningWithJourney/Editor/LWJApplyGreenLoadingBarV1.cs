#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJApplyGreenLoadingBarV1
    {
        const string SplashPath = "Assets/LearningWithJourney/Scenes/Splash.unity";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Apply Green Loading Bar V1")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode first.", "OK");
                return;
            }
            if (!File.Exists(SplashPath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Splash.unity was not found. Run the loading-screen builder first.", "OK");
                return;
            }

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Scene scene = EditorSceneManager.OpenScene(SplashPath, OpenSceneMode.Single);
            Transform canvas = scene.GetRootGameObjects().Length > 0
                ? Find(scene, "Canvas")
                : null;
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Splash Canvas was not found. No changes made.", "OK");
                return;
            }

            Image track = GetOrCreateImage(canvas, "LoadingBarTrack");
            SetRect(track.rectTransform, new Vector2(.16f, .155f), new Vector2(.84f, .180f));
            track.sprite = rounded;
            track.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            track.color = Hex("17351F");
            track.raycastTarget = false;
            ClearEffects(track.gameObject);
            AddOutline(track.gameObject, Hex("8CF5A5", .72f), new Vector2(2f, -2f));

            Image fill = GetOrCreateImage(track.transform, "Fill");
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one);
            fill.sprite = rounded;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            fill.color = Hex("35E66E");
            fill.raycastTarget = false;
            ClearEffects(fill.gameObject);
            AddOutline(fill.gameObject, Hex("B8FFC7", .9f), new Vector2(1f, -1f));

            TMP_Text loading = Find(scene, "Loading")?.GetComponent<TMP_Text>();
            if (loading != null)
            {
                SetRect(loading.rectTransform, new Vector2(.28f, .19f), new Vector2(.72f, .225f));
                loading.text = "LOADING...";
                loading.color = Hex("D9FFE1");
                loading.alignment = TextAlignmentOptions.Center;
            }

            Transform controllerTransform = Find(scene, "SplashController");
            GameObject controllerObject = controllerTransform != null
                ? controllerTransform.gameObject
                : new GameObject("SplashController");
            SplashLoadingBarV1 bar = controllerObject.GetComponent<SplashLoadingBarV1>();
            if (bar == null) bar = Undo.AddComponent<SplashLoadingBarV1>(controllerObject);
            SerializedObject serialized = new SerializedObject(bar);
            serialized.FindProperty("progressFill").objectReferenceValue = fill;
            serialized.FindProperty("fillDurationSeconds").floatValue = 2.15f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SplashPath);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Learning with Journey",
                "Green loading bar added. It fills smoothly during startup and reaches 100% before the loading screen changes scenes. Run Play mode to test it with the DSHMENT audio.", "OK");
        }

        static Transform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (item.name == name) return item;
            return null;
        }

        static Image GetOrCreateImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                Image image = existing.GetComponent<Image>();
                if (image != null) return image;
            }
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void ClearEffects(GameObject go)
        {
            foreach (Shadow shadow in go.GetComponents<Shadow>()) Undo.DestroyObjectImmediate(shadow);
            foreach (Outline outline in go.GetComponents<Outline>()) Undo.DestroyObjectImmediate(outline);
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            Outline outline = Undo.AddComponent<Outline>(go);
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
