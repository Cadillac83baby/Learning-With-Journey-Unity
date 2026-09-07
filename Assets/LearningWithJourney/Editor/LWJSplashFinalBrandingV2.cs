#if UNITY_EDITOR
using System;
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
    /// <summary>
    /// Compile-safe Unity 6 installer for the final Learning with Journey splash.
    /// Uses pre-made transparent logo and mastered DSHMENT loading tag from Downloads.
    /// </summary>
    public static class LWJSplashFinalBrandingV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/Splash.unity";
        const string BrandDirectory = "Assets/LearningWithJourney/Art/Brand";
        const string TransparentLogoPath = BrandDirectory + "/LearningWithJourneyLoadingLogo_Transparent.png";
        const string OriginalLogoPath = BrandDirectory + "/LearningWithJourneyLoadingLogo.jpg";
        const string AudioDirectory = "Assets/LearningWithJourney/Audio/Brand";
        const string AudioPath = AudioDirectory + "/DSHMENT_Loading_Tag_GAME_MASTER.wav";

        [MenuItem("Learning with Journey/Install Final Loading Screen Logo + Audio")]
        public static void InstallFinalAssets()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string logoSource = Path.Combine(downloads, "LWJ_Loading_Logo_TRANSPARENT.png");
            string audioSource = Path.Combine(downloads, "DSHMENT_Loading_Tag_GAME_MASTER.wav");

            if (!File.Exists(logoSource))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "LWJ_Loading_Logo_TRANSPARENT.png was not found in Downloads. Download the transparent loading logo first.",
                    "OK");
                return;
            }

            if (!File.Exists(audioSource))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "DSHMENT_Loading_Tag_GAME_MASTER.wav was not found in Downloads. Download the mastered loading audio first.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(BrandDirectory);
            Directory.CreateDirectory(AudioDirectory);
            File.Copy(logoSource, TransparentLogoPath, true);
            File.Copy(audioSource, AudioPath, true);
            AssetDatabase.Refresh();

            var textureImporter = AssetImporter.GetAtPath(TransparentLogoPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.SaveAndReimport();
            }

            var audioImporter = AssetImporter.GetAtPath(AudioPath) as AudioImporter;
            if (audioImporter != null)
            {
                AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = .82f;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData = true;
                audioImporter.defaultSampleSettings = settings;
                audioImporter.forceToMono = false;
                audioImporter.loadInBackground = false;
                audioImporter.SaveAndReimport();
            }

            ApplyFinalBlackSplash();
        }

        [MenuItem("Learning with Journey/Apply Final Black Loading Screen")]
        public static void ApplyFinalBlackSplash()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Splash.unity does not exist yet. Run Build Access + Splash Flow V1 first.",
                    "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
            }

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Splash scene has no Canvas.", "OK");
                return;
            }

            Image background = GetOrCreateImage(canvas.transform, "Background");
            SetRect(background.rectTransform, Vector2.zero, Vector2.one);
            background.color = Color.black;
            background.raycastTarget = false;
            background.transform.SetAsFirstSibling();

            Image glow = GetOrCreateImage(canvas.transform, "BrandGlow");
            SetRect(glow.rectTransform, new Vector2(.09f, .22f), new Vector2(.91f, .82f));
            glow.color = new Color(.25f, .03f, .36f, .35f);
            glow.raycastTarget = false;
            glow.transform.SetSiblingIndex(Mathf.Min(1, glow.transform.parent.childCount - 1));

            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TransparentLogoPath);
            if (logoTexture == null)
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(OriginalLogoPath);

            RawImage logo = GetOrCreateRawImage(canvas.transform, "LearningWithJourneyLogo");
            SetRect(logo.rectTransform, new Vector2(.07f, .26f), new Vector2(.93f, .81f));
            logo.texture = logoTexture;
            logo.color = Color.white;
            logo.raycastTarget = false;

            AspectRatioFitter fitter = logo.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = logo.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            if (logoTexture != null && logoTexture.height > 0)
                fitter.aspectRatio = (float)logoTexture.width / logoTexture.height;

            TMP_Text powered = GetOrCreateText(canvas.transform, "PoweredBy");
            ConfigureText(powered, "POWERED BY", 22f, FontStyles.Bold, new Color(.78f, .74f, .83f, 1f), new Vector2(.25f, .17f), new Vector2(.75f, .205f));

            TMP_Text company = GetOrCreateText(canvas.transform, "CompanyCredit");
            ConfigureText(company, "DOWN $OUTH HU$TLA MU$IC ENT", 31f, FontStyles.Bold, new Color(1f, .70f, .12f, 1f), new Vector2(.06f, .115f), new Vector2(.94f, .17f));
            company.enableAutoSizing = true;
            company.fontSizeMin = 20f;
            company.fontSizeMax = 31f;

            TMP_Text loading = GetOrCreateText(canvas.transform, "Loading");
            ConfigureText(loading, "LOADING...", 22f, FontStyles.Bold, new Color(.87f, .84f, .91f, 1f), new Vector2(.30f, .055f), new Vector2(.70f, .09f));

            SplashControllerV1 controller = UnityEngine.Object.FindFirstObjectByType<SplashControllerV1>();
            if (controller == null)
            {
                var go = new GameObject("SplashController");
                controller = go.AddComponent<SplashControllerV1>();
            }

            AudioSource player = controller.GetComponent<AudioSource>();
            if (player == null) player = controller.gameObject.AddComponent<AudioSource>();
            player.playOnAwake = false;
            player.loop = false;
            player.spatialBlend = 0f;
            player.volume = .92f;
            player.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);

            SerializedObject controllerSO = new SerializedObject(controller);
            SerializedProperty audioProp = controllerSO.FindProperty("startupAudioSource");
            if (audioProp != null) audioProp.objectReferenceValue = player;
            SerializedProperty minimumProp = controllerSO.FindProperty("minimumDisplaySeconds");
            if (minimumProp != null) minimumProp.floatValue = 2.0f;
            controllerSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                player.clip != null
                    ? "Final black loading screen applied. The DSHMENT tag will play once during loading."
                    : "Black loading screen applied. Install the mastered audio to enable the one-time DSHMENT tag.",
                "OK");
        }

        static Image GetOrCreateImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                Image found = existing.GetComponent<Image>();
                if (found != null) return found;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        static RawImage GetOrCreateRawImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                RawImage found = existing.GetComponent<RawImage>();
                if (found != null) return found;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RawImage>();
        }

        static TMP_Text GetOrCreateText(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                TMP_Text found = existing.GetComponent<TMP_Text>();
                if (found != null) return found;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            return go.GetComponent<TextMeshProUGUI>();
        }

        static void ConfigureText(TMP_Text text, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            SetRect(text.rectTransform, min, max);
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
        }

        static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
#endif
