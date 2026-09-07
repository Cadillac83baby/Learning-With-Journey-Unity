using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Parent-facing progress dashboard for Learning with Journey.
    /// Uses the existing persistent GameProgressService data and does not
    /// expose external links or child-directed purchases.
    /// </summary>
    public class ParentZoneControllerV1 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text starsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;

        [Header("Profile + Overview")]
        [SerializeField] TMP_InputField childNameInput;
        [SerializeField] TMP_Text profileNameText;
        [SerializeField] TMP_Text gamesCompletedText;
        [SerializeField] TMP_Text streakText;
        [SerializeField] TMP_Text bestStreakText;
        [SerializeField] Image levelProgressFill;
        [SerializeField] TMP_Text levelProgressText;

        [Header("Learning Progress")]
        [SerializeField] TMP_Text abcCorrectText;
        [SerializeField] TMP_Text countingCorrectText;
        [SerializeField] TMP_Text alphabetPairsText;

        [Header("Messages")]
        [SerializeField] TMP_Text journeySpeechText;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text resetButtonText;

        bool resetArmed;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += Refresh;

            Refresh();
            SetStatus("Review progress, celebrate growth, and keep learning fun at home.");
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= Refresh;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");
        public void GoLibrary() => SceneManager.LoadScene("Library");
        public void GoRewards() => SceneManager.LoadScene("RewardsRoom");

        public void SaveChildName()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            string value = childNameInput != null ? childNameInput.text : "Little Star";
            service.SetPlayerName(value);
            SetStatus("Child profile name saved.");
            if (journeySpeechText != null)
                journeySpeechText.text = "My name is saved! Let's keep learning!";
        }

        public void RequestResetProgress()
        {
            if (!resetArmed)
            {
                resetArmed = true;
                if (resetButtonText != null) resetButtonText.text = "TAP AGAIN TO RESET";
                SetStatus("Reset is armed. Tap the red button again within 5 seconds to erase learning progress.");
                CancelInvoke(nameof(CancelReset));
                Invoke(nameof(CancelReset), 5f);
                return;
            }

            CancelInvoke(nameof(CancelReset));
            resetArmed = false;
            GameProgressService.Instance?.ResetProgress();
            if (childNameInput != null) childNameInput.text = "Little Star";
            if (resetButtonText != null) resetButtonText.text = "RESET PROGRESS";
            SetStatus("Learning progress has been reset.");
            if (journeySpeechText != null)
                journeySpeechText.text = "A fresh start! We can learn and grow again!";
        }

        public void CancelReset()
        {
            resetArmed = false;
            if (resetButtonText != null) resetButtonText.text = "RESET PROGRESS";
            SetStatus("Reset cancelled. No progress was changed.");
        }

        void Refresh()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            var p = service.Progress;
            if (starsText != null) starsText.text = p.stars.ToString();
            if (coinsText != null) coinsText.text = p.coins.ToString();
            if (levelText != null) levelText.text = "LEVEL " + service.Level;

            if (profileNameText != null) profileNameText.text = string.IsNullOrWhiteSpace(p.playerName) ? "Little Star" : p.playerName;
            if (childNameInput != null && !childNameInput.isFocused)
                childNameInput.text = string.IsNullOrWhiteSpace(p.playerName) ? "Little Star" : p.playerName;

            if (gamesCompletedText != null) gamesCompletedText.text = p.gamesCompleted.ToString();
            if (streakText != null) streakText.text = p.currentStreak.ToString();
            if (bestStreakText != null) bestStreakText.text = p.bestStreak.ToString();

            int levelStars = Mathf.Max(0, p.stars % 50);
            if (levelProgressFill != null) levelProgressFill.fillAmount = levelStars / 50f;
            if (levelProgressText != null)
                levelProgressText.text = levelStars + " / 50 STARS TOWARD LEVEL " + (service.Level + 1);

            if (abcCorrectText != null) abcCorrectText.text = p.abcCorrect + " CORRECT";
            if (countingCorrectText != null) countingCorrectText.text = p.countingCorrect + " CORRECT";
            if (alphabetPairsText != null) alphabetPairsText.text = p.alphabetPairs + " PAIRS";

            if (journeySpeechText != null && string.IsNullOrWhiteSpace(journeySpeechText.text))
                journeySpeechText.text = "Look how much I'm learning!";
        }

        void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }
    }
}
