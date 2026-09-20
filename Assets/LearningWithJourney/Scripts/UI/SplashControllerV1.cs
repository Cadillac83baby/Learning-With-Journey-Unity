using System.Collections;
using LearningWithJourney.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Branded loading screen shown after access is granted and before gameplay.
    /// Plays the startup brand audio once, waits until the clip finishes (or the
    /// minimum display time elapses), then routes first-time players to NameSetup
    /// and returning players to MainMenu.
    /// </summary>
    public class SplashControllerV1 : MonoBehaviour
    {
        [SerializeField] float minimumDisplaySeconds = 2.0f;
        [SerializeField] float audioTailPaddingSeconds = 0.12f;
        [SerializeField] AudioSource startupAudioSource;
        [SerializeField] string nameSetupScene = "NameSetup";
        [SerializeField] string mainMenuScene = "MainMenu";

        IEnumerator Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            float waitSeconds = Mathf.Max(.5f, minimumDisplaySeconds);

            if (startupAudioSource != null && startupAudioSource.clip != null)
            {
                startupAudioSource.loop = false;
                startupAudioSource.playOnAwake = false;
                startupAudioSource.Stop();
                startupAudioSource.time = 0f;
                startupAudioSource.Play();
            startupAudioSource.volume *= Mathf.Pow(10f, -1f / 20f);
                waitSeconds = Mathf.Max(waitSeconds, startupAudioSource.clip.length + Mathf.Max(0f, audioTailPaddingSeconds));
            }

            float fadeSeconds =
                startupAudioSource != null && startupAudioSource.clip != null
                    ? Mathf.Min(.75f, startupAudioSource.clip.length)
                    : 0f;

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, waitSeconds - fadeSeconds));

            if (fadeSeconds > 0f && startupAudioSource != null)
                yield return StartCoroutine(FadeOutStartupAudio(fadeSeconds));

            var service = GameProgressService.Instance;
            string next = service != null && service.HasPlayerName ? mainMenuScene : nameSetupScene;
            yield return StartCoroutine(ShowBrandInterstitial(next));
        }

        IEnumerator FadeOutStartupAudio(float seconds)
        {
            if (startupAudioSource == null) yield break;

            float startVolume = startupAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / seconds);
                float smoothProgress =
                    progress * progress * (3f - 2f * progress);

                startupAudioSource.volume =
                    Mathf.Lerp(startVolume, 0f, smoothProgress);
                yield return null;
            }

            startupAudioSource.volume = 0f;
            startupAudioSource.Stop();
        }

        IEnumerator ShowBrandInterstitial(string next)
        {
            HideLoadingCredits();

            GameObject root = new GameObject("BrandInterstitial");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5000;

            var scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode =
                UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = .5f;

            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            DontDestroyOnLoad(root);

            GameObject background = new GameObject(
                "BlackBackground",
                typeof(RectTransform),
                typeof(UnityEngine.UI.Image));

            background.transform.SetParent(canvas.transform, false);

            RectTransform backgroundRect =
                background.GetComponent<RectTransform>();

            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            background.GetComponent<UnityEngine.UI.Image>().color = Color.black;

            CreateBrandText(
                canvas.transform,
                "PoweredByText",
                "Powered by: Down $outh Hu$tla Music Ent",
                .54f,
                .66f,
                42f);

            CreateBrandText(
                canvas.transform,
                "EstablishedText",
                "Established: Sept 16, 2026",
                .39f,
                .49f,
                28f);

            yield return FadeCanvasGroup(canvasGroup, 1f, .45f);
            yield return new WaitForSecondsRealtime(6f);

            var transition = root.AddComponent<SplashTransitionOverlayV1>();
            transition.LoadSceneWithFade(next, canvasGroup);
        }


        IEnumerator FadeCanvasGroup(
            CanvasGroup group,
            float target,
            float seconds)
        {
            if (group == null) yield break;

            float start = group.alpha;
            float elapsed = 0f;

            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(
                    start,
                    target,
                    elapsed / seconds);
                yield return null;
            }

            group.alpha = target;
        }

        void HideLoadingCredits()
        {
            var labels =
                FindObjectsOfType<TMPro.TMP_Text>(true);

            foreach (var label in labels)
            {
                if (label == null) continue;

                string value = label.text ?? string.Empty;

                if (value.IndexOf(
                        "Powered by:",
                        System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf(
                        "Established:",
                        System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    label.gameObject.SetActive(false);
                }
            }
        }

        void CreateBrandText(
            Transform parent,
            string objectName,
            string value,
            float minY,
            float maxY,
            float size)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TMPro.TextMeshProUGUI));

            textObject.transform.SetParent(parent, false);

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(.08f, minY);
            rect.anchorMax = new Vector2(.92f, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var label =
                textObject.GetComponent<TMPro.TextMeshProUGUI>();

            label.text = value;
            label.color = Color.white;
            label.fontStyle = TMPro.FontStyles.Bold;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.fontSize = size;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = size;

            if (TMPro.TMP_Settings.defaultFontAsset != null)
                label.font = TMPro.TMP_Settings.defaultFontAsset;

            label.raycastTarget = false;
        }
    }
}

public sealed class SplashTransitionOverlayV1 : MonoBehaviour
{
    public void LoadSceneWithFade(
        string next,
        CanvasGroup group)
    {
        StartCoroutine(LoadRoutine(next, group));
    }

    IEnumerator LoadRoutine(
        string next,
        CanvasGroup group)
    {
        AsyncOperation pending =
            SceneManager.LoadSceneAsync(next);

        if (pending != null)
            yield return pending;

        yield return null;

        yield return FadeCanvasGroup(
            group,
            0f,
            .45f);

        Destroy(gameObject);
    }

    IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        float target,
        float seconds)
    {
        if (group == null) yield break;

        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(
                start,
                target,
                elapsed / seconds);
            yield return null;
        }

        group.alpha = target;
    }
}