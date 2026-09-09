using System.Collections;
using LearningWithJourney.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Core
{
    public class SceneRouter : MonoBehaviour
    {
        static bool mainMenuLoadRequested;

        public void OpenMainMenu() => LoadMainMenu();
        public void OpenCounting() => LoadAfterMenuCue("CountingWorld", "JourneyVoice/UI/Menu_Counting");
        public void OpenABC() => LoadAfterMenuCue("ABCWorld", "JourneyVoice/UI/Menu_letters");
        public void OpenAlphabetMatch() => LoadAfterMenuCue("AlphabetMatchWorld", "JourneyVoice/UI/Menu_Matching");
        public void OpenRewards() => Load("RewardsRoom");
        public void OpenLibrary() => Load("Library");
        public void OpenParentZone() => Load("ParentZone");

        public static void LoadMainMenu()
        {
            if (mainMenuLoadRequested) return;
            mainMenuLoadRequested = true;
            SceneManager.LoadScene("MainMenu");
        }

        void Awake()
        {
            // A newly loaded Main Menu starts a fresh return cycle. This is
            // intentionally scene-owned; serialized menu events must continue
            // to target the router saved in that scene.
            if (SceneManager.GetActiveScene().name == "MainMenu")
                mainMenuLoadRequested = false;
        }

        bool loading;

        public void LoadAfterMenuCue(string sceneName, string cueResourcePath)
        {
            if (loading || string.IsNullOrWhiteSpace(sceneName)) return;
            StartCoroutine(LoadAfterMenuCueRoutine(sceneName, cueResourcePath));
        }

        IEnumerator LoadAfterMenuCueRoutine(string sceneName, string cueResourcePath)
        {
            loading = true;
            AudioClip cue = string.IsNullOrWhiteSpace(cueResourcePath)
                ? null
                : Resources.Load<AudioClip>(cueResourcePath);

            if (cue != null)
            {
                JourneyUiVoiceV1.PlayPath(cueResourcePath);
                // Leave a small tail so the final consonant is not clipped.
                yield return new WaitForSecondsRealtime(cue.length + .12f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(.15f);
            }

            Load(sceneName);
        }

        public void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            SceneManager.LoadScene(sceneName);
        }
    }
}
