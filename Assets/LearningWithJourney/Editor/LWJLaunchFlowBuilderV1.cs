#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using LearningWithJourney.Core;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJLaunchFlowBuilderV1
    {
        const string Root = "Assets/LearningWithJourney";
        const string AccessScene = Root + "/Scenes/AccessGate.unity";
        const string SplashScene = Root + "/Scenes/Splash.unity";
        const string LogoPath = Root + "/Art/Brand/LearningWithJourneyLoadingLogo.jpg";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Install Loading Logo From Downloads")]
        public static void InstallLogoFromDownloads()
        {
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string[] candidates =
            {
                Path.Combine(downloads, "LWJ_Loading_Logo_768.jpg"),
                Path.Combine(downloads, "LWJ_Loading_Logo.png"),
                Path.Combine(downloads, "ChatGPT Image Jul 13, 2026, 06_27_05 PM(2).png")
            };

            string source = null;
            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate)) { source = candidate; break; }
            }

            if (string.IsNullOrEmpty(source))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Logo file was not found in Downloads. Download the loading-logo file first, then run this menu again.", "OK");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(LogoPath));
            byte[] bytes = File.ReadAllBytes(source);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, bytes, false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                EditorUtility.DisplayDialog("Learning with Journey", "The logo image could not be read.", "OK");
                return;
            }

            File.WriteAllBytes(LogoPath, texture.EncodeToJPG(92));
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(LogoPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = false;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog("Learning with Journey", "Loading logo installed. Now run Build Access + Splash Flow V1.", "OK");
        }

        [MenuItem("Learning with Journey/Build Access + Splash Flow V1")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Scenes");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            BuildAccessGate();
            BuildSplash();
            SetLaunchBuildOrder();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Launch flow ready: Access Gate -> Logo Loading Screen -> Name Setup (first time) or Main Menu (returning player). The 3-day trial works locally now; production purchase verification remains connected through the store hook for final Google Play / App Store integration.",
                "OK");
        }

        [MenuItem("Learning with Journey/Reset Trial + Purchase For Testing")]
        public static void ResetAccessForTesting()
        {
            PlayerPrefs.DeleteKey("LWJ_ACCESS_TRIAL_START_UTC_V1");
            PlayerPrefs.DeleteKey("LWJ_ACCESS_PURCHASED_V1");
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Learning with Journey", "Trial and purchase test state cleared.", "OK");
        }

        static void BuildAccessGate()
        {
            Scene scene = File.Exists(AccessScene)
                ? EditorSceneManager.OpenScene(AccessScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);

            var services = new GameObject("Services");
            services.AddComponent<AppAccessServiceV1>();

            BuildCamera();
            var canvas = BuildCanvas();
            CreateImage(canvas.transform, "Background", Vector2.zero, Vector2.one, Hex("FAE7F2"));
            CreateImage(canvas.transform, "TopGlow", new Vector2(0f, .55f), Vector2.one, Hex("FFF7DA", .45f));

            var title = CreateText(canvas.transform, "Title", "LEARNING WITH JOURNEY", 56f, FontStyles.Bold, Hex("59269B"), new Vector2(.08f, .80f), new Vector2(.92f, .90f));
            title.alignment = TextAlignmentOptions.Center;
            var sub = CreateText(canvas.transform, "Subtitle", "LEARN  •  GROW  •  SHINE", 23f, FontStyles.Bold, Hex("E63F98"), new Vector2(.15f, .755f), new Vector2(.85f, .80f));
            sub.alignment = TextAlignmentOptions.Center;

            var card = CreatePanel(canvas.transform, "AccessCard", new Vector2(.08f, .25f), new Vector2(.92f, .735f), Color.white);
            AddShadow(card.gameObject, new Vector2(0f, -10f), Hex("6C4D75", .20f));
            AddOutline(card.gameObject, Hex("C99BE4"), new Vector2(4f, -4f));

            var h = CreateText(card.transform, "Heading", "CHOOSE HOW TO START", 31f, FontStyles.Bold, Hex("5C2F9D"), new Vector2(.08f, .82f), new Vector2(.92f, .95f));
            h.alignment = TextAlignmentOptions.Center;

            var trialBox = CreatePanel(card.transform, "TrialBox", new Vector2(.06f, .46f), new Vector2(.94f, .79f), Hex("FFF4FA"));
            AddOutline(trialBox.gameObject, Hex("F04AA4"), new Vector2(3f, -3f));
            var trialTitle = CreateText(trialBox.transform, "Title", "3-DAY FREE TRIAL", 30f, FontStyles.Bold, Hex("E93E96"), new Vector2(.06f, .60f), new Vector2(.94f, .90f));
            var trialText = CreateText(trialBox.transform, "Detail", "Try the full learning game free for 3 days.", 20f, FontStyles.Normal, Hex("624B69"), new Vector2(.08f, .35f), new Vector2(.92f, .60f));
            trialText.textWrappingMode = TextWrappingModes.Normal;
            var trialButton = CreateButton(trialBox.transform, "StartTrial", "START FREE TRIAL", new Vector2(.20f, .08f), new Vector2(.80f, .33f), Hex("F04AA4"), Hex("A5236D"), 22f);

            var purchaseBox = CreatePanel(card.transform, "PurchaseBox", new Vector2(.06f, .12f), new Vector2(.94f, .43f), Hex("F7F2FF"));
            AddOutline(purchaseBox.gameObject, Hex("7541CF"), new Vector2(3f, -3f));
            var purchaseTitle = CreateText(purchaseBox.transform, "Title", "FULL GAME  •  $0.99", 30f, FontStyles.Bold, Hex("6331B0"), new Vector2(.06f, .62f), new Vector2(.94f, .90f));
            var purchaseText = CreateText(purchaseBox.transform, "Detail", "One purchase unlocks the full game on this store account.", 19f, FontStyles.Normal, Hex("624B69"), new Vector2(.08f, .38f), new Vector2(.92f, .62f));
            purchaseText.textWrappingMode = TextWrappingModes.Normal;
            var purchaseButton = CreateButton(purchaseBox.transform, "Purchase", "PURCHASE FULL GAME", new Vector2(.12f, .09f), new Vector2(.65f, .34f), Hex("6F3ACA"), Hex("42217D"), 20f);
            var restoreButton = CreateButton(purchaseBox.transform, "Restore", "RESTORE", new Vector2(.68f, .09f), new Vector2(.90f, .34f), Hex("2CB7C2"), Hex("177680"), 17f);

            var status = CreateText(canvas.transform, "Status", "Choose your access option to continue.", 18f, FontStyles.Bold, Hex("5D456A"), new Vector2(.10f, .17f), new Vector2(.90f, .23f));
            status.textWrappingMode = TextWrappingModes.Normal;
            var privacy = CreateText(canvas.transform, "Privacy", "Child profile information stays local on this device unless a parent chooses a future account feature.", 15f, FontStyles.Normal, Hex("745F7C"), new Vector2(.10f, .08f), new Vector2(.90f, .16f));
            privacy.textWrappingMode = TextWrappingModes.Normal;

            var controllerGo = new GameObject("AccessGateController");
            var controller = controllerGo.AddComponent<AccessGateControllerV1>();
            UnityEventTools.AddPersistentListener(trialButton.onClick, controller.StartThreeDayTrial);
            UnityEventTools.AddPersistentListener(purchaseButton.onClick, controller.PurchaseFullGame);
            UnityEventTools.AddPersistentListener(restoreButton.onClick, controller.RestorePurchases);

            var so = new SerializedObject(controller);
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("trialDetailText").objectReferenceValue = trialText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, AccessScene);
        }

        static void BuildSplash()
        {
            Scene scene = File.Exists(SplashScene)
                ? EditorSceneManager.OpenScene(SplashScene, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);

            BuildCamera();
            var canvas = BuildCanvas();
            CreateImage(canvas.transform, "Background", Vector2.zero, Vector2.one, Color.white);

            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
            if (logo != null)
            {
                var logoGo = new GameObject("LearningWithJourneyLogo", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
                logoGo.transform.SetParent(canvas.transform, false);
                var rect = (RectTransform)logoGo.transform;
                rect.anchorMin = new Vector2(.08f, .19f);
                rect.anchorMax = new Vector2(.92f, .83f);
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
                var placeholder = CreateText(canvas.transform, "LogoMissing", "LEARNING\nWITH\nJOURNEY", 70f, FontStyles.Bold, Hex("5D2BA4"), new Vector2(.12f, .30f), new Vector2(.88f, .70f));
                placeholder.alignment = TextAlignmentOptions.Center;
            }

            var loading = CreateText(canvas.transform, "Loading", "Loading...", 21f, FontStyles.Bold, Hex("8B66A0"), new Vector2(.30f, .09f), new Vector2(.70f, .15f));
            loading.alignment = TextAlignmentOptions.Center;

            var controllerGo = new GameObject("SplashController");
            controllerGo.AddComponent<SplashControllerV1>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SplashScene);
        }

        static void SetLaunchBuildOrder()
        {
            string[] preferred =
            {
                AccessScene,
                SplashScene,
                Root + "/Scenes/NameSetup.unity",
                Root + "/Scenes/MainMenu.unity",
                Root + "/Scenes/CountingWorld.unity",
                Root + "/Scenes/ABCWorld.unity",
                Root + "/Scenes/AlphabetMatchWorld.unity",
                Root + "/Scenes/RewardsRoom.unity",
                Root + "/Scenes/Library.unity",
                Root + "/Scenes/BookReader.unity",
                Root + "/Scenes/ParentZone.unity"
            };

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in preferred)
                if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));

            foreach (var existing in EditorBuildSettings.scenes)
            {
                bool already = scenes.Exists(s => s.path == existing.path);
                if (!already && File.Exists(existing.path)) scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void BuildCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.white;
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

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color) => CreateImage(parent, name, min, max, color);

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

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color top, Color shadowColor, float fontSize)
        {
            var shadow = CreateImage(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), shadowColor);
            shadow.raycastTarget = false;
            var image = CreateImage(parent, name, min, max, top);
            var button = image.gameObject.AddComponent<Button>();
            var text = CreateText(image.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, new Vector2(.04f, .07f), new Vector2(.96f, .93f));
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(11f, fontSize * .62f);
            text.fontSizeMax = fontSize;
            return button;
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void AddShadow(GameObject target, Vector2 distance, Color color)
        {
            var shadow = target.GetComponent<Shadow>();
            if (shadow == null) shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
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
