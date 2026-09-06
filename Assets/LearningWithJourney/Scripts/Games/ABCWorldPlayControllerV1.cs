using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class ABCWorldPlayControllerV1 : MonoBehaviour
    {
        [Header("ABC Activity")]
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text focusLetterText;
        [SerializeField] TMP_Text wordText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] TMP_Text roundText;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] Button[] answerButtons;
        [SerializeField] ABCWordPictureVisual pictureVisual;
        [SerializeField] Button letterRepeatButton;
        [SerializeField] Button pictureRepeatButton;
        [SerializeField] Button phraseRepeatButton;

        [Header("Journey")]
        [SerializeField] RectTransform journeyRect;
        [SerializeField] JourneyABCSpeech journeySpeech;

        [Header("Legacy Letter Audio - Optional")]
        [SerializeField] AudioSource letterAudioSource;
        [SerializeField] AudioClip[] letterClips;

        [Header("Progression")]
        [SerializeField, Range(1, 10)] int totalLevels = 10;
        [SerializeField, Range(1, 10)] int roundsPerLevel = 5;

        const string LevelKey = "LWJ_ABC_LEVEL_V1";
        const string RoundKey = "LWJ_ABC_ROUND_V1";
        const string CompleteKey = "LWJ_ABC_COMPLETE_V1";
        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Approved preschool A-Z vocabulary list.
        static readonly string[] Words =
        {
            "Apple", "Ball", "Cat", "Dog", "Elephant", "Fish", "Grapes", "Hat", "Ice Cream",
            "Juice", "Kite", "Lion", "Moon", "Nest", "Owl", "Pig", "Queen", "Rainbow",
            "Sun", "Turtle", "Umbrella", "Violin", "Watermelon", "Xylophone", "Yo-Yo", "Zebra"
        };

        readonly System.Random rng = new();
        int currentLevel;
        int round;
        int targetIndex;
        int previousTargetIndex = -1;
        bool worldCompleted;

        enum ChallengeMode
        {
            FindUppercase,
            MatchLowercase,
            BeginningLetter
        }

        ChallengeMode currentMode;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += RefreshPoints;

            totalLevels = Mathf.Clamp(totalLevels, 1, 10);
            roundsPerLevel = Mathf.Max(1, roundsPerLevel);
            currentLevel = Mathf.Clamp(PlayerPrefs.GetInt(LevelKey, 1), 1, totalLevels);
            round = Mathf.Clamp(PlayerPrefs.GetInt(RoundKey, 1), 1, roundsPerLevel);
            worldCompleted = PlayerPrefs.GetInt(CompleteKey, 0) == 1;

            WireRepeatButtons();
            RefreshPoints();
            UpdateProgressHud();

            if (worldCompleted)
            {
                currentLevel = totalLevels;
                round = roundsPerLevel;
                ShowCompletedState();
            }
            else
            {
                StartRound();
            }
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= RefreshPoints;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");

        public void ResetABCProgress()
        {
            currentLevel = 1;
            round = 1;
            worldCompleted = false;
            previousTargetIndex = -1;
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(RoundKey);
            PlayerPrefs.DeleteKey(CompleteKey);
            PlayerPrefs.Save();
            StartRound();
        }

        public void StartRound()
        {
            if (worldCompleted) return;

            CancelInvoke(nameof(StartRound));
            currentMode = GetChallengeMode(currentLevel);
            PickTargetForLevel();
            ConfigureChallenge();
            BuildAnswers();
            SetAnswersInteractable(true);
            UpdateProgressHud();
        }

        ChallengeMode GetChallengeMode(int level)
        {
            if (level == 9) return ChallengeMode.MatchLowercase;
            if (level == 10) return ChallengeMode.BeginningLetter;
            return ChallengeMode.FindUppercase;
        }

        void WireRepeatButtons()
        {
            if (letterRepeatButton != null)
            {
                letterRepeatButton.onClick.RemoveAllListeners();
                letterRepeatButton.onClick.AddListener(RepeatLetter);
            }

            if (pictureRepeatButton != null)
            {
                pictureRepeatButton.onClick.RemoveAllListeners();
                pictureRepeatButton.onClick.AddListener(RepeatWord);
            }

            if (phraseRepeatButton != null)
            {
                phraseRepeatButton.onClick.RemoveAllListeners();
                phraseRepeatButton.onClick.AddListener(RepeatPhrase);
            }
        }

        void PickTargetForLevel()
        {
            GetLetterRangeForLevel(currentLevel, out int minIndex, out int maxIndex);

            int picked = rng.Next(minIndex, maxIndex + 1);
            if (maxIndex > minIndex && picked == previousTargetIndex)
                picked = picked == maxIndex ? minIndex : picked + 1;

            targetIndex = Mathf.Clamp(picked, 0, 25);
            previousTargetIndex = targetIndex;
        }

        void ConfigureChallenge()
        {
            string letter = Alphabet[targetIndex].ToString();
            string lower = letter.ToLowerInvariant();
            string word = Words[targetIndex];

            pictureVisual?.Show(targetIndex);

            if (feedbackText)
                feedbackText.text = LevelInstruction(currentLevel);

            switch (currentMode)
            {
                case ChallengeMode.MatchLowercase:
                    if (promptText) promptText.text = $"Which big letter matches {lower}?";
                    if (focusLetterText) focusLetterText.text = lower;
                    if (wordText) wordText.text = $"{letter} is for {word}";
                    if (speechText) speechText.text = $"Find the big letter that matches {lower}.";
                    if (journeySpeech != null)
                        journeySpeech.SpeakLetter(targetIndex, letter);
                    else
                        PlayLegacyLetterAudio(targetIndex);
                    break;

                case ChallengeMode.BeginningLetter:
                    if (promptText) promptText.text = $"Which letter starts {word}?";
                    if (focusLetterText) focusLetterText.text = "?";
                    if (wordText) wordText.text = word;
                    if (speechText) speechText.text = $"What letter does {word} start with?";
                    if (journeySpeech != null)
                        journeySpeech.SpeakWord(targetIndex, word);
                    else
                        PlayLegacyLetterAudio(targetIndex);
                    break;

                default:
                    if (promptText) promptText.text = $"Can you find the letter {letter}?";
                    if (focusLetterText) focusLetterText.text = letter;
                    if (wordText) wordText.text = $"{letter} is for {word}";
                    if (speechText) speechText.text = $"{letter} is for {word}. Find {letter} below!";
                    if (journeySpeech != null)
                        journeySpeech.SpeakPhrase(targetIndex, letter, word);
                    else
                        PlayLegacyLetterAudio(targetIndex);
                    break;
            }
        }

        public void RepeatLetter()
        {
            if (targetIndex < 0 || targetIndex >= Alphabet.Length) return;
            string letter = Alphabet[targetIndex].ToString();
            if (speechText) speechText.text = $"Letter {letter}.";

            if (journeySpeech != null)
                journeySpeech.SpeakLetter(targetIndex, letter);
            else
                PlayLegacyLetterAudio(targetIndex);
        }

        public void RepeatWord()
        {
            if (targetIndex < 0 || targetIndex >= Words.Length) return;
            string word = Words[targetIndex];
            if (speechText) speechText.text = word + ".";
            journeySpeech?.SpeakWord(targetIndex, word);
        }

        public void RepeatPhrase()
        {
            if (targetIndex < 0 || targetIndex >= Alphabet.Length || targetIndex >= Words.Length) return;
            string letter = Alphabet[targetIndex].ToString();
            string word = Words[targetIndex];
            if (speechText) speechText.text = $"{letter} is for {word}.";

            if (journeySpeech != null)
                journeySpeech.SpeakPhrase(targetIndex, letter, word);
            else
                PlayLegacyLetterAudio(targetIndex);
        }

        void BuildAnswers()
        {
            if (answerButtons == null || answerButtons.Length == 0) return;

            string correct = Alphabet[targetIndex].ToString();
            var values = new HashSet<string> { correct };

            GetAnswerRangeForLevel(currentLevel, out int minIndex, out int maxIndex);
            int guard = 0;
            while (values.Count < answerButtons.Length && guard++ < 100)
                values.Add(Alphabet[rng.Next(minIndex, maxIndex + 1)].ToString());

            while (values.Count < answerButtons.Length)
                values.Add(Alphabet[rng.Next(Alphabet.Length)].ToString());

            var list = new List<string>(values);
            for (int i = 0; i < list.Count; i++)
            {
                int swap = rng.Next(i, list.Count);
                (list[i], list[swap]) = (list[swap], list[i]);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                var button = answerButtons[i];
                if (!button) continue;

                string value = list[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Answer(value));
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label) label.text = value;
                button.interactable = true;
            }
        }

        void Answer(string value)
        {
            if (worldCompleted) return;

            string correct = Alphabet[targetIndex].ToString();
            string word = Words[targetIndex];

            if (!string.Equals(value, correct, System.StringComparison.OrdinalIgnoreCase))
            {
                if (feedbackText) feedbackText.text = "Good try. Pick another letter.";
                if (speechText) speechText.text = $"Try again. Look for {correct}.";
                GameProgressService.Instance?.RegisterMiss();
                return;
            }

            SetAnswersInteractable(false);
            if (feedbackText) feedbackText.text = "Great job! You found it!";
            if (focusLetterText) focusLetterText.text = correct;
            if (wordText) wordText.text = $"{correct} is for {word}";
            if (speechText) speechText.text = $"Great job! {correct} is for {word}!";

            journeySpeech?.SpeakPhrase(targetIndex, correct, word);

            GameProgressService.Instance?.AwardCorrect("abc");
            GameProgressService.Instance?.CompleteGame();
            if (journeyRect) StartCoroutine(CelebrateJourney());

            AdvanceProgress();
        }

        void AdvanceProgress()
        {
            if (round < roundsPerLevel)
            {
                round++;
                SaveProgress();
                Invoke(nameof(StartRound), 1.8f);
                return;
            }

            if (currentLevel < totalLevels)
            {
                int completedLevel = currentLevel;
                currentLevel++;
                round = 1;
                SaveProgress();
                GameProgressService.Instance?.AddReward(3, 15);

                if (speechText) speechText.text = $"Level {completedLevel} complete! Level {currentLevel} is ready!";
                if (feedbackText) feedbackText.text = "New alphabet level unlocked!";
                UpdateProgressHud();
                Invoke(nameof(StartRound), 2.3f);
                return;
            }

            worldCompleted = true;
            PlayerPrefs.SetInt(CompleteKey, 1);
            PlayerPrefs.SetInt(LevelKey, totalLevels);
            PlayerPrefs.SetInt(RoundKey, roundsPerLevel);
            PlayerPrefs.Save();
            GameProgressService.Instance?.AddReward(10, 50);
            ShowCompletedState();
        }

        void ShowCompletedState()
        {
            worldCompleted = true;
            SetAnswersInteractable(false);
            if (promptText) promptText.text = "ABC WORLD COMPLETE!";
            if (focusLetterText) focusLetterText.text = "A-Z";
            if (wordText) wordText.text = "ALPHABET SUPERSTAR";
            if (speechText) speechText.text = "Amazing! You finished all 10 ABC levels!";
            if (feedbackText) feedbackText.text = "You learned your letters, words, and beginning sounds!";
            if (roundText) roundText.text = $"LEVEL {totalLevels} COMPLETE";
            if (levelText) levelText.text = $"LEVEL {totalLevels} / {totalLevels}";
        }

        void GetLetterRangeForLevel(int level, out int minIndex, out int maxIndex)
        {
            switch (Mathf.Clamp(level, 1, 10))
            {
                case 1: minIndex = 0; maxIndex = 2; break;    // A-C
                case 2: minIndex = 3; maxIndex = 5; break;    // D-F
                case 3: minIndex = 6; maxIndex = 8; break;    // G-I
                case 4: minIndex = 9; maxIndex = 11; break;   // J-L
                case 5: minIndex = 12; maxIndex = 14; break;  // M-O
                case 6: minIndex = 15; maxIndex = 17; break;  // P-R
                case 7: minIndex = 18; maxIndex = 20; break;  // S-U
                case 8: minIndex = 21; maxIndex = 25; break;  // V-Z
                default: minIndex = 0; maxIndex = 25; break;  // advanced review
            }
        }

        void GetAnswerRangeForLevel(int level, out int minIndex, out int maxIndex)
        {
            if (level >= 9)
            {
                minIndex = 0;
                maxIndex = 25;
                return;
            }

            GetLetterRangeForLevel(level, out minIndex, out maxIndex);
        }

        string LevelInstruction(int level)
        {
            if (level <= 8)
            {
                GetLetterRangeForLevel(level, out int min, out int max);
                return $"Level {level}: practice {Alphabet[min]} through {Alphabet[max]}.";
            }

            if (level == 9)
                return "Level 9: match little letters to big letters.";

            return "Level 10: find the beginning letter for each picture and word.";
        }

        void PlayLegacyLetterAudio(int index)
        {
            if (letterAudioSource == null || letterClips == null) return;
            if (index < 0 || index >= letterClips.Length) return;
            var clip = letterClips[index];
            if (clip != null) letterAudioSource.PlayOneShot(clip);
        }

        void SaveProgress()
        {
            PlayerPrefs.SetInt(LevelKey, currentLevel);
            PlayerPrefs.SetInt(RoundKey, round);
            PlayerPrefs.SetInt(CompleteKey, worldCompleted ? 1 : 0);
            PlayerPrefs.Save();
        }

        void UpdateProgressHud()
        {
            if (levelText) levelText.text = $"LEVEL {currentLevel} / {totalLevels}";
            if (roundText) roundText.text = $"ROUND {round} / {roundsPerLevel}";
        }

        void SetAnswersInteractable(bool value)
        {
            if (answerButtons == null) return;
            foreach (var button in answerButtons)
                if (button) button.interactable = value;
        }

        IEnumerator CelebrateJourney()
        {
            Vector2 start = journeyRect.anchoredPosition;
            Vector3 scale = journeyRect.localScale;
            float elapsed = 0f;
            const float duration = .50f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                float y = Mathf.Sin(p * Mathf.PI) * 18f;
                float s = 1f + Mathf.Sin(p * Mathf.PI) * .04f;
                journeyRect.anchoredPosition = start + new Vector2(0f, y);
                journeyRect.localScale = scale * s;
                yield return null;
            }

            journeyRect.anchoredPosition = start;
            journeyRect.localScale = scale;
        }

        void RefreshPoints()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;
            if (pointsText) pointsText.text = service.Progress.stars.ToString();
        }
    }
}
