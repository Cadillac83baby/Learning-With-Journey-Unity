using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Core
{
    /// <summary>
    /// Persistent scene-aware background music. The existing fallback bed is
    /// kept on MainMenu only. Dedicated beds are used for NameSetup, ABCWorld,
    /// AlphabetMatchWorld, Library, BookReader, ParentZone, and RewardsRoom;
    /// all other scenes remain quiet until assigned a track.
    /// </summary>
    public sealed class GameplayMusicControllerV1 : MonoBehaviour
    {
        const string HostName = "LearningWithJourneyGameplayMusic";
        const string FallbackResource = "JourneyVoice/Music/LearningWithJourney_GameplayMusic";
        const string MusicFolder = "JourneyVoice/Music/";

        static GameplayMusicControllerV1 instance;

        [SerializeField, Range(0f, 1f)] float musicVolume = .35f;
        [SerializeField, Min(0.1f)] float fadeInSeconds = 2.5f;

        AudioSource musicSource;
        AudioListener fallbackListener;
        Coroutine fadeRoutine;
        Coroutine loadRoutine;
        string activeResource;
        string requestedResource;

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
            musicSource.priority = 200;
            musicSource.mute = false;
            musicSource.enabled = true;
            musicSource.bypassEffects = true;
            musicSource.bypassListenerEffects = true;
            musicSource.bypassReverbZones = true;
            musicSource.volume = 0f;
        }

        void Start()
        {
            EnsureAudioListener();
            RequestTrack(SceneManager.GetActiveScene());
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        void OnActiveSceneChanged(Scene from, Scene to)
        {
            // Stop before the new scene begins so no previous bed bleeds into
            // the transition or plays over an unassigned scene.
            StopCurrentTrack();
        }

        void Update()
        {
            var scene = SceneManager.GetActiveScene();
            string resource = ResourceForScene(scene.name);
            if (resource == null)
            {
                // Do not let a track from the previous scene bleed into an
                // unassigned scene.
                if (musicSource != null && (musicSource.isPlaying || activeResource != null || loadRoutine != null))
                    StopCurrentTrack();
                return;
            }

            if (resource != activeResource || (musicSource != null && !musicSource.isPlaying && loadRoutine == null))
                RequestTrack(scene);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();
            // Every scene owns its own bed. Stop the previous one before
            // loading the new scene's track, including a MainMenu reload.
            StopCurrentTrack();
            RequestTrack(scene);
        }

        static string ResourceForScene(string sceneName)
        {
            // Use exact scene names first, then tolerate a clone/suffix from
            // an async scene loader. This keeps BookReader's story bed from
            // being missed when the scene is opened by a generated flow.
            if (string.Equals(sceneName, "MainMenu", System.StringComparison.OrdinalIgnoreCase))
                return FallbackResource;
            if (sceneName.StartsWith("NameSetup", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Name_Setup_Back_ground_Music";
            if (sceneName.StartsWith("ABCWorld", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "ABC_World_BackgroundMusic";
            if (sceneName.StartsWith("CountingWorld", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.StartsWith("Counting", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Counting_Back_Ground_Music";
            if (sceneName.StartsWith("AlphabetMatchWorld", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.StartsWith("AlphabetMatch", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "ABC_Match_background_Music";
            if (sceneName.StartsWith("Library", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Story_time_Background_Music";
            if (sceneName.StartsWith("BookReader", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Story_time_Background_Music";
            if (sceneName.StartsWith("ParentZone", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.StartsWith("Parent", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Parent_Zone_Background_Music";
            if (sceneName.StartsWith("RewardsRoom", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.StartsWith("Rewards", System.StringComparison.OrdinalIgnoreCase))
                return MusicFolder + "Rewards_Room_Background_Music";
            return null;
        }

        void RequestTrack(Scene scene)
        {
            string resource = ResourceForScene(scene.name);
            if (resource == null) return;
            if (resource == activeResource && musicSource != null && musicSource.isPlaying) return;
            if (resource == requestedResource && loadRoutine != null) return;

            if (activeResource != null && activeResource != resource)
                StopCurrentTrack();

            requestedResource = resource;
            if (loadRoutine != null) StopCoroutine(loadRoutine);
            loadRoutine = StartCoroutine(LoadAndSwitch(resource));
        }

        IEnumerator LoadAndSwitch(string resource)
        {
            AudioClip clip = null;
            for (int attempt = 0; attempt < 40; attempt++)
            {
                clip = Resources.Load<AudioClip>(resource);
                if (clip != null) break;
                yield return new WaitForSecondsRealtime(.25f);
            }

            loadRoutine = null;
            if (clip == null)
            {
                Debug.LogWarning("[LearningWithJourney] Music track missing at Resources/" + resource + ".mp3");
                yield break;
            }

            // A slower import must not restart audio for a scene that has
            // already been replaced.
            if (requestedResource != resource || ResourceForScene(SceneManager.GetActiveScene().name) != resource)
                yield break;

            if (clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();

            // Larger MP3s can still be decoding when Resources.Load returns.
            // Wait for the clip to become playable instead of calling Play on
            // an unloaded clip (which is silent on some Unity 6 imports).
            float waitStarted = Time.realtimeSinceStartup;
            while (clip.loadState == AudioDataLoadState.Loading &&
                   Time.realtimeSinceStartup - waitStarted < 8f)
                yield return new WaitForSecondsRealtime(.05f);

            if (clip.loadState == AudioDataLoadState.Failed)
            {
                Debug.LogWarning("[LearningWithJourney] Music clip failed to decode: Resources/" + resource + ".mp3");
                yield break;
            }

            activeResource = resource;
            Debug.Log("[LearningWithJourney] Scene music loaded: " + resource + " (" + clip.length.ToString("0.0") + "s).");

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(StartTrack(clip));
        }

        void StopCurrentTrack()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
            if (loadRoutine != null)
            {
                StopCoroutine(loadRoutine);
                loadRoutine = null;
            }
            if (musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = null;
                musicSource.volume = 0f;
            }
            activeResource = null;
            requestedResource = null;
        }

        IEnumerator StartTrack(AudioClip clip)
        {
            if (musicSource == null) yield break;
            musicSource.Stop();
            musicSource.enabled = true;
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            Debug.Log("[LearningWithJourney] Scene music started: " + clip.name + ".");

            float elapsed = 0f;
            while (elapsed < fadeInSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeInSeconds);
                yield return null;
            }

            musicSource.volume = musicVolume;
            fadeRoutine = null;
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
            }
            else if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
            }
        }
    }
}
