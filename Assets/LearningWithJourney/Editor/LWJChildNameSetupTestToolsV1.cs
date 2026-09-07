#if UNITY_EDITOR
using LearningWithJourney.Core;
using UnityEditor;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJChildNameSetupTestToolsV1
    {
        const string SaveKey = "LWJ_PROGRESS_V1";

        [MenuItem("Learning with Journey/Reset Child Name Setup For Testing")]
        public static void ResetChildNameForTesting()
        {
            PlayerProgress progress = new PlayerProgress();

            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                var loaded = JsonUtility.FromJson<PlayerProgress>(json);
                if (loaded != null) progress = loaded;
            }

            progress.playerName = "Little Star";
            progress.hasPlayerName = false;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
            PlayerPrefs.Save();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "The saved child name was cleared for testing. Existing stars, coins, game totals, and learning progress were preserved.",
                "OK");
        }
    }
}
#endif
