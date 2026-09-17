using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Lightweight UI sound host. It adds a short click to every Button in the
    /// active scene without replacing any existing onClick listeners. The
    /// BookReader's page-turn effect remains owned by JourneyVoicePlayerV2.
    /// </summary>
    public sealed class JourneyUiSfxV1 : MonoBehaviour
    {
        const string HostName = "LearningWithJourneyUiSfx";
        const string ClickResource = "JourneyVoice/Common/mouse_click";

        static JourneyUiSfxV1 instance;
        AudioSource source;
        AudioClip clickClip;
        Coroutine wireRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeHost()
        {
            if (FindFirstObjectByType<JourneyUiSfxV1>() != null) return;
            var host = new GameObject(HostName);
            host.AddComponent<JourneyUiSfxV1>();
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
            source = gameObject.GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.priority = 160;
            source.volume = .42f;
            clickClip = Resources.Load<AudioClip>(ClickResource);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            QueueWire();
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (wireRoutine != null) StopCoroutine(wireRoutine);
            wireRoutine = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => QueueWire();

        void QueueWire()
        {
            if (!isActiveAndEnabled) return;
            if (wireRoutine != null) StopCoroutine(wireRoutine);
            wireRoutine = StartCoroutine(WireAfterSceneBuild());
        }

        IEnumerator WireAfterSceneBuild()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(.1f);
            WireButtons();
            wireRoutine = null;
        }

        void WireButtons()
        {
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button == null || button.transform.IsChildOf(transform)) continue;
                button.onClick.RemoveListener(PlayClick);
                button.onClick.AddListener(PlayClick);
            }
        }

        void PlayClick()
        {
            if (source == null) return;
            if (clickClip == null) clickClip = Resources.Load<AudioClip>(ClickResource);
            if (clickClip != null) source.PlayOneShot(clickClip, .42f);
        }
    }
}
