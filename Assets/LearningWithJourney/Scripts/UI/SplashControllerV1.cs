using System.Collections;
using LearningWithJourney.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Branded loading screen shown after access is granted and before gameplay.
    /// Routes first-time players to NameSetup and returning players to MainMenu.
    /// </summary>
    public class SplashControllerV1 : MonoBehaviour
    {
        [SerializeField] float minimumDisplaySeconds = 2.0f;
        [SerializeField] string nameSetupScene = "NameSetup";
        [SerializeField] string mainMenuScene = "MainMenu";

        IEnumerator Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            yield return new WaitForSecondsRealtime(Mathf.Max(.5f, minimumDisplaySeconds));

            var service = GameProgressService.Instance;
            string next = service != null && service.HasPlayerName ? mainMenuScene : nameSetupScene;
            SceneManager.LoadScene(next);
        }
    }
}
