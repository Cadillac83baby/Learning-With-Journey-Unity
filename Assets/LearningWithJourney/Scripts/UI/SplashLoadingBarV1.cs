using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Fills the branded loading bar during the local startup/loading screen.
    /// This is intentionally a visual startup indicator; the current launch
    /// flow loads local scenes and does not expose a remote-download byte count.
    /// </summary>
    public sealed class SplashLoadingBarV1 : MonoBehaviour
    {
        [SerializeField] Image progressFill;
        [SerializeField] TMP_Text progressLabel;
        [SerializeField] float fillDurationSeconds = 2.15f;

        const int MaxDisplayedPercent = 99;

        void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(FillBar());
        }

        IEnumerator FillBar()
        {
            if (progressFill == null) yield break;

            progressFill.fillAmount = 0f;
            SetProgress(0);
            float duration = Mathf.Max(.25f, fillDurationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                int percent = Mathf.Clamp(Mathf.FloorToInt(normalized * MaxDisplayedPercent), 0, MaxDisplayedPercent);
                SetProgress(percent);
                yield return null;
            }
            SetProgress(MaxDisplayedPercent);
        }

        void SetProgress(int percent)
        {
            percent = Mathf.Clamp(percent, 0, MaxDisplayedPercent);
            if (progressFill != null)
            {
                // Leave a tiny unfilled edge so the display intentionally stops
                // at 99% until the splash scene transitions away.
                float normalized = percent / (float)MaxDisplayedPercent;
                progressFill.fillAmount = Mathf.SmoothStep(0f, .99f, normalized);
            }
            if (progressLabel != null)
                progressLabel.text = percent + "%";
        }
    }
}
