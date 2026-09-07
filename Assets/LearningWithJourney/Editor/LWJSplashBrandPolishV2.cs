#if UNITY_EDITOR
using System;
using System.IO;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJSplashBrandPolishV2
    {
        const string Root = "Assets/LearningWithJourney";
        const string SplashScene = Root + "/Scenes/Splash.unity";
        const string LogoPath = Root + "/Art/Brand/LearningWithJourneyLoadingLogo.jpg";
        const string AudioPath = Root + "/Audio/Brand/DSHMENT_Loading_Tag_GAME_MASTER.wav";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Install D$HM Loading Audio From Downloads")]
        public static void InstallAudioFromDownloads()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string[] candidates =
            {
                Path.Combine(downloads, "DSHMENT_Loading_Tag_GAME_MASTER.wav"),
                Path.Combine(downloads, "DSHMENT_Loading_Tag_GAME_MASTER (1).wav"),
                Path.Combine(downloads, "DSHMENT_Loading_Tag_GAME_MASTER(1).wav")
            };

            string source = null;
            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    source = candidate;
                    break;
                }
            }

            if (string.IsNullOrEmpty(source))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The mastered D$HM loading audio was not found in Downloads. Download DSHMENT_Loading_Tag_GAME_MASTER.wav first, then run this menu again.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(AudioPath));
            File.Copy(source, AudioPath, true);
            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(AudioPath) as AudioImporter;
            if (importer != null)
            {
                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.preloadAudioData = true;

                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "D$HM game-mastered loading audio installed. Now run Apply Black Branded Loading Screen V2.",
                "OK");
        }

        [MenuItem("Learning with Journey/Apply Black Branded Loading Screen V2")]
        public static void Apply()
        {
            if (!File.Exists(SplashScene))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Splash.unity does not exist yet. Run Build Access + Splash Flow V1 first.",
                    "OK");
                return;
            }

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Scene scene = EditorSceneManager.OpenScene(SplashScene, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
                UnityEngine.Object.DestroyImmediate(root);

            BuildCamera();
            var canvas = BuildCanvas();
            CreateImage(canvas.transform, "BlackBackground", Vector2.zero, Vector2.one, Color.black);

            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
            if (logo != null)
            {
                var glow = CreateImage(canvas.transform, "LogoGlow", new Vector2(.08f, .245f), new Vector2(.92f, .805f), new Color(.45f, .08f, .55f, .32f));
                AddOutline(glow.gameObject, new Color(1f, .18f, .62f, .65f), new Vector2(5f, -5f));
                glow.raycastTarget = false;

                var logoGo = new GameObject("LearningWithJourneyLogo", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
                logoGo.transform.SetParent(canvas.transform, false);
                var rect = (RectTransform)logoGo.transform;
                rect.anchorMin = new Vector2(.10f, .27f);
                rect.anchorMax = new Vector2(.90f, .79f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;

                var raw = logoGo.GetComponent<RawImage>();
                raw.texture = logo;
                raw.color = Color.white;
                raw.raycastTarget = false;

                var fitter = logoGo.GetComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = (float)logo.width / logo.height;
            }
            else
            {
                var placeholder = CreateText(
                    canvas.transform,
                    "LogoMissing",
                    "LEARNING\nWITH\nJOURNEY",
                    72f,
                    FontStyles.Bold,
                    Color.white,
                    new Vector2(.12f, .34f),
                    new Vector2(.88f, .68f));
                placeholder.alignment = TextAlignmentOptions.Center;
            }

            var loading = CreateText(
                canvas.transform,
                "Loading",
                "LOADING...",
                24f,
                FontStyles.Bold,
                new Color(1f, .30f, .68f, 1f),
                new Vector2(.28f, .18f),
                new Vector2(.72f, .225f));
            loading.alignment = TextAlignmentOptions.Center;

            var powered = CreateText(
                canvas.transform,
                "PoweredBy",
                "POWERED BY",
                18f,
                FontStyles.Bold,
                new Color(.82f, .72f, 1f, 1f),
                new Vector2(.25f, .115f),
                new Vector2(.75f, .155f));
            powered.alignment = TextAlignmentOptions.Center;

            var company = CreateText(
                canvas.transform,
                "Company",
                "DOWN $OUTH HU$TLA MU$IC ENT",
                25f,
                FontStyles.Bold,
                Color.white,
                new Vector2(.08f, .065f),
                new Vector2(.92f, .115f));
            company.alignment = TextAlignmentOptions.Center;
            company.enableAutoSizing = true;
            company.fontSizeMin = 18f;
            company.fontSizeMax = 27f;

            var controllerGo = new GameObject("SplashController");
            var audioSource = controllerGo.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 0f;
            audioSource.ignoreListenerPause = true;

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
            if (clip != null) audioSource.clip = clip;

            var controller = controllerGo.AddComponent<SplashControllerV2>();
            var so = new SerializedObject(controller);
            so.FindProperty("loadingAudioSource").objectReferenceValue = audioSource;
            so.FindProperty("minimumDisplaySeconds").floatValue = 2f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SplashScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string audioState = clip != null
                ? "The mastered D$HM tag is connected and will play once during each loading screen."
                : "The loading screen was updated, but the mastered audio is not installed yet. Run Install D$HM Loading Audio From Downloads.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Black branded loading screen applied. Powered by Down $outh Hu$tla Mu$ic Ent has been added. " + audioState,
                "OK");
        }

        static void BuildCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";
        }

        static GameObject BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            return canvasGo;
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

        static TMP_Text CreateText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
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
            text.alignment = TextAlignmentOptions.Center;
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
    }
}
#endif
