using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class CountingWorldPlayControllerV2 : MonoBehaviour
    {
        [SerializeField] GameObject[] countObjects;
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] TMP_Text roundText;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] RectTransform journeyRect;

        readonly System.Random rng = new();
        int targetCount;
        int round = 1;
        Coroutine revealRoutine;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += RefreshProgress;

            RefreshProgress();
            StartRound();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= RefreshProgress;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");

        public void StartRound()
        {
            if (revealRoutine != null)
                StopCoroutine(revealRoutine);

            targetCount = rng.Next(1, 21);

            if (promptText) promptText.text = "How many apples do you see?";
            if (speechText) speechText.text = "Count the apples with me!";
            if (feedbackText) feedbackText.text = "";
            if (roundText) roundText.text = $"ROUND {round} / 5";

            // Make the target objects visible immediately so the child always has something to count.
            for (int i = 0; i < countObjects.Length; i++)
            {
                var go = countObjects[i];
                if (!go) continue;
                bool visible = i < targetCount;
                go.SetActive(visible);
                go.transform.localScale = visible ? Vector3.one * .90f : Vector3.zero;
            }

            BuildAnswers();
            SetAnswersInteractable(false);
            revealRoutine = StartCoroutine(CountObjectsTogether());
        }

        IEnumerator CountObjectsTogether()
        {
            yield return new WaitForSeconds(.30f);

            for (int i = 0; i < targetCount && i < countObjects.Length; i++)
            {
                var go = countObjects[i];
                if (!go) continue;

                if (speechText) speechText.text = (i + 1).ToString();
                yield return Pop(go.transform);
                yield return new WaitForSeconds(.045f);
            }

            if (speechText) speechText.text = "Now tap the right number!";
            SetAnswersInteractable(true);
            revealRoutine = null;
        }

        IEnumerator Pop(Transform target)
        {
            const float duration = .16f;
            float elapsed = 0f;
            Vector3 start = Vector3.one * .90f;
            Vector3 peak = Vector3.one * 1.16f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                target.localScale = p < .55f
                    ? Vector3.Lerp(start, peak, p / .55f)
                    : Vector3.Lerp(peak, Vector3.one, (p - .55f) / .45f);
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
            SetAnswersInteractable(false);

            if (value == targetCount)
            {
                if (feedbackText) feedbackText.text = "Great counting!";
                if (speechText) speechText.text = $"Yes! There are {targetCount} apples!";
                GameProgressService.Instance?.AwardCorrect("counting");
                if (journeyRect) StartCoroutine(CelebrateJourney());
            }
            else
            {
                if (feedbackText) feedbackText.text = $"Good try. There are {targetCount} apples.";
                if (speechText) speechText.text = "Let's count them one more time!";
                GameProgressService.Instance?.RegisterMiss();
            }

            GameProgressService.Instance?.CompleteGame();
            round = round >= 5 ? 1 : round + 1;
            Invoke(nameof(StartRound), 1.8f);
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
                float s = 1f + Mathf.Sin(p * Mathf.PI) * .035f;
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
