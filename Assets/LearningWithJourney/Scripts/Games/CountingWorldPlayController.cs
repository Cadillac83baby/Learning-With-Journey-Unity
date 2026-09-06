using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class CountingWorldPlayController : MonoBehaviour
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

        int targetCount;
        int round = 1;
        Coroutine countingRoutine;
        readonly System.Random rng = new();

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
            if (countingRoutine != null) StopCoroutine(countingRoutine);

            targetCount = rng.Next(1, 21);
            if (promptText) promptText.text = "How many apples do you see?";
            if (feedbackText) feedbackText.text = "";
            if (roundText) roundText.text = $"ROUND {round} / 5";

            for (int i = 0; i < countObjects.Length; i++)
            {
                if (!countObjects[i]) continue;
                countObjects[i].SetActive(false);
                countObjects[i].transform.localScale = Vector3.zero;
            }

            BuildAnswers();
            SetAnswersInteractable(false);
            countingRoutine = StartCoroutine(CountTogether());
        }

        IEnumerator CountTogether()
        {
            if (speechText) speechText.text = "Let's count together!";
            yield return new WaitForSeconds(.45f);

            for (int i = 0; i < targetCount && i < countObjects.Length; i++)
            {
                var go = countObjects[i];
                if (!go) continue;
                go.SetActive(true);
                if (speechText) speechText.text = (i + 1).ToString();
                yield return StartCoroutine(Pop(go.transform));
                yield return new WaitForSeconds(.10f);
            }

            if (speechText) speechText.text = "Now choose the number!";
            SetAnswersInteractable(true);
            countingRoutine = null;
        }

        IEnumerator Pop(Transform target)
        {
            float t = 0f;
            const float duration = .14f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float s = Mathf.Lerp(0f, 1.12f, p);
                target.localScale = Vector3.one * s;
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        void BuildAnswers()
        {
            var values = new HashSet<int> { targetCount };
            while (values.Count < answerButtons.Length)
            {
                int candidate = Mathf.Clamp(targetCount + rng.Next(-4, 5), 1, 20);
                values.Add(candidate);
            }

            var list = new List<int>(values);
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int value = list[i];
                var button = answerButtons[i];
                if (!button) continue;
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
                if (speechText) speechText.text = $"Yes! There are {targetCount}.";
                GameProgressService.Instance?.AwardCorrect("counting");
                if (journeyRect) StartCoroutine(CelebrateJourney());
            }
            else
            {
                if (feedbackText) feedbackText.text = $"Nice try. There are {targetCount}.";
                if (speechText) speechText.text = "Good try! Let's keep learning.";
                GameProgressService.Instance?.RegisterMiss();
            }

            GameProgressService.Instance?.CompleteGame();
            round = round >= 5 ? 1 : round + 1;
            Invoke(nameof(StartRound), 1.6f);
        }

        IEnumerator CelebrateJourney()
        {
            Vector2 start = journeyRect.anchoredPosition;
            float t = 0f;
            while (t < .42f)
            {
                t += Time.deltaTime;
                float y = Mathf.Sin((t / .42f) * Mathf.PI) * 14f;
                journeyRect.anchoredPosition = start + new Vector2(0f, y);
                yield return null;
            }
            journeyRect.anchoredPosition = start;
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
