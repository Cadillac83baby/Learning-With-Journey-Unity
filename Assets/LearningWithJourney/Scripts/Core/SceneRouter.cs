using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.Core
{
    public class SceneRouter : MonoBehaviour
    {
        public void OpenMainMenu() => Load("MainMenu");
        public void OpenCounting() => Load("CountingWorld");
        public void OpenABC() => Load("ABCWorld");
        public void OpenAlphabetMatch() => Load("AlphabetMatchWorld");
        public void OpenRewards() => Load("RewardsRoom");
        public void OpenLibrary() => Load("Library");
        public void OpenParentZone() => Load("ParentZone");

        public void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            SceneManager.LoadScene(sceneName);
        }
    }
}
