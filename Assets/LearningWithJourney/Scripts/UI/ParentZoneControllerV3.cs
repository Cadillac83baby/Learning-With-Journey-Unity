using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Cleaner parent dashboard controller with fewer duplicated stats and larger,
    /// easier-to-read controls. Child profile data remains local-only.
    /// </summary>
    public class ParentZoneControllerV3 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text starsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;

        [Header("Profile")]
        [SerializeField] TMP_Text profileNameText;
        [SerializeField] TMP_InputField childNameInput;
        [SerializeField] TMP_Text gamesCompletedText;
        [SerializeField] TMP_Text streakText;

        [Header("Learning Progress")]
        [SerializeField] TMP_Text abcCorrectText;
        [SerializeField] TMP_Text countingCorrectText;
        [SerializeField] TMP_Text alphabetPairsText;
        [SerializeField] Image abcProgressFill;
        [SerializeField] Image countingProgressFill;
        [SerializeField] Image matchingProgressFill;

        [Header("Messages")]
        [SerializeField] TMP_Text journeySpeechText;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text resetButtonText;

        [Header("Parent Gate")]
        [SerializeField] GameObject parentGatePanel;
        [SerializeField] TMP_Text parentGateMessageText;

        bool parentGateUnlocked;
        bool resetArmed;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += Refresh;

            parentGateUnlocked = false;
            if (parentGatePanel != null) parentGatePanel.SetActive(false);
            if (childNameInput != null) childNameInput.interactable = false;

            Refresh();
            SetStatus("Profile stays on this device. Parent tools require a grown-up check.");
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= Refresh;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");
        public void GoLibrary() => SceneManager.LoadScene("Library");
        public void GoRewards() => SceneManager.LoadScene("RewardsRoom");

        public void RequestEditName()
        {
            if (!parentGateUnlocked)
            {
                OpenParentGate("Unlock Parent Tools to edit the child name.");
                return;
            }

            if (childNameInput != null)
            {
                childNameInput.interactable = true;
                childNameInput.Select();
                childNameInput.ActivateInputField();
            }
            SetStatus("Edit the name, then tap SAVE NAME.");
        }

        public void SaveName()
        {
            if (!parentGateUnlocked)
            {
                OpenParentGate("Unlock Parent Tools to save profile changes.");
                return;
            }

            var service = GameProgressService.Instance;
            if (service == null || childNameInput == null) return;

            string value = childNameInput.text.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                SetStatus("Enter a name before saving.");
                return;
            }

            service.SetPlayerName(value);
            childNameInput.interactable = false;
            SetStatus("Child name saved on this device.");
            if (journeySpeechText != null)
                journeySpeechText.text = "Thanks! My name is saved!";
        }

        public void OpenParentGate()
        {
            OpenParentGate("For grown-ups only: answer the question to unlock Parent Tools.");
        }

        void OpenParentGate(string message)
        {
            if (parentGatePanel != null) parentGatePanel.SetActive(true);
            if (parentGateMessageText != null)
                parentGateMessageText.text = message + "\n\nWhat is 4 + 3?";
        }

        public void AnswerParentGate(int answer)
        {
            if (answer == 7)
            {
                parentGateUnlocked = true;
                if (parentGatePanel != null) parentGatePanel.SetActive(false);
                SetStatus("Parent Tools unlocked for this session.");
            }
            else if (parentGateMessageText != null)
            {
                parentGateMessageText.text = "That answer is not correct. Try again.\n\nWhat is 4 + 3?";
            }
        }

        public void CloseParentGate()
        {
            if (parentGatePanel != null) parentGatePanel.SetActive(false);
        }

        public void RequestResetProgress()
        {
            if (!parentGateUnlocked)
            {
                OpenParentGate("Unlock Parent Tools before resetting progress.");
                return;
            }

            if (!resetArmed)
            {
                resetArmed = true;
                if (resetButtonText != null) resetButtonText.text = "TAP AGAIN";
                SetStatus("Tap RESET again within 5 seconds to confirm.");
                CancelInvoke(nameof(CancelReset));
                Invoke(nameof(CancelReset), 5f);
                return;
            }

            CancelInvoke(nameof(CancelReset));
            resetArmed = false;
            GameProgressService.Instance?.ResetProgress();
            if (resetButtonText != null) resetButtonText.text = "RESET PROGRESS";
            SetStatus("Learning progress has been reset.");
            if (journeySpeechText != null)
                journeySpeechText.text = "A fresh start! Let's learn, grow, and shine!";
        }

        public void CancelReset()
        {
            resetArmed = false;
            if (resetButtonText != null) resetButtonText.text = "RESET PROGRESS";
        }

        void Refresh()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;
            var p = service.Progress;

            string name = string.IsNullOrWhiteSpace(p.playerName) ? "Little Star" : p.playerName;

            if (starsText != null) starsText.text = p.stars.ToString();
            if (coinsText != null) coinsText.text = p.coins.ToString();
            if (levelText != null) levelText.text = "LEVEL " + service.Level;
            if (profileNameText != null) profileNameText.text = name;
            if (childNameInput != null && !childNameInput.isFocused) childNameInput.text = name;
            if (gamesCompletedText != null) gamesCompletedText.text = p.gamesCompleted.ToString();
            if (streakText != null) streakText.text = p.currentStreak.ToString();

            int abc = Mathf.Max(0, p.abcCorrect);
            int counting = Mathf.Max(0, p.countingCorrect);
            int matching = Mathf.Max(0, p.alphabetPairs);

            if (abcCorrectText != null) abcCorrectText.text = abc + " / 20";
            if (countingCorrectText != null) countingCorrectText.text = counting + " / 20";
            if (alphabetPairsText != null) alphabetPairsText.text = matching + " / 20";

            if (abcProgressFill != null) abcProgressFill.fillAmount = Mathf.Clamp01(abc / 20f);
            if (countingProgressFill != null) countingProgressFill.fillAmount = Mathf.Clamp01(counting / 20f);
            if (matchingProgressFill != null) matchingProgressFill.fillAmount = Mathf.Clamp01(matching / 20f);
        }

        void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }
    }
}
