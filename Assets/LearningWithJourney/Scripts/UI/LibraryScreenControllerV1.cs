using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    public class LibraryScreenControllerV1 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text starsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;

        [Header("Journey")]
        [SerializeField] TMP_Text speechText;

        [Header("Selection")]
        [SerializeField] TMP_Text selectionTitleText;
        [SerializeField] TMP_Text selectionMessageText;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += RefreshHud;

            RefreshHud();
            ShowWelcome();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= RefreshHud;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");
        public void GoRewards() => SceneManager.LoadScene("RewardsRoom");
        public void GoParents() => SceneManager.LoadScene("ParentZone");

        public void ShowWelcome()
        {
            SetSelection(
                "CHOOSE A BOOK SHELF",
                "Pick something fun to read and learn with Journey!",
                "What should we read today?");
        }

        public void SelectABCBooks()
        {
            SetSelection(
                "ABC BOOKS",
                "Letters, sounds, and first words. The ABC reader is the first book set we will build next.",
                "Let's read our ABCs together!");
        }

        public void SelectNumbersCounting()
        {
            SetSelection(
                "NUMBERS AND COUNTING",
                "Counting stories and number practice from 1 through 20.",
                "Let's count while we read!");
        }

        public void SelectColorsShapes()
        {
            SetSelection(
                "COLORS AND SHAPES",
                "Bright picture books for colors, shapes, sorting, and early visual learning.",
                "Can you find your favorite color?");
        }

        public void SelectStoryTime()
        {
            SetSelection(
                "STORY TIME",
                "Short preschool stories made for read-aloud, page turns, pictures, and Journey's voice prompts.",
                "Story time is one of my favorite times!");
        }

        void SetSelection(string title, string message, string speech)
        {
            if (selectionTitleText != null) selectionTitleText.text = title;
            if (selectionMessageText != null) selectionMessageText.text = message;
            if (speechText != null) speechText.text = speech;
        }

        void RefreshHud()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            if (starsText != null) starsText.text = service.Progress.stars.ToString();
            if (coinsText != null) coinsText.text = service.Progress.coins.ToString();
            if (levelText != null) levelText.text = "LEVEL " + service.Level;
        }
    }
}
