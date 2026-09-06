using System;
using UnityEngine;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Central audio path for Journey's spoken lines.
    /// There is deliberately no device/system TTS fallback: if a Journey voice clip is not
    /// assigned, the reader stays silent rather than speaking in a different voice.
    /// </summary>
    public class JourneyVoicePlayerV1 : MonoBehaviour
    {
        [Serializable]
        public class VoiceLine
        {
            public string bookId;
            public int pageIndex;
            public AudioClip clip;
        }

        [SerializeField] AudioSource voiceSource;
        [SerializeField] VoiceLine[] pageLines = Array.Empty<VoiceLine>();
        [SerializeField] AudioClip welcomeClip;
        [SerializeField] AudioClip pageTurnClip;

        public bool IsSpeaking => voiceSource != null && voiceSource.isPlaying;

        void Awake()
        {
            if (voiceSource == null)
            {
                voiceSource = GetComponent<AudioSource>();
                if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
            }

            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.spatialBlend = 0f;
        }

        public void PlayWelcome()
        {
            PlayClip(welcomeClip);
        }

        public void PlayPage(string bookId, int pageIndex)
        {
            var clip = FindPageClip(bookId, pageIndex);
            if (clip == null)
            {
                Debug.Log($"Journey voice clip not assigned yet for {bookId} page {pageIndex + 1}. Reader will stay silent rather than use another voice.");
                return;
            }
            PlayClip(clip);
        }

        public void PlayPageTurn()
        {
            if (pageTurnClip == null) return;
            if (voiceSource == null) return;
            voiceSource.PlayOneShot(pageTurnClip, .45f);
        }

        public void StopSpeaking()
        {
            if (voiceSource != null) voiceSource.Stop();
        }

        public AudioClip FindPageClip(string bookId, int pageIndex)
        {
            if (pageLines == null) return null;
            string wanted = string.IsNullOrEmpty(bookId) ? "ABC" : bookId.Trim().ToUpperInvariant();
            foreach (var line in pageLines)
            {
                if (line == null || line.clip == null) continue;
                if (line.pageIndex != pageIndex) continue;
                if (string.Equals((line.bookId ?? "").Trim(), wanted, StringComparison.OrdinalIgnoreCase))
                    return line.clip;
            }
            return null;
        }

        void PlayClip(AudioClip clip)
        {
            if (clip == null || voiceSource == null) return;
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }
}
