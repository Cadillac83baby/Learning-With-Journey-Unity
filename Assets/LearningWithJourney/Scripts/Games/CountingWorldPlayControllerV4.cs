using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class CountingWorldPlayControllerV4 : MonoBehaviour
    {
        [Header("Counting Objects")]
        [SerializeField] GameObject[] countObjects;
        [SerializeField] TMP_Text[] countBadges;

        [Header("Answer UI")]
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] TMP_Text roundText;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text levelText;

        [Header("Journey")]
        [SerializeField] RectTransform journeyRect;

        [Header("Number Audio - Optional")]
        [SerializeField] AudioSource numberAudioSource;
        [SerializeField] AudioClip[] numberClips;

        [Header("Progression")]
        [SerializeField, Range(1, 10)] int totalLevels = 10;
        [SerializeField, Range(1, 10)] int roundsPerLevel = 5;

        const string LevelKey = "LWJ_COUNTING_LEVEL_V1";
        const string RoundKey = "LWJ_COUNTING_ROUND_V1";
        const string CompleteKey = "LWJ_COUNTING_COMPLETE_V1";

        readonly System.Random rng = new();
        bool[] counted;
        int[] assignedNumbers;
        Color[] originalColors;

        int targetCount;
        int tappedCount;
        int currentLevel;
        int round;
        bool worldCompleted;
        Coroutine revealRoutine;

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

            PrepareApples();
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

        public void ResetCountingProgress()
        {
            currentLevel = 1;
            round = 1;
            worldCompleted = false;
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(RoundKey);
            PlayerPrefs.DeleteKey(CompleteKey);
            PlayerPrefs.Save();
            StartRound();
        }

        void PrepareApples()
        {
            int length = countObjects != null ? countObjects.Length : 0;
            counted = new bool[length];
            assignedNumbers = new int[length];
            originalColors = new Color[length];

            for (int i = 0; i < length; i++)
            {
                int index = i;
                var apple = countObjects[i];
                if (!apple) continue;

                var image = apple.GetComponent<Image>();
                if (image != null)
                {
                    originalColors[i] = image.color;
                    image.raycastTarget = true;
                }

                var button = apple.GetComponent<Button>();
                if (button == null) button = apple.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => TapApple(index));
            }
        }

        public void StartRound()
        {
            if (worldCompleted) return;

            if (revealRoutine != null)
                StopCoroutine(revealRoutine);

            CancelInvoke(nameof(StartRound));

            GetCountRangeForLevel(currentLevel, out int minCount, out int maxCount);
            targetCount = rng.Next(minCount, maxCount + 1);
            tappedCount = 0;

            if (promptText) promptText.text = "Touch the apples. How many are there?";
            if (speechText) speechText.text = "Touch each apple and count with me!";
            if (feedbackText) feedbackText.text = LevelInstruction(currentLevel);

            UpdateProgressHud();

            for (int i = 0; i < countObjects.Length; i++)
            {
                var apple = countObjects[i];
                if (!apple) continue;

                bool visible = i < targetCount;
                apple.SetActive(visible);
                apple.transform.localScale = visible ? Vector3.one * .82f : Vector3.zero;

                if (i < counted.Length) counted[i] = false;
                if (i < assignedNumbers.Length) assignedNumbers[i] = 0;

                var image = apple.GetComponent<Image>();
                if (image != null && i < originalColors.Length)
                    image.color = originalColors[i];

                var button = apple.GetComponent<Button>();
                if (button != null) button.interactable = visible;

                if (countBadges != null && i < countBadges.Length && countBadges[i] != null)
                {
                    countBadges[i].text = "";
                    if (countBadges[i].transform.parent != null)
                        countBadges[i].transform.parent.gameObject.SetActive(false);
                }
            }

            BuildAnswers();
            SetAnswersInteractable(false);
            revealRoutine = StartCoroutine(RevealApples());
        }

        IEnumerator RevealApples()
        {
            yield return new WaitForSeconds(.18f);

            for (int i = 0; i < targetCount && i < countObjects.Length; i++)
            {
                var apple = countObjects[i];
                if (!apple) continue;
                yield return Pop(apple.transform, .82f, 1.05f, .11f);
            }

            if (speechText) speechText.text = "Your turn! Touch an apple to start at 1.";
            revealRoutine = null;
        }

        void TapApple(int index)
        {
            if (worldCompleted) return;
            if (index < 0 || index >= targetCount || index >= countObjects.Length) return;

            var apple = countObjects[index];
            if (!apple || !apple.activeInHierarchy) return;

            if (counted[index])
            {
                SpeakNumber(assignedNumbers[index]);
                StartCoroutine(Pop(apple.transform, 1f, 1.12f, .14f));
                return;
            }

            tappedCount++;
            counted[index] = true;
            assignedNumbers[index] = tappedCount;

            var image = apple.GetComponent<Image>();
            if (image != null)
                image.color = Color.Lerp(originalColors[index], Color.white, .20f);

            if (countBadges != null && index < countBadges.Length && countBadges[index] != null)
            {
                countBadges[index].text = tappedCount.ToString();
                if (countBadges[index].transform.parent != null)
                    countBadges[index].transform.parent.gameObject.SetActive(true);
            }

            SpeakNumber(tappedCount);
            StartCoroutine(Pop(apple.transform, 1f, 1.18f, .18f));

            if (feedbackText)
                feedbackText.text = $"You counted {tappedCount}!";

            if (tappedCount >= targetCount)
            {
                if (speechText) speechText.text = $"Great job! You counted {targetCount}. Now choose the number!";
                if (feedbackText) feedbackText.text = "Now tap the correct answer below.";
                SetAnswersInteractable(true);
            }
        }

        void SpeakNumber(int number)
        {
            if (number <= 0) return;

            if (speechText) speechText.text = number.ToString();

            if (numberAudioSource != null && numberClips != null && number <= numberClips.Length)
            {
                var clip = numberClips[number - 1];
                if (clip != null)
                    numberAudioSource.PlayOneShot(clip);
            }
        }

        void BuildAnswers()
        {
            if (answerButtons == null || answerButtons.Length == 0) return;

            var values = new HashSet<int> { targetCount };
            int spread = currentLevel <= 3 ? 4 : currentLevel <= 6 ? 3 : 2;
            int guard = 0;

            while (values.Count < answerButtons.Length && guard++ < 100)
            {
                int candidate = Mathf.Clamp(targetCount + rng.Next(-spread, spread + 1), 1, 20);
                values.Add(candidate);
            }

            for (int n = 1; values.Count < answerButtons.Length && n <= 20; n++)
                values.Add(n);

            var list = new List<int>(values);
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                var button = answerButtons[i];
                if (!button) continue;

                int value = list[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Answer(value));

                var label = button.GetComponentInChildren<TMP_Text>();
                if (label) label.text = value.ToString();
            }
        }

        void Answer(int value)
        {
            if (worldCompleted) return;

            if (tappedCount < targetCount)
            {
                if (speechText) speechText.text = "Count all the apples first!";
                return;
            }

            if (value != targetCount)
            {
                if (feedbackText) feedbackText.text = "Good try. Pick another number.";
                if (speechText) speechText.text = "Try again!";
                GameProgressService.Instance?.RegisterMiss();
                return;
            }

            SetAnswersInteractable(false);
            if (feedbackText) feedbackText.text = "Great counting!";
            if (speechText) speechText.text = $"Yes! The answer is {targetCount}!";

            GameProgressService.Instance?.AwardCorrect("counting");
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
                Invoke(nameof(StartRound), 1.9f);
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
                if (feedbackText) feedbackText.text = "New level unlocked!";
                UpdateProgressHud();
                Invoke(nameof(StartRound), 2.4f);
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

            if (promptText) promptText.text = "COUNTING WORLD COMPLETE!";
            if (speechText) speechText.text = "Amazing! You finished all 10 counting levels!";
            if (feedbackText) feedbackText.text = "You are a counting superstar!";
            if (roundText) roundText.text = $"LEVEL {totalLevels} COMPLETE";
            if (levelText) levelText.text = $"LEVEL {totalLevels} / {totalLevels}";
        }

        void GetCountRangeForLevel(int level, out int min, out int max)
        {
            switch (Mathf.Clamp(level, 1, 10))
            {
                case 1: min = 1; max = 3; break;
                case 2: min = 1; max = 5; break;
                case 3: min = 2; max = 6; break;
                case 4: min = 3; max = 8; break;
                case 5: min = 4; max = 10; break;
                case 6: min = 5; max = 12; break;
                case 7: min = 7; max = 14; break;
                case 8: min = 9; max = 16; break;
                case 9: min = 11; max = 18; break;
                default: min = 13; max = 20; break;
            }
        }

        string LevelInstruction(int level)
        {
            GetCountRangeForLevel(level, out int min, out int max);
            return level == 1
                ? "Touch each apple one time."
                : $"Level {level}: count from {min} to {max}.";
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

        IEnumerator Pop(Transform target, float startScale, float peakScale, float duration)
        {
            if (!target) yield break;

            float elapsed = 0f;
            Vector3 start = Vector3.one * startScale;
            Vector3 peak = Vector3.one * peakScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                target.localScale = p < .52f
                    ? Vector3.Lerp(start, peak, p / .52f)
                    : Vector3.Lerp(peak, Vector3.one, (p - .52f) / .48f);
                yield return null;
            }

            target.localScale = Vector3.one;
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

        void SetAnswersInteractable(bool value)
        {
            if (answerButtons == null) return;
            foreach (var button in answerButtons)
                if (button) button.interactable = value;
        }

        void RefreshPoints()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;
            if (pointsText) pointsText.text = service.Progress.stars.ToString();
        }
    }
}
