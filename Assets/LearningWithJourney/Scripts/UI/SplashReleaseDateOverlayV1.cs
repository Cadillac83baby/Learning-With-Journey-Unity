using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Adds the original release date beneath the existing Powered by credit
    /// on the loading screen without changing the Splash scene asset.
    /// </summary>
    public sealed class SplashReleaseDateOverlayV1 : MonoBehaviour
    {
        const string HostName = "LearningWithJourneyReleaseDateOverlay";
        const string DateText = "Est: Sept 16, 2026";

        static SplashReleaseDateOverlayV1 instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeHost()
        {
            if (FindFirstObjectByType<SplashReleaseDateOverlayV1>() != null) return;
            var host = new GameObject(HostName);
            DontDestroyOnLoad(host);
            host.AddComponent<SplashReleaseDateOverlayV1>();
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Splash") return;
            AddDateLabel();
        }

        void AddDateLabel()
        {
            TMP_Text[] labels = FindObjectsOfType<TMP_Text>(true);
            TMP_Text poweredBy = null;
            foreach (TMP_Text label in labels)
            {
                if (label != null && label.text != null &&
                    label.text.IndexOf("Powered by:", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    poweredBy = label;
                    break;
                }
            }

            if (poweredBy == null) return;
            Transform parent = poweredBy.transform.parent;
            if (parent == null || parent.Find("ReleaseDate") != null) return;

            var dateObject = new GameObject("ReleaseDate", typeof(RectTransform), typeof(TextMeshProUGUI));
            dateObject.transform.SetParent(parent, false);
            var dateRect = dateObject.GetComponent<RectTransform>();
            dateRect.anchorMin = new Vector2(.08f, .042f);
            dateRect.anchorMax = new Vector2(.92f, .074f);
            dateRect.offsetMin = Vector2.zero;
            dateRect.offsetMax = Vector2.zero;
            dateRect.localScale = Vector3.one;

            var dateLabel = dateObject.GetComponent<TextMeshProUGUI>();
            dateLabel.text = DateText;
            dateLabel.font = poweredBy.font;
            dateLabel.fontSharedMaterial = poweredBy.fontSharedMaterial;
            dateLabel.fontSize = Mathf.Clamp(poweredBy.fontSize * .8f, 12f, 18f);
            dateLabel.fontWeight = poweredBy.fontWeight;
            dateLabel.color = poweredBy.color;
            dateLabel.alignment = TextAlignmentOptions.Center;
            dateLabel.enableAutoSizing = true;
            dateLabel.fontSizeMin = 10f;
            dateLabel.fontSizeMax = 18f;
            dateLabel.raycastTarget = false;
        }
    }
}
