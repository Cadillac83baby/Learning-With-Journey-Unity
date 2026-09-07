using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Polished parent dashboard controller. Child profile data remains local-only.
    /// Sensitive parent actions require an adult gate during the current session.
    /// </summary>
    public class ParentZoneControllerV2 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text starsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;

        [Header("Child Profile")]
        [SerializeField] TMP_Text profileNameText;
        [SerializeField] TMP_InputField childNameInput;
        [SerializeField] TMP_Text streakText;
        [SerializeField] TMP_Text currentLevelText;
        [SerializeField] TMP_Text profileStarsText;

        [Header("Learning Progress")]
        [SerializeField] TMP_Text abcCorrectText;
        [SerializeField] TMP_Text countingCorrectText;
        [SerializeField] TMP_Text alphabetPairsText;
        [SerializeField] Image abcProgressFill;
        [SerializeField] Image countingProgressFill;
        [SerializeField] Image matchingProgressFill;

        [Header("Parent Tools")]
        [SerializeField] Button editNameButton;
        [SerializeField] Button resetProgressButton;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text journeySpeechText;
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
            SetStatus("Child profile is saved locally on this device.");
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
                OpenParentGate("Unlock Parent Tools to edit the child profile.");
                return;
            }

            if (childNameInput != null)
            {
                childNameInput.interactable = true;
                childNameInput.Select();
                childNameInput.ActivateInputField();
            }
            SetStatus("Edit the name, then tap SAVE PROGRESS.");
        }

        public void SaveProgress()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            if (parentGateUnlocked && childNameInput != null && childNameInput.interactable)
            {
                string value = childNameInput.text.Trim();
                if (!string.IsNullOrWhiteSpace(value)) service.SetPlayerName(value);
                childNameInput.interactable = false;
            }

            PlayerPrefs.Save();
            Refresh();
            SetStatus("Progress is saved on this device.");
            if (journeySpeechText != null)
                journeySpeechText.text = "Great job! My progress is saved!";
        }

        public void OpenParentGate() => OpenParentGate("For grown-ups only: answer the question to unlock Parent Tools.");

        void OpenParentGate(string message)
        {
            if (parentGatePanel != null) parentGatePanel.SetActive(true);
            if (parentGateMessageText != null)
                parentGateMessageText.text = message + "\n\nWhat is 4 + 3?";
        }

        public void AnswerParentGate6() => AnswerParentGate(6);
        public void AnswerParentGate7() => AnswerParentGate(7);
        public void AnswerParentGate8() => AnswerParentGate(8);

        void AnswerParentGate(int answer)
        {
            if (answer == 7)
            {
                parentGateUnlocked = true;
                if (parentGatePanel != null) parentGatePanel.SetActive(false);
                SetStatus("Parent Tools unlocked for this session.");
                if (journeySpeechText != null)
                    journeySpeechText.text = "Grown-up tools are ready!";
            }
            else if (parentGateMessageText != null)
            {
                parentGateMessageText.text = "That answer is not correct. Please try again.\n\nWhat is 4 + 3?";
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
                if (resetButtonText != null) resetButtonText.text = "TAP AGAIN TO RESET";
                SetStatus("Tap RESET PROGRESS again within 5 seconds to confirm.");
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
            if (profileNameText != null) profileNameText.text = "Child Name: " + name;
            if (childNameInput != null && !childNameInput.isFocused) childNameInput.text = name;

            if (profileStarsText != null) profileStarsText.text = p.stars.ToString();
            if (streakText != null) streakText.text = p.currentStreak.ToString();
            if (currentLevelText != null) currentLevelText.text = "Level " + service.Level;

            int abc = Mathf.Max(0, p.abcCorrect);
            int counting = Mathf.Max(0, p.countingCorrect);
            int match = Mathf.Max(0, p.alphabetPairs);

            if (abcCorrectText != null) abcCorrectText.text = abc + " / 20\nCorrect Answers";
            if (countingCorrectText != null) countingCorrectText.text = counting + " / 20\nCorrect Answers";
            if (alphabetPairsText != null) alphabetPairsText.text = match + " / 20\nCorrect Answers";

            if (abcProgressFill != null) abcProgressFill.fillAmount = Mathf.Clamp01(abc / 20f);
            if (countingProgressFill != null) countingProgressFill.fillAmount = Mathf.Clamp01(counting / 20f);
            if (matchingProgressFill != null) matchingProgressFill.fillAmount = Mathf.Clamp01(match / 20f);
        }

        void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }
    }
}
