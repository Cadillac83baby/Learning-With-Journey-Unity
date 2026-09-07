using System.Collections;
using LearningWithJourney.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Branded loading screen controller with a single non-looping audio tag.
    /// Waits long enough for the tag to finish, then routes first-time players
    /// to NameSetup and returning players to MainMenu.
    /// </summary>
    public class SplashControllerV2 : MonoBehaviour
    {
        [SerializeField] AudioSource loadingAudioSource;
        [SerializeField] float minimumDisplaySeconds = 2.0f;
        [SerializeField] string nameSetupScene = "NameSetup";
        [SerializeField] string mainMenuScene = "MainMenu";

        IEnumerator Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            float displaySeconds = Mathf.Max(.5f, minimumDisplaySeconds);

            if (loadingAudioSource != null && loadingAudioSource.clip != null)
            {
                loadingAudioSource.playOnAwake = false;
                loadingAudioSource.loop = false;
                loadingAudioSource.volume = 1f;
                loadingAudioSource.Play();
                displaySeconds = Mathf.Max(displaySeconds, loadingAudioSource.clip.length + .15f);
            }

            yield return new WaitForSecondsRealtime(displaySeconds);

            var service = GameProgressService.Instance;
            string next = service != null && service.HasPlayerName ? mainMenuScene : nameSetupScene;
            SceneManager.LoadScene(next);
        }
    }
}
