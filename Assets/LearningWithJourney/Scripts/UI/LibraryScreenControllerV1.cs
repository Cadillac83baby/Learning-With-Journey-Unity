using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    public class LibraryScreenControllerV1 : MonoBehaviour
    {
        public const string SelectedBookKey = "LWJ_LIBRARY_SELECTED_BOOK_V1";

        [Header("HUD")]
        [SerializeField] TMP_Text starsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;

        [Header("Journey")]
        [SerializeField] TMP_Text speechText;

        [Header("Selection")]
        [SerializeField] TMP_Text selectionTitleText;
        [SerializeField] TMP_Text selectionMessageText;
        [SerializeField] Button openBookButton;

        string selectedBookId = "";

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
            selectedBookId = "";
            if (openBookButton != null) openBookButton.interactable = false;
            SetSelection(
                "CHOOSE A BOOK SHELF",
                "Pick something fun to read and learn with Journey!",
                "What should we read today?");
        }

        public void SelectABCBooks()
        {
            SelectBook(
                "ABC",
                "ABC BOOKS",
                "Letters, sounds, and first words with big pictures and simple preschool reading.",
                "Let's read our ABCs together!");
        }

        public void SelectNumbersCounting()
        {
            SelectBook(
                "NUMBERS",
                "NUMBERS AND COUNTING",
                "Counting stories and number practice from 1 through 20.",
                "Let's count while we read!");
        }

        public void SelectColorsShapes()
        {
            SelectBook(
                "COLORS",
                "COLORS AND SHAPES",
                "Bright picture pages for colors, shapes, sorting, and early visual learning.",
                "Can you find your favorite color?");
        }

        public void SelectStoryTime()
        {
            SelectBook(
                "STORY",
                "STORY TIME",
                "Short preschool stories with pictures, page turns, and Journey's read-aloud prompts.",
                "Story time is one of my favorite times!");
        }

        public void OpenSelectedBook()
        {
            if (string.IsNullOrEmpty(selectedBookId)) return;
            PlayerPrefs.SetString(SelectedBookKey, selectedBookId);
            PlayerPrefs.Save();
            SceneManager.LoadScene("BookReader");
        }

        void SelectBook(string id, string title, string message, string speech)
        {
            selectedBookId = id;
            if (openBookButton != null) openBookButton.interactable = true;
            SetSelection(title, message, speech);
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
