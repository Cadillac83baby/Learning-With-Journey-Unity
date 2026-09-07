using System;
using UnityEngine;

namespace LearningWithJourney.Core
{
    [Serializable]
    public class PlayerProgress
    {
        public int stars;
        public int coins;
        public int countingCorrect;
        public int abcCorrect;
        public int alphabetPairs;
        public int gamesCompleted;
        public int currentStreak;
        public int bestStreak;
        public string playerName = "Little Star";
        public bool hasPlayerName;
    }

    public class GameProgressService : MonoBehaviour
    {
        public static GameProgressService Instance { get; private set; }
        public PlayerProgress Progress { get; private set; } = new();
        public int Level => Mathf.FloorToInt(Progress.stars / 50f) + 1;
        public bool HasPlayerName => Progress != null && Progress.hasPlayerName && !string.IsNullOrWhiteSpace(Progress.playerName);
        public event Action OnProgressChanged;

        const string SaveKey = "LWJ_PROGRESS_V1";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void AwardCorrect(string category, int stars = 1, int coins = 5)
        {
            Progress.stars += stars;
            Progress.coins += coins;
            Progress.currentStreak++;
            Progress.bestStreak = Mathf.Max(Progress.bestStreak, Progress.currentStreak);
            switch (category)
            {
                case "counting": Progress.countingCorrect++; break;
                case "abc": Progress.abcCorrect++; break;
                case "match": Progress.alphabetPairs++; break;
            }
            Save();
        }

        public void RegisterMiss()
        {
            Progress.currentStreak = 0;
            Save();
        }

        public void CompleteGame()
        {
            Progress.gamesCompleted++;
            Save();
        }

        public void SetPlayerName(string value)
        {
            string cleaned = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            Progress.playerName = string.IsNullOrEmpty(cleaned) ? "Little Star" : cleaned;
            Progress.hasPlayerName = !string.IsNullOrEmpty(cleaned);
            Save();
        }

        public void AddReward(int stars, int coins)
        {
            Progress.stars += stars;
            Progress.coins += coins;
            Save();
        }

        public void ResetProgress()
        {
            Progress = new PlayerProgress();
            Save();
        }

        void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Progress));
            PlayerPrefs.Save();
            OnProgressChanged?.Invoke();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;
            var loaded = JsonUtility.FromJson<PlayerProgress>(PlayerPrefs.GetString(SaveKey));
            if (loaded != null) Progress = loaded;
        }
    }
}
