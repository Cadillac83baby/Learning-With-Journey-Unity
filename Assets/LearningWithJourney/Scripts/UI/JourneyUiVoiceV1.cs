using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Shared Journey voice cues for the menu and child-facing setup screens.
    /// Clips are resolved from Resources so the same voice remains consistent
    /// across scene changes and the child's name is never recorded or uploaded.
    /// </summary>
    public sealed class JourneyUiVoiceV1 : MonoBehaviour
    {
        static JourneyUiVoiceV1 instance;
        static AudioSource persistentSource;
        AudioSource source;

        const string MenuWelcome = "JourneyVoice/UI/MENU_welcome";
        const string MenuChoose = "JourneyVoice/UI/Menu_choose";
        const string MenuCounting = "JourneyVoice/UI/Menu_Counting";
        const string MenuLetters = "JourneyVoice/UI/Menu_letters";
        const string MenuMatching = "JourneyVoice/UI/Menu_Matching";
        const string NameAsk = "JourneyVoice/UI/Name_ask";
        const string NameHelp = "JourneyVoice/UI/Name_help";
        const string NameReady = "JourneyVoice/UI/Name_ready";
        const string NavHome = "JourneyVoice/UI/Nav_home";
        const string ParentWelcome = "JourneyVoice/UI/Parent_welcome";

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            EnsurePersistentSource();
            EnsureAudioListener();
            source = persistentSource;
        }

        void Start()
        {
            HookButton("Counting", PlayMenuCounting);
            HookButton("ABC", PlayMenuLetters);
            HookButton("Match", PlayMenuMatching);
        }

        void HookButton(string objectName, UnityEngine.Events.UnityAction action)
        {
            GameObject target = GameObject.Find(objectName);
            Button button = target != null ? target.GetComponent<Button>() : null;
            if (button != null) button.onClick.AddListener(action);
        }

        public void PlayMenuWelcome() => PlayPath(MenuWelcome);
        public void PlayMenuChoose() => PlayPath(MenuChoose);
        public void PlayMenuCounting() => PlayPath(MenuCounting);
        public void PlayMenuLetters() => PlayPath(MenuLetters);
        public void PlayMenuMatching() => PlayPath(MenuMatching);
        public void PlayNameAsk() => PlayPath(NameAsk);
        public void PlayNameHelp() => PlayPath(NameHelp);
        public void PlayNameReady() => PlayPath(NameReady);
        public void PlayNavHome() => PlayPath(NavHome);
        public void PlayParentWelcome() => PlayPath(ParentWelcome);

        public static void PlayPath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath)) return;
            EnsureInstance().PlayResource(resourcePath);
        }

        static JourneyUiVoiceV1 EnsureInstance()
        {
            if (instance != null) return instance;
            GameObject host = new GameObject("JourneyUiVoice");
            return host.AddComponent<JourneyUiVoiceV1>();
        }

        static void EnsurePersistentSource()
        {
            if (persistentSource != null) return;

            GameObject host = new GameObject("JourneyUiVoiceAudio");
            Object.DontDestroyOnLoad(host);
            persistentSource = host.AddComponent<AudioSource>();
            persistentSource.playOnAwake = false;
            persistentSource.loop = false;
            persistentSource.spatialBlend = 0f;
            persistentSource.volume = .92f;
        }

        static void EnsureAudioListener()
        {
            // Unity should have exactly one enabled listener. If a generated or
            // test scene is missing one, attach it to the active main camera so
            // Journey's voice is audible without requiring manual scene repair.
            AudioListener existing = Object.FindFirstObjectByType<AudioListener>();
            if (existing != null) return;

            Camera camera = Camera.main;
            if (camera == null) return;

            camera.gameObject.AddComponent<AudioListener>();
        }

        void PlayResource(string resourcePath)
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                Debug.Log($"Journey UI voice clip not installed: {resourcePath}");
                return;
            }

            source.Stop();
            source.clip = clip;
            source.Play();
        }
    }
}
