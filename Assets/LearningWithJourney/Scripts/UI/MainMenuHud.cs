using LearningWithJourney.Core;
using TMPro;
using UnityEngine;

namespace LearningWithJourney.UI
{
    public class MainMenuHud : MonoBehaviour
    {
        [SerializeField] TMP_Text starText;
        [SerializeField] TMP_Text coinText;
        [SerializeField] TMP_Text playerText;
        [SerializeField] TMP_Text levelText;

        void Start()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= Refresh;
        }

        public void Refresh()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            var progress = service.Progress;
            if (starText != null) starText.text = progress.stars.ToString();
            if (coinText != null) coinText.text = progress.coins.ToString();
            if (playerText != null) playerText.text = $"Hi, {progress.playerName}!";
            if (levelText != null) levelText.text = $"Level {service.Level}";
        }
    }
}
