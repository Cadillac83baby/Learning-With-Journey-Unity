using System.Collections;
using LearningWithJourney.Character;
using LearningWithJourney.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Core
{
    public class SceneRouter : MonoBehaviour
    {
        static bool mainMenuLoadRequested;
        static bool gameTransitionRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InstallRuntimeNavigationBridge()
        {
            if (GameObject.Find("JourneyNavigationRuntime") != null) return;

            GameObject host = new GameObject("JourneyNavigationRuntime");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<RuntimeNavigationBridge>();
        }

        public void OpenMainMenu() => LoadMainMenu();
        public void OpenCounting()
        {
            Debug.Log("[LearningWithJourney] Counting tile clicked.");
            LoadAfterMenuCue("CountingWorld", "JourneyVoice/UI/Menu_Counting");
        }

        public void OpenABC()
        {
            Debug.Log("[LearningWithJourney] ABC tile clicked.");
            LoadAfterMenuCue("ABCWorld", "JourneyVoice/UI/Menu_letters");
        }

        public void OpenAlphabetMatch()
        {
            Debug.Log("[LearningWithJourney] Alphabet Match tile clicked.");
            LoadAfterMenuCue("AlphabetMatchWorld", "JourneyVoice/UI/Menu_Matching");
        }
        public void OpenRewards() => Load("RewardsRoom");
        public void OpenLibrary() => Load("Library");
        public void OpenParentZone() => Load("ParentZone");

        AudioSource menuCueSource;
        public static void LoadMainMenu()
        {
            if (mainMenuLoadRequested) return;

            // GameProgressService keeps its Systems object alive between
            // scenes, which also keeps this router alive. Clear the
            // scene-owned cue lock before returning so the next menu
            // selection can play its prompt.
            SceneRouter router = Object.FindFirstObjectByType<SceneRouter>();
            if (router != null) router.loading = false;

            mainMenuLoadRequested = true;
            gameTransitionRequested = false;
            SceneManager.LoadScene("MainMenu");
        }

        void Awake()
        {
            // A newly loaded Main Menu starts a fresh return cycle. This is
            // intentionally scene-owned; serialized menu events must continue
            // to target the router saved in that scene.
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                mainMenuLoadRequested = false;
                gameTransitionRequested = false;
            }

            EnsureAudioListener();

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
            // loading is scene-owned and is enough to prevent duplicate clicks
            // during one transition. Do not let a stale static flag from the
            // previous game block the first selection after returning home.
            if (loading || string.IsNullOrWhiteSpace(sceneName)) return;
            gameTransitionRequested = true;
            StartCoroutine(LoadAfterMenuCueRoutine(sceneName, cueResourcePath));
        }

        IEnumerator LoadAfterMenuCueRoutine(string sceneName, string cueResourcePath)
        {
            loading = true;
            EnsureAudioListener();
            AudioClip cue = string.IsNullOrWhiteSpace(cueResourcePath)
                ? null
                : Resources.Load<AudioClip>(cueResourcePath);

            if (cue != null)
            {
                // Begin loading immediately while the menu cue plays. Scene
                // activation stays paused until the cue is complete, so the
                // voice is never cut off but the expensive load is hidden
                // behind narration.
                AsyncOperation pendingLoad = SceneManager.LoadSceneAsync(sceneName);
                if (pendingLoad != null)
                    pendingLoad.allowSceneActivation = false;

                // Play through the persistent Journey voice host so the cue
                // cannot be cut off when the Main Menu scene is destroyed.
                JourneyUiVoiceV1.PlayPath(cueResourcePath);
                JourneyMainMenuCharacter journey = Object.FindFirstObjectByType<JourneyMainMenuCharacter>();
                journey?.ShowPrompt(CaptionForCue(cueResourcePath));
                Debug.Log($"[LearningWithJourney] Playing game prompt: {cueResourcePath}");

                // Leave a small tail so the final consonant is not clipped.
                yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, cue.length - 0.20f) + .04f);
                if (pendingLoad != null)
                {
                    pendingLoad.allowSceneActivation = true;
                    yield return pendingLoad;
                }
                loading = false;
                yield break;
            }
            else
            {
                Debug.LogWarning($"[LearningWithJourney] Missing game prompt clip: {cueResourcePath}");
                yield return new WaitForSecondsRealtime(.15f);
            }

            Load(sceneName);
        }

        static void EnsureAudioListener()
        {
            if (Object.FindFirstObjectByType<AudioListener>() != null) return;

            Camera activeCamera = Camera.main;
            if (activeCamera == null) return;

            activeCamera.gameObject.AddComponent<AudioListener>();
            Debug.Log("[LearningWithJourney] Added AudioListener to the active Main Camera.");
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
                WireMainMenuNavigationButtons();
                StartCoroutine(WireAfterSceneBuild());
            }

            void OnDisable()
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name == "MainMenu")
                {
                    mainMenuLoadRequested = false;
                    gameTransitionRequested = false;

                    // The persistent Systems object owns the router, so its
                    // instance Awake does not run again on a return to the
                    // Main Menu. Explicitly reset the cue lock here.
                    SceneRouter router = Object.FindFirstObjectByType<SceneRouter>();
                    if (router != null) router.loading = false;
                }

                WireBackButton();
                WireMainMenuButtons();
                WireMainMenuNavigationButtons();
                StartCoroutine(WireAfterSceneBuild());
            }

            IEnumerator WireAfterSceneBuild()
            {
                // Main Menu UI can be rebuilt by its scene scripts one frame
                // after sceneLoaded. Wire again after that build completes so
                // returning from a game behaves exactly like a fresh launch.
                yield return null;
                yield return new WaitForSecondsRealtime(.1f);
                EnsureAudioListener();
                WireBackButton();
                WireMainMenuButtons();
                WireMainMenuNavigationButtons();
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
                WireGameButton("Counting", "COUNTING", OpenCountingFallback);
                WireGameButton("ABC", "ABC ADVENTURE", OpenABCFallback);
                WireGameButton("Match", "ALPHABET MATCH", OpenMatchFallback);
            }

            // Bottom navigation objects can be rebuilt by the menu at runtime.
            // Rebind them after every scene load so returning from a game does
            // not leave buttons pointing at the destroyed menu instance.
            static void WireMainMenuNavigationButtons()
            {
                if (SceneManager.GetActiveScene().name != "MainMenu") return;
                WireNavigationButton("Home", OpenHomeFallback);
                WireNavigationButton("Library", OpenLibraryFallback);
                WireNavigationButton("Rewards", OpenRewardsFallback);
                WireNavigationButton("Parent", OpenParentFallback);
            }

            static void WireNavigationButton(string token, UnityEngine.Events.UnityAction callback)
            {
                Button button = null;
                foreach (Button candidate in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!IsMatchingNavigationButton(candidate, token)) continue;
                    button = candidate;
                    break;
                }
                if (button == null) return;
                button.onClick.RemoveListener(callback);
                button.onClick.AddListener(callback);
                Debug.Log($"[LearningWithJourney] Wired Main Menu navigation button: {button.gameObject.name}");
            }

            static bool IsMatchingNavigationButton(Button button, string token)
            {
                if (button == null) return false;
                string name = button.gameObject.name;
                if (name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
                foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
                    if (text != null && text.text.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
                return false;
            }

            static void WireGameButton(string objectName, string labelToken, UnityEngine.Events.UnityAction callback)
            {
                if (SceneManager.GetActiveScene().name != "MainMenu") return;

                GameObject target = GameObject.Find(objectName);
                Button button = target != null ? target.GetComponent<Button>() : null;
                if (button == null)
                {
                    foreach (Button candidate in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        if (!IsMatchingGameButton(candidate, labelToken)) continue;
                        button = candidate;
                        break;
                    }
                }

                if (button == null)
                {
                    Debug.LogWarning($"[LearningWithJourney] Could not find Main Menu game button: {objectName}");
                    return;
                }

                button.onClick.RemoveListener(callback);
                button.onClick.AddListener(callback);
                Debug.Log($"[LearningWithJourney] Wired Main Menu game button: {button.gameObject.name}");
            }

            static bool IsMatchingGameButton(Button button, string labelToken)
            {
                if (button == null || string.IsNullOrWhiteSpace(labelToken)) return false;

                if (button.gameObject.name.IndexOf(labelToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (text == null || string.IsNullOrWhiteSpace(text.text)) continue;
                    if (text.text.IndexOf(labelToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }

                return false;
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
            static void OpenHomeFallback() => FindRouter().OpenMainMenu();
            static void OpenLibraryFallback() => FindRouter().OpenLibrary();
            static void OpenRewardsFallback() => FindRouter().OpenRewards();
            static void OpenParentFallback() => FindRouter().OpenParentZone();
        }
    }
}
