using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Games
{
    /// <summary>
    /// Assigns the shared Word_A-Z Resources clips to the speech components in
    /// ABCWorld and AlphabetMatchWorld at runtime. This deliberately leaves
    /// the scene YAML and the original recordings untouched.
    /// </summary>
    public sealed class WordAudioSceneBridgeV1 : MonoBehaviour
    {
        const string HostName = "LearningWithJourneyWordAudioBridge";
        const string LetterResourceFolder = "JourneyVoice/ABC/Letter_";
        const string WordResourceFolder = "JourneyVoice/ABC/Word_";
        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static WordAudioSceneBridgeV1 instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeHost()
        {
            if (FindFirstObjectByType<WordAudioSceneBridgeV1>() != null) return;
            var host = new GameObject(HostName);
            DontDestroyOnLoad(host);
            host.AddComponent<WordAudioSceneBridgeV1>();
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

        void Start() => WireActiveScene();

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => WireScene(scene);

        void WireActiveScene() => WireScene(SceneManager.GetActiveScene());

        void WireScene(Scene scene)
        {
            if (!scene.IsValid()) return;
            EnsureAudioListener();

            if (scene.name == "ABCWorld")
            {
                var controller = FindFirstObjectByType<ABCWorldPlayControllerV1>();
                if (controller != null)
                    WireSpeech(controller, "journeySpeech", typeof(JourneyABCSpeech), "ABCWorld");
            }
            else if (scene.name == "AlphabetMatchWorld")
            {
                var controller = FindFirstObjectByType<AlphabetMatchWorldPlayControllerV1>();
                if (controller != null)
                    WireSpeech(controller, "journeySpeech", typeof(JourneyAlphabetMatchSpeech), "AlphabetMatchWorld");
            }
        }

        static void WireSpeech(Component controller, string speechFieldName, Type speechType, string sceneName)
        {
            var controllerType = controller.GetType();
            var field = controllerType.GetField(
                speechFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Component speech = field != null ? field.GetValue(controller) as Component : null;
            if (speech == null)
            {
                speech = controller.GetComponent(speechType);
                if (speech == null) speech = controller.gameObject.AddComponent(speechType);
                if (field != null && field.FieldType.IsInstanceOfType(speech))
                    field.SetValue(controller, speech);
            }

            if (speech == null)
            {
                Debug.LogWarning("[LearningWithJourney] Could not find speech component for " + sceneName + ".");
                return;
            }

            var source = speech.GetComponent<AudioSource>();
            if (source == null) source = speech.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = .92f;
            SetField(speech, "audioSource", source);

            int loadedLetters = AssignClips(speech, "letterClips", LetterResourceFolder);
            int loadedWords = AssignClips(speech, "wordClips", WordResourceFolder);
            Debug.Log("[LearningWithJourney] " + sceneName + " Letter A-X audio wired: " + loadedLetters + "/26 clips.");
            Debug.Log("[LearningWithJourney] " + sceneName + " Word A-Z audio wired: " + loadedWords + "/26 clips.");
        }

        static int AssignClips(Component speech, string fieldName, string resourceFolder)
        {
            var field = speech.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || !field.FieldType.IsArray || field.FieldType.GetElementType() != typeof(AudioClip))
                return 0;

            var clips = new AudioClip[Alphabet.Length];
            int loaded = 0;
            for (int i = 0; i < Alphabet.Length; i++)
            {
                clips[i] = Resources.Load<AudioClip>(resourceFolder + Alphabet[i]);
                if (clips[i] == null) continue;
                loaded++;
                if (clips[i].loadState == AudioDataLoadState.Unloaded)
                    clips[i].LoadAudioData();
            }

            field.SetValue(speech, clips);
            if (loaded < Alphabet.Length)
                Debug.LogWarning("[LearningWithJourney] Missing " + (Alphabet.Length - loaded) + " clips for " + fieldName + " in Resources/JourneyVoice/ABC.");
            return loaded;
        }

        static void SetField(Component target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType.IsInstanceOfType(value))
                field.SetValue(target, value);
        }

        static void EnsureAudioListener()
        {
            if (FindFirstObjectByType<AudioListener>() != null) return;
            var camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (camera != null)
                camera.gameObject.AddComponent<AudioListener>();
        }
    }
}
