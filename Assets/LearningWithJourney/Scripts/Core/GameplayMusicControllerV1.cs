using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Core
{
    /// <summary>
    /// Persistent gameplay bed. It starts quietly when NameSetup loads, fades
    /// in once, and survives scene changes without taking over Journey's voice
    /// AudioSources.
    /// </summary>
    public sealed class GameplayMusicControllerV1 : MonoBehaviour
    {
        const string HostName = "LearningWithJourneyGameplayMusic";
        const string MusicResource = "JourneyVoice/Music/LearningWithJourney_GameplayMusic";

        static GameplayMusicControllerV1 instance;

        // The music file is normalized near -18 LUFS. Keep it modestly below
        // narration so Journey's prompts remain the focus.
        [SerializeField, Range(0f, 1f)] float musicVolume = .35f;
        [SerializeField, Min(0.1f)] float fadeInSeconds = 2.5f;

        AudioSource musicSource;
        AudioClip musicClip;
        AudioListener fallbackListener;
        Coroutine fadeRoutine;
        Coroutine loadRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeHost()
        {
            if (FindFirstObjectByType<GameplayMusicControllerV1>() != null) return;
            var host = new GameObject(HostName);
            host.AddComponent<GameplayMusicControllerV1>();
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

            musicSource = GetComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.mute = false;
            musicSource.volume = 0f;
            musicClip = Resources.Load<AudioClip>(MusicResource);

            if (musicClip == null)
                Debug.LogWarning("[LearningWithJourney] Gameplay music was not found at Resources/" + MusicResource + ".mp3");
            else
                Debug.Log("[LearningWithJourney] Gameplay music loaded (" + musicClip.length.ToString("0.0") + "s).\n");
        }

        void Start()
        {
            // Calling this from Start as well as sceneLoaded covers Unity
            // editor tests where the first scene is already active when the
            // runtime host is created.
            EnsureAudioListener();
            if (IsMusicScene(SceneManager.GetActiveScene()))
                RequestStart();
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void Update()
        {
            // A scene can finish loading before sceneLoaded is subscribed in
            // an editor test or during a fast Splash -> MainMenu transition.
            // This one-frame-safe check makes MainMenu startup deterministic.
            if (!IsMusicScene(SceneManager.GetActiveScene())) return;
            if (musicClip == null)
            {
                RequestStart();
                return;
            }

            if (musicSource != null && !musicSource.isPlaying && fadeRoutine == null)
            {
                EnsureAudioListener();
                StartAndFadeIn();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();
            if (IsMusicScene(scene))
                RequestStart();
        }

        static bool IsMusicScene(Scene scene)
        {
            // Splash and AccessGate stay quiet. Starting on any later scene
            // also makes direct editor testing reliable, while normal play
            // still fades in from NameSetup.
            return scene.IsValid() && scene.name != "Splash" && scene.name != "AccessGate";
        }

        void EnsureAudioListener()
        {
            var listeners = FindObjectsOfType<AudioListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] == null || !listeners[i].enabled) continue;
                if (listeners[i] != fallbackListener)
                {
                    if (fallbackListener != null) fallbackListener.enabled = false;
                    return;
                }
            }

            var camera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            if (camera != null)
            {
                fallbackListener = camera.GetComponent<AudioListener>();
                if (fallbackListener == null)
                    fallbackListener = camera.gameObject.AddComponent<AudioListener>();
                fallbackListener.enabled = true;
                Debug.Log("[LearningWithJourney] Added AudioListener to " + camera.name + ".");
            }
            else if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
                Debug.Log("[LearningWithJourney] Added fallback AudioListener to gameplay music host.");
            }
        }

        void StartAndFadeIn()
        {
            if (musicSource == null || musicClip == null) return;
            if (!musicSource.isPlaying)
            {
                musicSource.clip = musicClip;
                musicSource.volume = 0f;
                musicSource.Play();
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeToVolume());
        }

        void RequestStart()
        {
            if (musicClip != null)
            {
                StartAndFadeIn();
                return;
            }

            // A freshly copied MP3 can still be importing when Play is
            // pressed. Retry briefly instead of giving up during Awake.
            if (loadRoutine == null)
                loadRoutine = StartCoroutine(LoadClipAndStart());
        }

        IEnumerator LoadClipAndStart()
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                musicClip = Resources.Load<AudioClip>(MusicResource);
                if (musicClip != null)
                {
                    Debug.Log("[LearningWithJourney] Gameplay music loaded after import (" + musicClip.length.ToString("0.0") + "s).");
                    loadRoutine = null;
                    StartAndFadeIn();
                    yield break;
                }

                yield return new WaitForSecondsRealtime(.25f);
            }

            loadRoutine = null;
            Debug.LogWarning("[LearningWithJourney] Gameplay music is still missing. Confirm the MP3 is at Resources/JourneyVoice/Music/LearningWithJourney_GameplayMusic.mp3.");
        }

        IEnumerator FadeToVolume()
        {
            float start = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeInSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(start, musicVolume, elapsed / fadeInSeconds);
                yield return null;
            }
            musicSource.volume = musicVolume;
            fadeRoutine = null;
        }
    }
}
