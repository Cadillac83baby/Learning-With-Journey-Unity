using LearningWithJourney.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.UI
{
    public class AccessGateControllerV1 : MonoBehaviour
    {
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text trialDetailText;
        [SerializeField] string splashScene = "Splash";

        void Start()
        {
            EnsureService();
            var service = AppAccessServiceV1.Instance;
            if (service == null) return;

            if (service.HasAccess)
            {
                SceneManager.LoadScene(splashScene);
                return;
            }

            RefreshCopy();
        }

        public void StartThreeDayTrial()
        {
            EnsureService();
            var service = AppAccessServiceV1.Instance;
            if (service == null) return;

            if (service.TrialStarted && !service.TrialActive)
            {
                SetStatus("Your 3-day trial has ended. Purchase the full game to continue.");
                return;
            }

            if (service.StartTrial())
            {
                SetStatus("Your 3-day free trial is ready!");
                SceneManager.LoadScene(splashScene);
            }
        }

        public void PurchaseFullGame()
        {
            EnsureService();
#if UNITY_EDITOR
            // Editor-only convenience so the complete launch flow can be tested now.
            AppAccessServiceV1.Instance?.GrantPurchaseForEditorTesting();
            SetStatus("Editor test purchase granted.");
            SceneManager.LoadScene(splashScene);
#else
            // Production builds must connect this button to Google Play Billing / StoreKit.
            SetStatus("Store purchase connection will be enabled before release.");
#endif
        }

        public void RestorePurchases()
        {
#if UNITY_EDITOR
            SetStatus("Restore purchase testing will use the current local entitlement.");
#else
            SetStatus("Restore Purchases will connect to the platform store before release.");
#endif
        }

        /// <summary>
        /// Platform billing code can call this after a verified $0.99 purchase succeeds.
        /// </summary>
        public void CompleteVerifiedStorePurchase()
        {
            EnsureService();
            AppAccessServiceV1.Instance?.MarkPurchasedFromStore();
            SceneManager.LoadScene(splashScene);
        }

        void RefreshCopy()
        {
            var service = AppAccessServiceV1.Instance;
            if (service == null) return;

            if (trialDetailText != null)
            {
                if (!service.TrialStarted)
                    trialDetailText.text = "Try the full learning game free for 3 days.";
                else if (service.TrialActive)
                    trialDetailText.text = "Trial active.";
                else
                    trialDetailText.text = "Your free trial has ended.";
            }

            SetStatus("Choose your access option to continue.");
        }

        void EnsureService()
        {
            if (AppAccessServiceV1.Instance == null)
                new GameObject("AppAccessService").AddComponent<AppAccessServiceV1>();
        }

        void SetStatus(string value)
        {
            if (statusText != null) statusText.text = value;
        }
    }
}
