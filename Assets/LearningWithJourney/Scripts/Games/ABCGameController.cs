using System;
using System.Collections.Generic;
using LearningWithJourney.Character;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class ABCGameController : MonoBehaviour
    {
        [Serializable]
        public class LetterQuestion
        {
            public string letter = "A";
            public string word = "Apple";
            public Sprite picture;
            public AudioClip prompt;
        }

        [SerializeField] JourneyAnimatorController journey;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text letterText;
        [SerializeField] Image pictureImage;
        [SerializeField] Button[] answerButtons;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] List<LetterQuestion> questions = new();

        LetterQuestion current;
        readonly System.Random rng = new();

        void Start() => NextQuestion();

        public void NextQuestion()
        {
            if (questions.Count == 0) return;
            current = questions[rng.Next(questions.Count)];
            if (promptText) promptText.text = current.picture ? $"Which letter starts {current.word}?" : $"Find the letter {current.letter}.";
            if (letterText) letterText.text = current.picture ? "" : current.letter;
            if (pictureImage)
            {
                pictureImage.enabled = current.picture != null;
                pictureImage.sprite = current.picture;
                pictureImage.preserveAspect = true;
            }
            if (feedbackText) feedbackText.text = "";
            BuildAnswers();
            journey?.PlayThink();
            if (current.prompt) journey?.Speak(current.prompt);
        }

        void BuildAnswers()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var answers = new HashSet<string> { current.letter.ToUpperInvariant() };
            while (answers.Count < answerButtons.Length)
                answers.Add(alphabet[rng.Next(alphabet.Length)].ToString());

            var list = new List<string>(answers);
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
                if (txt) txt.text=value;
                button.interactable=true;
            }
        }

        void Answer(string value)
        {
            foreach (var b in answerButtons) b.interactable=false;
            if (string.Equals(value,current.letter,StringComparison.OrdinalIgnoreCase))
            {
                if (feedbackText) feedbackText.text="You found it! ⭐";
                GameProgressService.Instance?.AwardCorrect("abc");
                journey?.PlayCelebrate();
            }
            else
            {
                if (feedbackText) feedbackText.text=$"Good try! The letter is {current.letter}.";
                GameProgressService.Instance?.RegisterMiss();
                journey?.PlayTryAgain();
            }
            GameProgressService.Instance?.CompleteGame();
            Invoke(nameof(NextQuestion),1.4f);
        }
    }
}
