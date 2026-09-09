using System.Collections;
using LearningWithJourney.Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Core
{
    public class SceneRouter : MonoBehaviour
    {
        static bool mainMenuLoadRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeNavigationBridge()
        {
            if (GameObject.Find("JourneyNavigationRuntime") != null) return;

            GameObject host = new GameObject("JourneyNavigationRuntime");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<RuntimeNavigationBridge>();
        }

        public void OpenMainMenu() => LoadMainMenu();
        public void OpenCounting() => LoadAfterMenuCue("CountingWorld", "JourneyVoice/UI/Menu_Counting");
        public void OpenABC() => LoadAfterMenuCue("ABCWorld", "JourneyVoice/UI/Menu_letters");
        public void OpenAlphabetMatch() => LoadAfterMenuCue("AlphabetMatchWorld", "JourneyVoice/UI/Menu_Matching");
        public void OpenRewards() => Load("RewardsRoom");
        public void OpenLibrary() => Load("Library");
        public void OpenParentZone() => Load("ParentZone");

        AudioSource menuCueSource;
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

            menuCueSource = GetComponent<AudioSource>();
            if (menuCueSource == null)
                menuCueSource = gameObject.AddComponent<AudioSource>();

            menuCueSource.playOnAwake = false;
            menuCueSource.loop = false;
            menuCueSource.spatialBlend = 0f;
            menuCueSource.volume = .92f;
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
                // Give the cue to Journey when the character is present. This
                // stops the return-menu line cleanly, displays the matching
                // caption, and plays through the same known-good voice source.
                JourneyMainMenuCharacter journey = Object.FindFirstObjectByType<JourneyMainMenuCharacter>();
                if (journey != null && journey.CanPlayVoice)
                {
                    journey.Speak(cue, CaptionForCue(cueResourcePath));
                }
                else
                {
                    // Fallback for a menu scene without the Journey character.
                    menuCueSource.Stop();
                    menuCueSource.clip = cue;
                    menuCueSource.Play();
                }

                // Leave a small tail so the final consonant is not clipped.
                yield return new WaitForSecondsRealtime(cue.length + .12f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(.15f);
            }

            Load(sceneName);
        }

        static string CaptionForCue(string cueResourcePath)
        {
            if (cueResourcePath == "JourneyVoice/UI/Menu_Counting")
                return "Let’s count together!";
            if (cueResourcePath == "JourneyVoice/UI/Menu_letters")
                return "Let’s learn letters!";
            if (cueResourcePath == "JourneyVoice/UI/Menu_Matching")
                return "Let’s play Alphabet Match!";
            return string.Empty;
        }

        public void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            SceneManager.LoadScene(sceneName);
        }

        sealed class RuntimeNavigationBridge : MonoBehaviour
        {
            void OnEnable()
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                WireBackButton();
                WireMainMenuButtons();
            }

            void OnDisable()
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                WireBackButton();
                WireMainMenuButtons();
            }

            static void WireBackButton()
            {
                GameObject backObject = GameObject.Find("BackButton");
                if (backObject == null) return;

                Button button = backObject.GetComponent<Button>();
                if (button == null) return;

                button.onClick.RemoveListener(ReturnToMainMenu);
                button.onClick.AddListener(ReturnToMainMenu);
            }

            static void ReturnToMainMenu()
            {
                LoadMainMenu();
            }

            static void WireMainMenuButtons()
            {
                WireGameButton("Counting", OpenCountingFallback);
                WireGameButton("ABC", OpenABCFallback);
                WireGameButton("Match", OpenMatchFallback);
            }

            static void WireGameButton(string objectName, UnityEngine.Events.UnityAction callback)
            {
                if (SceneManager.GetActiveScene().name != "MainMenu") return;

                GameObject target = GameObject.Find(objectName);
                if (target == null) return;

                Button button = target.GetComponent<Button>();
                if (button == null) return;

                button.onClick.RemoveListener(callback);
                button.onClick.AddListener(callback);
            }

            static SceneRouter FindRouter()
            {
                SceneRouter router = Object.FindFirstObjectByType<SceneRouter>();
                if (router != null) return router;

                GameObject host = new GameObject("SceneRouterRuntime");
                return host.AddComponent<SceneRouter>();
            }

            static void OpenCountingFallback() => FindRouter().OpenCounting();
            static void OpenABCFallback() => FindRouter().OpenABC();
            static void OpenMatchFallback() => FindRouter().OpenAlphabetMatch();
        }
    }
}
