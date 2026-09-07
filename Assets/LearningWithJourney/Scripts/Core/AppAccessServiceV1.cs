using System;
using UnityEngine;

namespace LearningWithJourney.Core
{
    /// <summary>
    /// Local entitlement state for the pre-release build.
    /// The 3-day trial works now. Store purchase verification is intentionally
    /// exposed through MarkPurchasedFromStore so Google Play / App Store billing
    /// can call it later; no fake payment is performed in production builds.
    /// </summary>
    public class AppAccessServiceV1 : MonoBehaviour
    {
        public static AppAccessServiceV1 Instance { get; private set; }
        public const int TrialDays = 3;

        const string TrialStartKey = "LWJ_ACCESS_TRIAL_START_UTC_V1";
        const string PurchasedKey = "LWJ_ACCESS_PURCHASED_V1";

        public bool IsPurchased => PlayerPrefs.GetInt(PurchasedKey, 0) == 1;
        public bool TrialStarted => PlayerPrefs.HasKey(TrialStartKey);
        public DateTime TrialStartUtc => ReadTrialStart();
        public DateTime TrialEndUtc => TrialStartUtc.AddDays(TrialDays);
        public bool TrialActive => TrialStarted && DateTime.UtcNow < TrialEndUtc;
        public bool HasAccess => IsPurchased || TrialActive;
        public TimeSpan TrialRemaining => TrialActive ? TrialEndUtc - DateTime.UtcNow : TimeSpan.Zero;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool StartTrial()
        {
            if (IsPurchased) return true;
            if (TrialStarted) return TrialActive;

            PlayerPrefs.SetString(TrialStartKey, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>
        /// Call this only after the platform store reports a verified successful purchase.
        /// </summary>
        public void MarkPurchasedFromStore()
        {
            PlayerPrefs.SetInt(PurchasedKey, 1);
            PlayerPrefs.Save();
        }

        public void RestorePurchasedEntitlement(bool purchased)
        {
            if (!purchased) return;
            MarkPurchasedFromStore();
        }

#if UNITY_EDITOR
        public void GrantPurchaseForEditorTesting()
        {
            MarkPurchasedFromStore();
        }

        public void ResetAccessForEditorTesting()
        {
            PlayerPrefs.DeleteKey(TrialStartKey);
            PlayerPrefs.DeleteKey(PurchasedKey);
            PlayerPrefs.Save();
        }
#endif

        DateTime ReadTrialStart()
        {
            if (!PlayerPrefs.HasKey(TrialStartKey)) return DateTime.MinValue;
            string raw = PlayerPrefs.GetString(TrialStartKey, string.Empty);
            if (long.TryParse(raw, out long ticks))
            {
                try { return new DateTime(ticks, DateTimeKind.Utc); }
                catch { }
            }
            return DateTime.MinValue;
        }
    }
}
