using UnityEngine;

namespace LearningWithJourney.Core
{
    public static class GameLevelProgressV1
    {
        public const int TotalLevels = 10;

        static string LevelKey(string gameId)
        {
            return "LWJ.GameLevel." + gameId;
        }

        static string CompleteKey(string gameId)
        {
            return "LWJ.GameComplete." + gameId;
        }

        public static int GetResumeLevel(string gameId)
        {
            return Mathf.Clamp(
                PlayerPrefs.GetInt(LevelKey(gameId), 1),
                1,
                TotalLevels);
        }

        public static void BeginLevel(string gameId, int level)
        {
            PlayerPrefs.SetInt(
                LevelKey(gameId),
                Mathf.Clamp(level, 1, TotalLevels));

            PlayerPrefs.Save();
        }

        public static bool CompleteLevel(string gameId, int level)
        {
            if (level >= TotalLevels)
            {
                PlayerPrefs.SetInt(CompleteKey(gameId), 1);
                PlayerPrefs.SetInt(LevelKey(gameId), 1);
                PlayerPrefs.Save();
                return true;
            }

            PlayerPrefs.SetInt(LevelKey(gameId), level + 1);
            PlayerPrefs.Save();
            return false;
        }

        public static void ResetGame(string gameId)
        {
            PlayerPrefs.DeleteKey(LevelKey(gameId));
            PlayerPrefs.DeleteKey(CompleteKey(gameId));
            PlayerPrefs.Save();
        }

        public static void ResetAllGameProgress()
        {
            ResetGame("COUNTING");
            ResetGame("ABC");
            ResetGame("MATCHING");
            ResetGame("COLORS");
            ResetGame("STORY");
        }
    }
}