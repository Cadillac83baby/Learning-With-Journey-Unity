using System.Collections;
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
        [SerializeField] float fillDurationSeconds = 2.15f;

        void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(FillBar());
        }

        IEnumerator FillBar()
        {
            if (progressFill == null) yield break;

            progressFill.fillAmount = 0f;
            float duration = Mathf.Max(.25f, fillDurationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                progressFill.fillAmount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            progressFill.fillAmount = 1f;
        }
    }
}
