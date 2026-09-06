using System.Collections;
using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    public class RewardsScreenControllerV1 : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] TMP_Text speechText;
        [SerializeField] TMP_Text rewardProgressText;
        [SerializeField] TMP_Text prizeTitleText;
        [SerializeField] TMP_Text prizeAmountText;

        [Header("Treasure")]
        [SerializeField] Button openTreasureButton;
        [SerializeField] TMP_Text openTreasureButtonText;
        [SerializeField] RectTransform chestRoot;
        [SerializeField] RectTransform chestLid;
        [SerializeField] RectTransform prizeRoot;
        [SerializeField] CanvasGroup prizeCanvasGroup;
        [SerializeField] RectTransform[] sparkles;
        [SerializeField] Image[] rewardMarkers;

        [Header("Settings")]
        [SerializeField, Min(1)] int starsPerTreasure = 5;
        [SerializeField, Min(0)] int coinsPerTreasure = 25;

        const string ClaimedStarsKey = "LWJ_REWARDS_CLAIMED_STARS_V1";
        const string OpenedCountKey = "LWJ_REWARDS_OPENED_V1";

        readonly string[] prizeNames =
        {
            "GOLD STAR STICKER",
            "RAINBOW BADGE",
            "CROWN BADGE",
            "SUPER LEARNER TROPHY"
        };

        int claimedStars;
        int openedCount;
        bool busy;
        Vector2 lidClosedPosition;
        Quaternion lidClosedRotation;
        Vector2 prizeClosedPosition;

        void Start()
        {
            if (GameProgressService.Instance == null)
                new GameObject("GameProgressService").AddComponent<GameProgressService>();

            claimedStars = Mathf.Max(0, PlayerPrefs.GetInt(ClaimedStarsKey, 0));
            openedCount = Mathf.Max(0, PlayerPrefs.GetInt(OpenedCountKey, 0));

            if (openTreasureButton != null)
                openTreasureButton.onClick.AddListener(OpenTreasure);

            if (chestLid != null)
            {
                lidClosedPosition = chestLid.anchoredPosition;
                lidClosedRotation = chestLid.localRotation;
            }

            if (prizeRoot != null)
            {
                prizeClosedPosition = prizeRoot.anchoredPosition;
                prizeRoot.gameObject.SetActive(false);
            }

            SetSparkles(false);

            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged += Refresh;

            Refresh();
        }

        void OnDestroy()
        {
            if (GameProgressService.Instance != null)
                GameProgressService.Instance.OnProgressChanged -= Refresh;
        }

        public void GoHome() => SceneManager.LoadScene("MainMenu");
        public void GoLibrary() => SceneManager.LoadScene("Library");
        public void GoParents() => SceneManager.LoadScene("ParentZone");

        public void OpenTreasure()
        {
            if (busy || !TreasureAvailable()) return;
            StartCoroutine(OpenTreasureSequence());
        }

        IEnumerator OpenTreasureSequence()
        {
            busy = true;
            if (openTreasureButton != null) openTreasureButton.interactable = false;
            if (speechText != null) speechText.text = "Here it comes! Let's open your reward!";

            ResetTreasurePose();
            yield return ScaleTo(chestRoot, Vector3.one * 1.045f, .16f);
            yield return ScaleTo(chestRoot, Vector3.one, .12f);

            SetSparkles(true);
            yield return AnimateLidOpen(.40f);

            int prizeIndex = openedCount % prizeNames.Length;
            if (prizeTitleText != null) prizeTitleText.text = prizeNames[prizeIndex];
            if (prizeAmountText != null) prizeAmountText.text = "+" + coinsPerTreasure + " JOURNEY COINS";

            if (prizeRoot != null)
            {
                prizeRoot.gameObject.SetActive(true);
                prizeRoot.localScale = Vector3.zero;
                prizeRoot.anchoredPosition = prizeClosedPosition + new Vector2(0f, -22f);
            }
            if (prizeCanvasGroup != null) prizeCanvasGroup.alpha = 0f;

            float t = 0f;
            const float revealDuration = .56f;
            while (t < revealDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / revealDuration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                if (prizeRoot != null)
                {
                    prizeRoot.anchoredPosition = Vector2.Lerp(prizeClosedPosition + new Vector2(0f, -22f), prizeClosedPosition + new Vector2(0f, 102f), eased);
                    float overshoot = Mathf.Sin(p * Mathf.PI) * .12f;
                    prizeRoot.localScale = Vector3.one * Mathf.Lerp(0f, 1f + overshoot, eased);
                }
                if (prizeCanvasGroup != null) prizeCanvasGroup.alpha = eased;
                PulseSparkles(t);
                yield return null;
            }

            if (prizeRoot != null)
            {
                prizeRoot.anchoredPosition = prizeClosedPosition + new Vector2(0f, 102f);
                prizeRoot.localScale = Vector3.one;
            }
            if (prizeCanvasGroup != null) prizeCanvasGroup.alpha = 1f;

            ClaimTreasureProgress();
            GameProgressService.Instance?.AddReward(0, coinsPerTreasure);

            openedCount++;
            PlayerPrefs.SetInt(OpenedCountKey, openedCount);
            PlayerPrefs.SetInt(ClaimedStarsKey, claimedStars);
            PlayerPrefs.Save();

            if (speechText != null) speechText.text = "You did it! Your prize is yours!";
            Refresh();

            yield return new WaitForSeconds(1.2f);
            busy = false;
            if (openTreasureButton != null) openTreasureButton.interactable = TreasureAvailable();
        }

        void ClaimTreasureProgress()
        {
            int stars = CurrentStars();
            if (openedCount == 0)
            {
                claimedStars = stars;
                return;
            }

            claimedStars = Mathf.Min(stars, claimedStars + starsPerTreasure);
        }

        bool TreasureAvailable()
        {
            if (openedCount == 0) return true;
            return Mathf.Max(0, CurrentStars() - claimedStars) >= starsPerTreasure;
        }

        int CurrentStars()
        {
            return GameProgressService.Instance != null ? GameProgressService.Instance.Progress.stars : 0;
        }

        int CurrentProgress()
        {
            if (openedCount == 0) return starsPerTreasure;
            return Mathf.Clamp(CurrentStars() - claimedStars, 0, starsPerTreasure);
        }

        void Refresh()
        {
            var service = GameProgressService.Instance;
            if (service == null) return;

            var progress = service.Progress;
            if (pointsText != null) pointsText.text = progress.stars.ToString();
            if (coinsText != null) coinsText.text = progress.coins.ToString();
            if (levelText != null) levelText.text = "LEVEL " + service.Level;

            int current = CurrentProgress();
            if (rewardProgressText != null) rewardProgressText.text = current + " / " + starsPerTreasure;

            if (rewardMarkers != null)
            {
                for (int i = 0; i < rewardMarkers.Length; i++)
                {
                    if (rewardMarkers[i] == null) continue;
                    bool earned = i < current;
                    rewardMarkers[i].color = earned
                        ? new Color(1f, .72f, .08f, 1f)
                        : new Color(.42f, .25f, .68f, .55f);
                }
            }

            bool available = TreasureAvailable();
            if (openTreasureButton != null && !busy) openTreasureButton.interactable = available;
            if (openTreasureButtonText != null)
            {
                if (available)
                    openTreasureButtonText.text = "OPEN TREASURE";
                else
                {
                    int need = Mathf.Max(0, starsPerTreasure - current);
                    openTreasureButtonText.text = "EARN " + need + " MORE " + (need == 1 ? "STAR" : "STARS");
                }
            }

            if (!busy && speechText != null)
            {
                speechText.text = available
                    ? "Let's open your reward!"
                    : "Keep learning! Your next treasure is getting closer!";
            }
        }

        void ResetTreasurePose()
        {
            if (chestRoot != null) chestRoot.localScale = Vector3.one;
            if (chestLid != null)
            {
                chestLid.anchoredPosition = lidClosedPosition;
                chestLid.localRotation = lidClosedRotation;
            }
            if (prizeRoot != null)
            {
                prizeRoot.gameObject.SetActive(false);
                prizeRoot.anchoredPosition = prizeClosedPosition;
                prizeRoot.localScale = Vector3.zero;
            }
            if (prizeCanvasGroup != null) prizeCanvasGroup.alpha = 0f;
        }

        IEnumerator AnimateLidOpen(float duration)
        {
            if (chestLid == null) yield break;

            Vector2 startPos = lidClosedPosition;
            // Keep the lid visually connected to the chest. The earlier 82px lift made
            // it look detached on portrait phones.
            Vector2 endPos = lidClosedPosition + new Vector2(0f, 52f);
            Quaternion startRot = lidClosedRotation;
            Quaternion endRot = Quaternion.Euler(0f, 0f, -4f) * lidClosedRotation;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                chestLid.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                chestLid.localRotation = Quaternion.Slerp(startRot, endRot, p);
                yield return null;
            }

            chestLid.anchoredPosition = endPos;
            chestLid.localRotation = endRot;
        }

        IEnumerator ScaleTo(RectTransform target, Vector3 scale, float duration)
        {
            if (target == null) yield break;
            Vector3 start = target.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(start, scale, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration)));
                yield return null;
            }
            target.localScale = scale;
        }

        void SetSparkles(bool active)
        {
            if (sparkles == null) return;
            foreach (var sparkle in sparkles)
                if (sparkle != null) sparkle.gameObject.SetActive(active);
        }

        void PulseSparkles(float time)
        {
            if (sparkles == null) return;
            for (int i = 0; i < sparkles.Length; i++)
            {
                var sparkle = sparkles[i];
                if (sparkle == null) continue;
                float pulse = 1f + Mathf.Sin(time * 9f + i * 1.1f) * .28f;
                sparkle.localScale = Vector3.one * pulse;
                sparkle.localRotation = Quaternion.Euler(0f, 0f, time * 70f * (i % 2 == 0 ? 1f : -1f));
            }
        }
    }
}
