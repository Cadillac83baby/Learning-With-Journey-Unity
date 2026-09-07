using System.Collections;
using LearningWithJourney.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Branded loading screen shown after access is granted and before gameplay.
    /// Plays the startup brand audio once, waits until the clip finishes (or the
    /// minimum display time elapses), then routes first-time players to NameSetup
    /// and returning players to MainMenu.
    /// </summary>
    public class SplashControllerV1 : MonoBehaviour
    {
        [SerializeField] float minimumDisplaySeconds = 2.0f;
        [SerializeField] float audioTailPaddingSeconds = 0.12f;
        [SerializeField] AudioSource startupAudioSource;
        [SerializeField] string nameSetupScene = "NameSetup";
        [SerializeField] string mainMenuScene = "MainMenu";

        IEnumerator Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            float waitSeconds = Mathf.Max(.5f, minimumDisplaySeconds);

            if (startupAudioSource != null && startupAudioSource.clip != null)
            {
                startupAudioSource.loop = false;
                startupAudioSource.playOnAwake = false;
                startupAudioSource.Stop();
                startupAudioSource.time = 0f;
                startupAudioSource.Play();
                waitSeconds = Mathf.Max(waitSeconds, startupAudioSource.clip.length + Mathf.Max(0f, audioTailPaddingSeconds));
            }

            yield return new WaitForSecondsRealtime(waitSeconds);

            var service = GameProgressService.Instance;
            string next = service != null && service.HasPlayerName ? mainMenuScene : nameSetupScene;
            SceneManager.LoadScene(next);
        }
    }
}
