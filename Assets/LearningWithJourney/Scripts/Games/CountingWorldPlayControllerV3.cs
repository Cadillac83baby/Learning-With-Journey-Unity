using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class CountingWorldPlayControllerV3 : MonoBehaviour
    {
        [SerializeField] GameObject[] countObjects;
        [SerializeField] TMP_Text[] countBadges;
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] TMP_Text roundText;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] RectTransform journeyRect;
        [SerializeField] AudioSource numberAudioSource;
        [SerializeField] AudioClip[] numberClips;

        readonly System.Random rng = new();
        bool[] counted;
        int[] assignedNumbers;
        Color[] originalColors;
        int targetCount;
        int tappedCount;
        int round = 1;
        Coroutine revealRoutine;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += RefreshProgress;

            PrepareApples();
            RefreshProgress();
            StartRound();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= RefreshProgress;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");

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
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => TapApple(index));
            }
        }

        public void StartRound()
        {
            if (revealRoutine != null)
                StopCoroutine(revealRoutine);

            CancelInvoke(nameof(StartRound));
            targetCount = rng.Next(1, 21);
            tappedCount = 0;

            if (promptText) promptText.text = "How many apples do you see?";
            if (speechText) speechText.text = "Touch each apple and count with me!";
            if (feedbackText) feedbackText.text = "Tap every apple one time.";
            if (roundText) roundText.text = $"ROUND {round} / 5";

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
                yield return Pop(apple.transform, .82f, 1.05f, .12f);
            }
            if (speechText) speechText.text = "Your turn! Touch an apple to start at 1.";
            revealRoutine = null;
        }

        void TapApple(int index)
        {
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
                if (clip != null) numberAudioSource.PlayOneShot(clip);
            }
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

        void BuildAnswers()
        {
            var values = new HashSet<int> { targetCount };
            int guard = 0;
            while (values.Count < answerButtons.Length && guard++ < 100)
            {
                int candidate = Mathf.Clamp(targetCount + rng.Next(-4, 5), 1, 20);
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
            if (tappedCount < targetCount)
            {
                if (speechText) speechText.text = "Count all the apples first!";
                return;
            }

            if (value == targetCount)
            {
                SetAnswersInteractable(false);
                if (feedbackText) feedbackText.text = "Great counting!";
                if (speechText) speechText.text = $"Yes! The answer is {targetCount}!";
                GameProgressService.Instance?.AwardCorrect("counting");
                GameProgressService.Instance?.CompleteGame();
                if (journeyRect) StartCoroutine(CelebrateJourney());
                round = round >= 5 ? 1 : round + 1;
                Invoke(nameof(StartRound), 1.9f);
            }
            else
            {
                if (feedbackText) feedbackText.text = "Good try. Pick another number.";
                if (speechText) speechText.text = "Try again!";
                GameProgressService.Instance?.RegisterMiss();
            }
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
            foreach (var button in answerButtons)
                if (button) button.interactable = value;
        }

        void RefreshProgress()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;
            if (pointsText) pointsText.text = service.Progress.stars.ToString();
            if (levelText) levelText.text = $"LEVEL {service.Level}";
        }
    }
}
