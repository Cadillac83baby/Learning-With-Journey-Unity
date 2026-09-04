using System;
using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Character;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class AlphabetMatchGameController : MonoBehaviour
    {
        [Serializable]
        public class MatchPair
        {
            public string letter = "A";
            public string word = "Apple";
            public Sprite picture;
        }

        [Serializable]
        public class CardView
        {
            public Button button;
            public GameObject back;
            public GameObject front;
            public TMP_Text letterText;
            public Image pictureImage;
            [NonSerialized] public string pairId;
            [NonSerialized] public bool matched;
            [NonSerialized] public bool showing;
        }

        [SerializeField] JourneyAnimatorController journey;
        [SerializeField] List<MatchPair> pairLibrary = new();
        [SerializeField] CardView[] cards;
        [SerializeField] TMP_Text pairProgressText;
        [SerializeField] TMP_Text moveText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField, Range(3,8)] int pairsPerRound = 6;

        readonly System.Random rng = new();
        CardView first;
        CardView second;
        bool locked;
        int matchedPairs;
        int moves;

        void Start() => NewRound();

        public void NewRound()
        {
            StopAllCoroutines();
            first=second=null; locked=false; matchedPairs=0; moves=0;
            var usablePairs=Mathf.Min(pairsPerRound,pairLibrary.Count,cards.Length/2);
            var pool=new List<MatchPair>(pairLibrary);
            Shuffle(pool);
            var deck=new List<(MatchPair pair,bool isLetter)>();
            for(int i=0;i<usablePairs;i++)
            {
                deck.Add((pool[i],true));
                deck.Add((pool[i],false));
            }
            Shuffle(deck);

            for(int i=0;i<cards.Length;i++)
            {
                var card=cards[i];
                card.button.onClick.RemoveAllListeners();
                card.matched=false; card.showing=false;
                card.button.gameObject.SetActive(i<deck.Count);
                if(i>=deck.Count) continue;

                var entry=deck[i];
                card.pairId=entry.pair.letter.ToUpperInvariant();
                card.letterText.gameObject.SetActive(entry.isLetter);
                card.pictureImage.gameObject.SetActive(!entry.isLetter);
                card.letterText.text=entry.pair.letter.ToUpperInvariant();
                card.pictureImage.sprite=entry.pair.picture;
                card.pictureImage.preserveAspect=true;
                Hide(card);
                var captured=card;
                card.button.onClick.AddListener(()=>Flip(captured));
            }
            UpdateHud(usablePairs);
            if(feedbackText) feedbackText.text="Tap two cards to find a match!";
            journey?.PlayWave();
        }

        void Flip(CardView card)
        {
            if(locked||card.matched||card.showing)return;
            Show(card);
            if(first==null)
            {
                first=card;
                journey?.PlayThink();
                return;
            }
            second=card;
            moves++;
            if(moveText) moveText.text=moves.ToString();
            if(first.pairId==second.pairId) StartCoroutine(Matched());
            else StartCoroutine(Missed());
        }

        IEnumerator Matched()
        {
            locked=true;
            first.matched=second.matched=true;
            matchedPairs++;
            GameProgressService.Instance?.AwardCorrect("match");
            if(feedbackText) feedbackText.text="Great match! ⭐";
            journey?.PlayCelebrate();
            UpdateHud(Mathf.Min(pairsPerRound,pairLibrary.Count,cards.Length/2));
            yield return new WaitForSeconds(.8f);
            first=second=null; locked=false;
            var total=Mathf.Min(pairsPerRound,pairLibrary.Count,cards.Length/2);
            if(matchedPairs>=total)
            {
                GameProgressService.Instance?.CompleteGame();
                if(feedbackText) feedbackText.text="Amazing! You matched them all!";
                journey?.PlayClap();
            }
        }

        IEnumerator Missed()
        {
            locked=true;
            GameProgressService.Instance?.RegisterMiss();
            if(feedbackText) feedbackText.text="Almost! Remember those cards.";
            journey?.PlayTryAgain();
            yield return new WaitForSeconds(.9f);
            Hide(first); Hide(second);
            first=second=null; locked=false;
        }

        void Show(CardView c){c.showing=true;c.back.SetActive(false);c.front.SetActive(true);}
        void Hide(CardView c){c.showing=false;c.back.SetActive(true);c.front.SetActive(false);}

        void UpdateHud(int total)
        {
            if(pairProgressText) pairProgressText.text=$"{matchedPairs}/{total} pairs";
            if(moveText) moveText.text=moves.ToString();
        }

        void Shuffle<T>(IList<T> list)
        {
            for(int i=0;i<list.Count;i++)
            {
                int j=rng.Next(i,list.Count); (list[i],list[j])=(list[j],list[i]);
            }
        }
    }
}
