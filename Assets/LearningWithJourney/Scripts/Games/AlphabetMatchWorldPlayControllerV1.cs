using System.Collections;
using System.Collections.Generic;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class AlphabetMatchWorldPlayControllerV1 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text feedbackText;
        [SerializeField] TMP_Text pairProgressText;
        [SerializeField] TMP_Text moveText;
        [SerializeField] TMP_Text roundText;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text levelText;

        [Header("Cards")]
        [SerializeField] Button[] cardButtons;
        [SerializeField] GameObject[] cardBacks;
        [SerializeField] GameObject[] cardFronts;
        [SerializeField] TMP_Text[] cardLetterTexts;
        [SerializeField] Image[] cardPictureImages;
        [SerializeField] TMP_Text[] cardWordTexts;
        [SerializeField] RectTransform[] cardRects;
        [SerializeField] Sprite[] pictures = new Sprite[26];

        [Header("Journey")]
        [SerializeField] RectTransform journeyRect;
        [SerializeField] JourneyAlphabetMatchSpeech journeySpeech;

        [Header("Progression")]
        [SerializeField, Range(1, 10)] int totalLevels = 10;
        [SerializeField, Range(1, 10)] int roundsPerLevel = 5;

        const string LevelKey = "LWJ_MATCH_LEVEL_V1";
        const string RoundKey = "LWJ_MATCH_ROUND_V1";
        const string CompleteKey = "LWJ_MATCH_COMPLETE_V1";
        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static readonly string[] Words =
        {
            "Apple", "Ball", "Cat", "Dog", "Elephant", "Fish", "Grapes", "Hat", "Ice Cream",
            "Juice", "Kite", "Lion", "Moon", "Nest", "Owl", "Pig", "Queen", "Rainbow",
            "Sun", "Turtle", "Umbrella", "Violin", "Watermelon", "Xylophone", "Yo-Yo", "Zebra"
        };

        enum MatchMode { LetterPicture, UpperLower }

        class DeckEntry
        {
            public int alphabetIndex;
            public bool secondSide;
        }

        readonly System.Random rng = new System.Random();
        string[] pairIds;
        int[] alphabetIndices;
        bool[] pictureSide;
        bool[] lowercaseSide;
        bool[] matched;
        bool[] showing;

        int currentLevel;
        int round;
        int matchedPairs;
        int moves;
        int firstIndex = -1;
        int secondIndex = -1;
        int activeCardCount;
        int activePairCount;
        bool locked;
        bool worldCompleted;
        MatchMode currentMode;

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

            AllocateCardState();
            RefreshPoints();
            UpdateProgressHud();

            if (worldCompleted)
                ShowCompletedState();
            else
                StartRound();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= RefreshPoints;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");

        public void ResetMatchProgress()
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

        void AllocateCardState()
        {
            int count = cardButtons != null ? cardButtons.Length : 0;
            pairIds = new string[count];
            alphabetIndices = new int[count];
            pictureSide = new bool[count];
            lowercaseSide = new bool[count];
            matched = new bool[count];
            showing = new bool[count];
        }

        public void StartRound()
        {
            if (worldCompleted) return;

            StopAllCoroutines();
            locked = false;
            matchedPairs = 0;
            moves = 0;
            firstIndex = secondIndex = -1;
            currentMode = currentLevel == 9 ? MatchMode.UpperLower : MatchMode.LetterPicture;
            activePairCount = PairCountForLevel(currentLevel);
            activeCardCount = Mathf.Min(activePairCount * 2, cardButtons != null ? cardButtons.Length : 0);

            BuildDeck();
            LayoutCards(activeCardCount);
            UpdateProgressHud();

            if (currentMode == MatchMode.UpperLower)
            {
                if (promptText) promptText.text = "Match each BIG letter with its little letter!";
                if (speechText) speechText.text = "Match uppercase letters with lowercase letters.";
                journeySpeech?.SpeakPrompt("Match each big letter with its little letter.");
            }
            else
            {
                if (promptText) promptText.text = "Match each letter to its picture!";
                if (speechText) speechText.text = "Tap two cards and find the matching pair.";
                journeySpeech?.SpeakPrompt("Match each letter to its picture.");
            }

            if (feedbackText) feedbackText.text = LevelInstruction(currentLevel);
        }

        void BuildDeck()
        {
            var chosen = ChoosePairIndices(currentLevel, activePairCount);
            var deck = new List<DeckEntry>();
            foreach (int index in chosen)
            {
                deck.Add(new DeckEntry { alphabetIndex = index, secondSide = false });
                deck.Add(new DeckEntry { alphabetIndex = index, secondSide = true });
            }
            Shuffle(deck);

            for (int i = 0; i < cardButtons.Length; i++)
            {
                var button = cardButtons[i];
                if (button == null) continue;

                button.onClick.RemoveAllListeners();
                bool active = i < activeCardCount;
                button.gameObject.SetActive(active);
                if (!active) continue;

                var entry = deck[i];
                int alphabetIndex = entry.alphabetIndex;
                pairIds[i] = Alphabet[alphabetIndex].ToString();
                alphabetIndices[i] = alphabetIndex;
                matched[i] = false;
                showing[i] = false;

                if (currentMode == MatchMode.UpperLower)
                {
                    pictureSide[i] = false;
                    lowercaseSide[i] = entry.secondSide;
                }
                else
                {
                    pictureSide[i] = entry.secondSide;
                    lowercaseSide[i] = false;
                }

                ConfigureCardFront(i);
                HideCard(i);
                int captured = i;
                button.onClick.AddListener(() => FlipCard(captured));
                button.interactable = true;
            }
        }

        void ConfigureCardFront(int i)
        {
            int alphabetIndex = alphabetIndices[i];
            string letter = Alphabet[alphabetIndex].ToString();

            if (cardLetterTexts != null && i < cardLetterTexts.Length && cardLetterTexts[i] != null)
            {
                bool showLetter = !pictureSide[i];
                cardLetterTexts[i].gameObject.SetActive(showLetter);
                cardLetterTexts[i].text = lowercaseSide[i] ? letter.ToLowerInvariant() : letter;
            }

            if (cardPictureImages != null && i < cardPictureImages.Length && cardPictureImages[i] != null)
            {
                cardPictureImages[i].gameObject.SetActive(pictureSide[i]);
                cardPictureImages[i].sprite = pictures != null && alphabetIndex < pictures.Length ? pictures[alphabetIndex] : null;
                cardPictureImages[i].preserveAspect = true;
                cardPictureImages[i].enabled = pictureSide[i] && cardPictureImages[i].sprite != null;
            }

            if (cardWordTexts != null && i < cardWordTexts.Length && cardWordTexts[i] != null)
            {
                cardWordTexts[i].gameObject.SetActive(pictureSide[i]);
                cardWordTexts[i].text = Words[alphabetIndex];
            }
        }

        void FlipCard(int index)
        {
            if (locked || index < 0 || index >= activeCardCount || matched[index] || showing[index]) return;

            ShowCard(index);
            SpeakCard(index);

            if (firstIndex < 0)
            {
                firstIndex = index;
                return;
            }

            secondIndex = index;
            moves++;
            UpdateProgressHud();

            bool samePair = pairIds[firstIndex] == pairIds[secondIndex];
            bool differentSides = pictureSide[firstIndex] != pictureSide[secondIndex] || lowercaseSide[firstIndex] != lowercaseSide[secondIndex];

            if (samePair && differentSides)
                StartCoroutine(HandleMatch());
            else
                StartCoroutine(HandleMiss());
        }

        IEnumerator HandleMatch()
        {
            locked = true;
            matched[firstIndex] = matched[secondIndex] = true;
            if (cardButtons[firstIndex]) cardButtons[firstIndex].interactable = false;
            if (cardButtons[secondIndex]) cardButtons[secondIndex].interactable = false;

            matchedPairs++;
            int alphabetIndex = alphabetIndices[firstIndex];
            string letter = Alphabet[alphabetIndex].ToString();
            string word = Words[alphabetIndex];

            GameProgressService.Instance?.AwardCorrect("match");

            if (currentMode == MatchMode.UpperLower)
            {
                if (feedbackText) feedbackText.text = "Great match! " + letter + " matches " + letter.ToLowerInvariant() + ".";
                if (speechText) speechText.text = "Great match! Uppercase " + letter + " and lowercase " + letter.ToLowerInvariant() + ".";
                journeySpeech?.SpeakCaseMatch(letter);
            }
            else
            {
                if (feedbackText) feedbackText.text = "Great match! " + letter + " is for " + word + ".";
                if (speechText) speechText.text = letter + " is for " + word + ".";
                journeySpeech?.SpeakPair(alphabetIndex, letter, word);
            }

            if (journeyRect) StartCoroutine(CelebrateJourney());
            UpdateProgressHud();

            yield return new WaitForSeconds(.8f);
            firstIndex = secondIndex = -1;
            locked = false;

            if (matchedPairs >= activePairCount)
                StartCoroutine(CompleteRound());
        }

        IEnumerator HandleMiss()
        {
            locked = true;
            GameProgressService.Instance?.RegisterMiss();
            if (feedbackText) feedbackText.text = "Almost! Remember the cards and try again.";
            if (speechText) speechText.text = "Almost. Try again!";
            journeySpeech?.SpeakTryAgain();

            yield return new WaitForSeconds(.9f);
            HideCard(firstIndex);
            HideCard(secondIndex);
            firstIndex = secondIndex = -1;
            locked = false;
        }

        IEnumerator CompleteRound()
        {
            locked = true;
            GameProgressService.Instance?.CompleteGame();
            if (feedbackText) feedbackText.text = "Amazing! You matched them all!";
            if (speechText) speechText.text = "Great job! You matched every pair.";
            journeySpeech?.SpeakRoundComplete();
            if (journeyRect) StartCoroutine(CelebrateJourney());

            yield return new WaitForSeconds(1.5f);

            if (round < roundsPerLevel)
            {
                round++;
                SaveProgress();
                StartRound();
                yield break;
            }

            if (currentLevel < totalLevels)
            {
                int finished = currentLevel;
                currentLevel++;
                round = 1;
                SaveProgress();
                GameProgressService.Instance?.AddReward(3, 15);
                if (speechText) speechText.text = "Level " + finished + " complete!";
                journeySpeech?.SpeakLevelComplete(finished);
                yield return new WaitForSeconds(1.1f);
                StartRound();
                yield break;
            }

            worldCompleted = true;
            PlayerPrefs.SetInt(CompleteKey, 1);
            PlayerPrefs.SetInt(LevelKey, totalLevels);
            PlayerPrefs.SetInt(RoundKey, roundsPerLevel);
            PlayerPrefs.Save();
            GameProgressService.Instance?.AddReward(10, 50);
            ShowCompletedState();
        }

        void SpeakCard(int index)
        {
            if (journeySpeech == null || index < 0 || index >= activeCardCount) return;
            int alphabetIndex = alphabetIndices[index];
            string letter = Alphabet[alphabetIndex].ToString();

            if (pictureSide[index])
            {
                if (speechText) speechText.text = Words[alphabetIndex] + ".";
                journeySpeech.SpeakWord(alphabetIndex, Words[alphabetIndex]);
            }
            else if (lowercaseSide[index])
            {
                if (speechText) speechText.text = "Lowercase " + letter.ToLowerInvariant() + ".";
                journeySpeech.SpeakLowercase(letter);
            }
            else
            {
                if (speechText) speechText.text = "Letter " + letter + ".";
                journeySpeech.SpeakLetter(alphabetIndex, letter);
            }
        }

        void ShowCard(int index)
        {
            if (index < 0 || index >= activeCardCount) return;
            showing[index] = true;
            if (cardBacks != null && index < cardBacks.Length && cardBacks[index]) cardBacks[index].SetActive(false);
            if (cardFronts != null && index < cardFronts.Length && cardFronts[index]) cardFronts[index].SetActive(true);
        }

        void HideCard(int index)
        {
            if (index < 0 || index >= activeCardCount) return;
            showing[index] = false;
            if (cardBacks != null && index < cardBacks.Length && cardBacks[index]) cardBacks[index].SetActive(true);
            if (cardFronts != null && index < cardFronts.Length && cardFronts[index]) cardFronts[index].SetActive(false);
        }

        void LayoutCards(int count)
        {
            if (cardRects == null) return;
            int rows = Mathf.Max(1, Mathf.CeilToInt(count / 2f));
            float cellH;
            float gapY;
            if (rows == 2) { cellH = .37f; gapY = .08f; }
            else if (rows == 3) { cellH = .27f; gapY = .045f; }
            else { cellH = .21f; gapY = .025f; }

            float totalH = rows * cellH + (rows - 1) * gapY;
            float bottom = (1f - totalH) * .5f;
            const float left = .025f;
            const float right = .975f;
            const float gapX = .035f;
            float cellW = (right - left - gapX) * .5f;

            for (int i = 0; i < count && i < cardRects.Length; i++)
            {
                var rect = cardRects[i];
                if (!rect) continue;
                int rowIndex = i / 2;
                int col = i % 2;
                float xMin = left + col * (cellW + gapX);
                float xMax = xMin + cellW;
                float yMax = 1f - bottom - rowIndex * (cellH + gapY);
                float yMin = yMax - cellH;
                rect.anchorMin = new Vector2(xMin, yMin);
                rect.anchorMax = new Vector2(xMax, yMax);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }
        }

        List<int> ChoosePairIndices(int level, int count)
        {
            GetLetterRangeForLevel(level, out int minIndex, out int maxIndex);
            var pool = new List<int>();
            for (int i = minIndex; i <= maxIndex; i++) pool.Add(i);
            Shuffle(pool);
            if (pool.Count > count) pool.RemoveRange(count, pool.Count - count);
            return pool;
        }

        int PairCountForLevel(int level)
        {
            if (level <= 3) return 2;
            if (level <= 6) return 3;
            return 4;
        }

        void GetLetterRangeForLevel(int level, out int minIndex, out int maxIndex)
        {
            switch (Mathf.Clamp(level, 1, 10))
            {
                case 1: minIndex = 0; maxIndex = 3; break;    // A-D
                case 2: minIndex = 4; maxIndex = 7; break;    // E-H
                case 3: minIndex = 8; maxIndex = 11; break;   // I-L
                case 4: minIndex = 12; maxIndex = 17; break;  // M-R
                case 5: minIndex = 18; maxIndex = 23; break;  // S-X
                case 6: minIndex = 0; maxIndex = 11; break;   // A-L review
                case 7: minIndex = 12; maxIndex = 25; break;  // M-Z review
                default: minIndex = 0; maxIndex = 25; break;  // full alphabet review
            }
        }

        string LevelInstruction(int level)
        {
            if (level <= 3) return "Level " + level + ": find 2 matching pairs.";
            if (level <= 6) return "Level " + level + ": find 3 matching pairs.";
            if (level <= 8) return "Level " + level + ": find 4 matching pairs.";
            if (level == 9) return "Level 9: match uppercase and lowercase letters.";
            return "Level 10: mixed letter-to-picture challenge.";
        }

        void UpdateProgressHud()
        {
            if (levelText) levelText.text = "LEVEL " + currentLevel + " / " + totalLevels;
            if (roundText) roundText.text = "ROUND " + round + " / " + roundsPerLevel;
            if (pairProgressText) pairProgressText.text = "PAIRS " + matchedPairs + " / " + Mathf.Max(1, activePairCount);
            if (moveText) moveText.text = "MOVES " + moves;
        }

        void RefreshPoints()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;
            if (pointsText) pointsText.text = service.Progress.stars.ToString();
        }

        void SaveProgress()
        {
            PlayerPrefs.SetInt(LevelKey, currentLevel);
            PlayerPrefs.SetInt(RoundKey, round);
            PlayerPrefs.SetInt(CompleteKey, worldCompleted ? 1 : 0);
            PlayerPrefs.Save();
            UpdateProgressHud();
        }

        void ShowCompletedState()
        {
            worldCompleted = true;
            currentLevel = totalLevels;
            round = roundsPerLevel;
            locked = true;
            if (promptText) promptText.text = "ALPHABET MATCH COMPLETE!";
            if (speechText) speechText.text = "Amazing! You finished all 10 matching levels!";
            if (feedbackText) feedbackText.text = "You are an Alphabet Match superstar!";
            if (pairProgressText) pairProgressText.text = "ALL PAIRS COMPLETE";
            if (moveText) moveText.text = "GREAT JOB";
            if (levelText) levelText.text = "LEVEL 10 / 10";
            if (roundText) roundText.text = "WORLD COMPLETE";
            journeySpeech?.SpeakWorldComplete();
        }

        IEnumerator CelebrateJourney()
        {
            Vector2 start = journeyRect.anchoredPosition;
            Vector3 startScale = journeyRect.localScale;
            float elapsed = 0f;
            const float duration = .52f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                float lift = Mathf.Sin(p * Mathf.PI) * 18f;
                float scale = 1f + Mathf.Sin(p * Mathf.PI) * .045f;
                journeyRect.anchoredPosition = start + new Vector2(0f, lift);
                journeyRect.localScale = startScale * scale;
                yield return null;
            }

            journeyRect.anchoredPosition = start;
            journeyRect.localScale = startScale;
        }

        void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
