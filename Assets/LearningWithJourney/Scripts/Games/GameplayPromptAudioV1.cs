using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Games
{
    // Provides the scene-opening prompts without interrupting letter/word/card voice.
    public sealed class GameplayPromptAudioV1 : MonoBehaviour
    {
        static GameplayPromptAudioV1 instance;
        AudioSource source;

        public static bool IsScenePromptPlaying =>
            instance != null && instance.source != null && instance.source.isPlaying;

        public static void StopScenePrompt()
        {
            if (instance != null && instance.source != null)
                instance.source.Stop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance != null) return;
            instance = new GameObject("JourneyGameplayPrompts").AddComponent<GameplayPromptAudioV1>();
            DontDestroyOnLoad(instance.gameObject);
        }

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            source.Stop();
            string path = scene.name switch
            {
                "ABCWorld" => "JourneyVoice/ABC/ABC_Intro",
                "AlphabetMatchWorld" => "JourneyVoice/MATCH/MATCH_cases",
                _ => null
            };
            if (string.IsNullOrEmpty(path) && scene.name.IndexOf("count", System.StringComparison.OrdinalIgnoreCase) >= 0)
                path = "JourneyVoice/COUNTING/COUNT_Intro";
            if (!string.IsNullOrEmpty(path))
            {
                AudioClip clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    source.clip = clip;
                    source.time = 0f;
                    source.Play();
                }
            }
        }
    }
}
