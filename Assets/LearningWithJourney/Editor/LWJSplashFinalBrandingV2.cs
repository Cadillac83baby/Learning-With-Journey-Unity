#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    public static class LWJSplashFinalBrandingV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/Splash.unity";
        const string SourceLogoPath = "Assets/LearningWithJourney/Art/Brand/LearningWithJourneyLoadingLogo.jpg";
        const string TransparentLogoPath = "Assets/LearningWithJourney/Art/Brand/LearningWithJourneyLoadingLogo_Transparent.png";
        const string AudioDirectory = "Assets/LearningWithJourney/Audio/Brand";
        const string AudioPath = AudioDirectory + "/DSHMENT_Loading_Tag_GAME_MASTER.wav";

        [MenuItem("Learning with Journey/Install Final DSHMENT Splash Audio")]
        public static void InstallAudioFromDownloads()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string source = Path.Combine(downloads, "DSHMENT_Loading_Tag_GAME_MASTER.wav");

            if (!File.Exists(source))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "DSHMENT_Loading_Tag_GAME_MASTER.wav was not found in your Downloads folder. Download the mastered WAV first, then run this command again.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(AudioDirectory);
            File.Copy(source, AudioPath, true);
            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(AudioPath) as AudioImporter;
            if (importer != null)
            {
                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.preloadAudioData = true;
                importer.defaultSampleSettings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.DecompressOnLoad,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = .82f,
                    sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
                };
                importer.SaveAndReimport();
            }

            ApplyFinalSplash();
        }

        [MenuItem("Learning with Journey/Apply Final Black Splash V2")]
        public static void ApplyFinalSplash()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Splash.unity does not exist yet. Build the Access + Splash flow first.", "OK");
                return;
            }

            Texture2D transparentLogo = BuildTransparentEdgeLogo();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The Splash scene does not contain a Canvas.", "OK");
                return;
            }

            Image background = FindOrCreateImage(canvas.transform, "Background");
            Stretch(background.rectTransform, Vector2.zero, Vector2.one);
            background.color = Color.black;
            background.raycastTarget = false;
            background.transform.SetAsFirstSibling();

            Image glow = FindOrCreateImage(canvas.transform, "BrandGlow");
            Stretch(glow.rectTransform, new Vector2(.10f, .18f), new Vector2(.90f, .82f));
            glow.color = Hex("3D0B57", .38f);
            glow.raycastTarget = false;
            glow.transform.SetSiblingIndex(Mathf.Min(1, glow.transform.parent.childCount - 1));

            RawImage logo = FindOrCreateRawImage(canvas.transform, "LearningWithJourneyLogo");
            Stretch(logo.rectTransform, new Vector2(.075f, .235f), new Vector2(.925f, .805f));
            logo.texture = transparentLogo != null ? transparentLogo : AssetDatabase.LoadAssetAtPath<Texture2D>(SourceLogoPath);
            logo.color = Color.white;
            logo.raycastTarget = false;
            EnsureAspectFit(logo.gameObject, logo.texture);

            TMP_Text loading = FindOrCreateText(canvas.transform, "Loading");
            ConfigureText(loading, "LOADING...", 22f, FontStyles.Bold, Hex("D5CCE0"), new Vector2(.32f, .055f), new Vector2(.68f, .09f));

            TMP_Text poweredBy = FindOrCreateText(canvas.transform, "PoweredBy");
            ConfigureText(poweredBy, "POWERED BY", 20f, FontStyles.Bold, Hex("B9AEC5"), new Vector2(.30f, .155f), new Vector2(.70f, .19f));

            TMP_Text company = FindOrCreateText(canvas.transform, "CompanyCredit");
            ConfigureText(company, "DOWN $OUTH HU$TLA MU$IC ENT", 28f, FontStyles.Bold, Hex("F0B42A"), new Vector2(.08f, .105f), new Vector2(.92f, .155f));
            company.enableAutoSizing = true;
            company.fontSizeMin = 20f;
            company.fontSizeMax = 28f;

            SplashControllerV1 controller = UnityEngine.Object.FindFirstObjectByType<SplashControllerV1>();
            if (controller == null)
            {
                var controllerGo = new GameObject("SplashController");
                controller = controllerGo.AddComponent<SplashControllerV1>();
            }

            AudioSource source = controller.GetComponent<AudioSource>();
            if (source == null) source = controller.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = .92f;
            source.priority = 96;
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);

            SerializedObject controllerSO = new SerializedObject(controller);
            SerializedProperty audioProp = controllerSO.FindProperty("startupAudioSource");
            if (audioProp != null) audioProp.objectReferenceValue = source;
            SerializedProperty minimumProp = controllerSO.FindProperty("minimumDisplaySeconds");
            if (minimumProp != null) minimumProp.floatValue = 2.0f;
            controllerSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string audioStatus = source.clip != null
                ? "The D$HM tag is connected and will play once before the next scene."
                : "The splash visuals are ready, but the mastered D$HM WAV is not installed yet. Run Install Final DSHMENT Splash Audio after downloading it.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Final black loading screen applied.\n\n" + audioStatus + "\n\nCredit: POWERED BY DOWN $OUTH HU$TLA MU$IC ENT",
                "OK");
        }

        static Texture2D BuildTransparentEdgeLogo()
        {
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceLogoPath);
            if (source == null) return null;

            var importer = AssetImporter.GetAtPath(SourceLogoPath) as TextureImporter;
            bool restoreReadable = false;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                restoreReadable = true;
                source = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceLogoPath);
            }

            int width = source.width;
            int height = source.height;
            Color32[] pixels = source.GetPixels32();
            bool[] outside = new bool[pixels.Length];
            var queue = new Queue<int>();

            Action<int> trySeed = index =>
            {
                if (index < 0 || index >= pixels.Length || outside[index]) return;
                if (!IsBackgroundWhite(pixels[index])) return;
                outside[index] = true;
                queue.Enqueue(index);
            };

            for (int x = 0; x < width; x++)
            {
                trySeed(x);
                trySeed((height - 1) * width + x);
            }
            for (int y = 0; y < height; y++)
            {
                trySeed(y * width);
                trySeed(y * width + width - 1);
            }

            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int x = i % width;
                int y = i / width;
                TryVisit(x - 1, y, width, height, pixels, outside, queue);
                TryVisit(x + 1, y, width, height, pixels, outside, queue);
                TryVisit(x, y - 1, width, height, pixels, outside, queue);
                TryVisit(x, y + 1, width, height, pixels, outside, queue);
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                if (outside[i]) pixels[i].a = 0;
            }

            var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            output.SetPixels32(pixels);
            output.Apply(false, false);

            Directory.CreateDirectory(Path.GetDirectoryName(TransparentLogoPath));
            File.WriteAllBytes(TransparentLogoPath, output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            AssetDatabase.Refresh();

            var outputImporter = AssetImporter.GetAtPath(TransparentLogoPath) as TextureImporter;
            if (outputImporter != null)
            {
                outputImporter.textureType = TextureImporterType.Default;
                outputImporter.alphaIsTransparency = true;
                outputImporter.mipmapEnabled = false;
                outputImporter.SaveAndReimport();
            }

            if (restoreReadable && importer != null)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(TransparentLogoPath);
        }

        static bool IsBackgroundWhite(Color32 c)
        {
            int max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            int min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return min >= 238 && max - min <= 18;
        }

        static void TryVisit(int x, int y, int width, int height, Color32[] pixels, bool[] outside, Queue<int> queue)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            int index = y * width + x;
            if (outside[index] || !IsBackgroundWhite(pixels[index])) return;
            outside[index] = true;
            queue.Enqueue(index);
        }

        static Image FindOrCreateImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent(out Image image)) return image;
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        static RawImage FindOrCreateRawImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent(out RawImage raw)) return raw;
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RawImage>();
        }

        static TMP_Text FindOrCreateText(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent(out TMP_Text text)) return text;
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            return go.GetComponent<TextMeshProUGUI>();
        }

        static void ConfigureText(TMP_Text text, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            RectTransform rect = text.rectTransform;
            Stretch(rect, min, max);
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
        }

        static void EnsureAspectFit(GameObject target, Texture texture)
        {
            var fitter = target.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = target.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            if (texture != null && texture.height > 0)
                fitter.aspectRatio = (float)texture.width / texture.height;
        }

        static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                color.a = alpha;
                return color;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
#endif
