using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// First-launch child profile setup. The child's entered name is stored only
    /// in the existing local GameProgressService save data. This screen does not
    /// transmit the name to analytics, purchases, or any network service.
    /// </summary>
    public class ChildNameSetupControllerV1 : MonoBehaviour
    {
        [SerializeField] TMP_InputField nameInput;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text journeySpeechText;
        [SerializeField] bool skipIfProfileAlreadyExists = true;
        [SerializeField] string nextScene = "MainMenu";

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            var service = GameProgressService.Instance;
            if (skipIfProfileAlreadyExists && service != null && service.HasPlayerName)
            {
                SceneManager.LoadScene(nextScene);
                return;
            }

            if (nameInput != null)
            {
                nameInput.characterLimit = 20;
                nameInput.contentType = TMP_InputField.ContentType.Name;
                nameInput.text = string.Empty;
                nameInput.Select();
                nameInput.ActivateInputField();
            }

            SetStatus("Type your first name, then tap LET'S LEARN!");
            if (journeySpeechText != null)
                journeySpeechText.text = "Hi! I'm Journey! What's your name?";
        }

        public void SaveNameAndStart()
        {
            string value = nameInput != null ? nameInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                SetStatus("Type your name first!");
                if (journeySpeechText != null)
                    journeySpeechText.text = "Tell me your name so we can learn together!";
                nameInput?.Select();
                nameInput?.ActivateInputField();
                return;
            }

            if (value.Length > 20)
                value = value.Substring(0, 20).Trim();

            var service = GameProgressService.Instance;
            if (service == null) return;

            service.SetPlayerName(value);
            SetStatus("Welcome, " + value + "!");
            if (journeySpeechText != null)
                journeySpeechText.text = "Hi, " + value + "! Let's learn, grow, and shine!";

            SceneManager.LoadScene(nextScene);
        }

        public void ClearForTesting()
        {
            GameProgressService.Instance?.SetPlayerName(string.Empty);
            if (nameInput != null) nameInput.text = string.Empty;
            SetStatus("Name cleared for testing.");
        }

        void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }
    }
}
