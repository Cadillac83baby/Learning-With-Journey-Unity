using System;
using System.Collections.Generic;
using LearningWithJourney.Character;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class CountingGameController : MonoBehaviour
    {
        [Serializable]
        public class CountingQuestion
        {
            public string label;
            public Sprite objectSprite;
            [Range(1,20)] public int count = 3;
            public AudioClip prompt;
        }

        [SerializeField] JourneyAnimatorController journey;
        [SerializeField] Transform objectGrid;
        [SerializeField] Image objectPrefab;
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] List<CountingQuestion> questions = new();

        CountingQuestion current;
        readonly System.Random rng = new();

        void Start() => NextQuestion();

        public void NextQuestion()
        {
            if (questions.Count == 0) return;
            current = questions[rng.Next(questions.Count)];
            BuildObjects();
            BuildAnswers();
            if (promptText) promptText.text = $"How many {current.label} do you see?";
            if (feedbackText) feedbackText.text = "";
            journey?.PlayThink();
            if (current.prompt) journey?.Speak(current.prompt);
        }

        void BuildObjects()
        {
            foreach (Transform child in objectGrid) Destroy(child.gameObject);
            for (int i=0;i<current.count;i++)
            {
                var image = Instantiate(objectPrefab, objectGrid);
                image.sprite = current.objectSprite;
                image.preserveAspect = true;
            }
        }

        void BuildAnswers()
        {
            var answers = new HashSet<int> { current.count };
            while (answers.Count < answerButtons.Length)
            {
                var value = Mathf.Clamp(current.count + rng.Next(-4,5), 1, 20);
                answers.Add(value);
            }
            var list = new List<int>(answers);
            for (int i=0;i<list.Count;i++)
            {
                int swap=rng.Next(i,list.Count); (list[i],list[swap])=(list[swap],list[i]);
            }
            for (int i=0;i<answerButtons.Length;i++)
            {
                var button=answerButtons[i];
                var value=list[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(()=>Answer(value));
                var txt=button.GetComponentInChildren<TMP_Text>();
                if (txt) txt.text=value.ToString();
                button.interactable=true;
            }
        }

        void Answer(int value)
        {
            foreach (var b in answerButtons) b.interactable=false;
            if (value == current.count)
            {
                if (feedbackText) feedbackText.text="Great counting! ⭐";
                GameProgressService.Instance?.AwardCorrect("counting");
                journey?.PlayCelebrate();
            }
            else
            {
                if (feedbackText) feedbackText.text=$"Nice try! There are {current.count}.";
                GameProgressService.Instance?.RegisterMiss();
                journey?.PlayTryAgain();
            }
            GameProgressService.Instance?.CompleteGame();
            Invoke(nameof(NextQuestion),1.4f);
        }
    }
}
