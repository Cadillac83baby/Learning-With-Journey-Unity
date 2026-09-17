using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Games
{
    // Provides the scene-opening prompts without interrupting letter/word/card voice.
    public sealed class GameplayPromptAudioV1 : MonoBehaviour
    {
        static GameplayPromptAudioV1 instance;
        AudioSource source;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance != null) return;
            instance = new GameObject("JourneyGameplayPrompts").AddComponent<GameplayPromptAudioV1>();
            DontDestroyOnLoad(instance.gameObject);
        }

        void Awake() { source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0f; SceneManager.sceneLoaded += OnSceneLoaded; }
        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string path = scene.name switch
            {
                "ABCWorld" => "JourneyVoice/ABC/ABC_Intro",
                "AlphabetMatchWorld" => "JourneyVoice/MATCH/MATCH_cases",
                _ => null
            };
            if (!string.IsNullOrEmpty(path))
            {
                AudioClip clip = Resources.Load<AudioClip>(path);
                if (clip != null) source.PlayOneShot(clip);
            }
        }
    }
}
