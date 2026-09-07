using System;
using UnityEngine;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Journey-only narration path for the Library book readers.
    ///
    /// Audio is resolved from Resources using this convention:
    ///   Resources/JourneyVoice/Common/welcome
    ///   Resources/JourneyVoice/Common/finish
    ///   Resources/JourneyVoice/ABC/01 ... 26
    ///   Resources/JourneyVoice/NUMBERS/01 ... 20
    ///   Resources/JourneyVoice/COLORS/01 ... 12
    ///   Resources/JourneyVoice/STORY/01 ... 10
    ///
    /// There is intentionally NO system/device TTS fallback. If the Journey
    /// narration clip does not exist, the screen remains silent instead of
    /// substituting another voice.
    /// </summary>
    public class JourneyVoicePlayerV2 : MonoBehaviour
    {
        [Serializable]
        public class VoiceOverride
        {
            public string bookId;
            public int pageIndex;
            public AudioClip clip;
        }

        [SerializeField] AudioSource voiceSource;
        [SerializeField] AudioSource effectsSource;
        [SerializeField] VoiceOverride[] overrides = Array.Empty<VoiceOverride>();
        [SerializeField] AudioClip welcomeOverride;
        [SerializeField] AudioClip finishOverride;
        [SerializeField] AudioClip pageTurnSound;
        [Range(0f, 1f)] [SerializeField] float voiceVolume = 1f;
        [Range(0f, 1f)] [SerializeField] float effectsVolume = .38f;

        public bool IsSpeaking => voiceSource != null && voiceSource.isPlaying;

        void Awake()
        {
            if (voiceSource == null)
            {
                voiceSource = GetComponent<AudioSource>();
                if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
            }

            if (effectsSource == null)
            {
                var fx = transform.Find("PageTurnAudio");
                if (fx == null)
                {
                    var fxGo = new GameObject("PageTurnAudio");
                    fxGo.transform.SetParent(transform, false);
                    effectsSource = fxGo.AddComponent<AudioSource>();
                }
                else
                {
                    effectsSource = fx.GetComponent<AudioSource>();
                    if (effectsSource == null) effectsSource = fx.gameObject.AddComponent<AudioSource>();
                }
            }

            ConfigureSource(voiceSource, voiceVolume);
            ConfigureSource(effectsSource, effectsVolume);
        }

        static void ConfigureSource(AudioSource source, float volume)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = volume;
        }

        public void PlayWelcome()
        {
            AudioClip clip = welcomeOverride != null
                ? welcomeOverride
                : Resources.Load<AudioClip>("JourneyVoice/Common/welcome");
            PlayVoice(clip, "Journey welcome narration");
        }

        public void PlayFinish()
        {
            AudioClip clip = finishOverride != null
                ? finishOverride
                : Resources.Load<AudioClip>("JourneyVoice/Common/finish");
            PlayVoice(clip, "Journey finish narration");
        }

        public void PlayPage(string bookId, int pageIndex)
        {
            string normalized = NormalizeBookId(bookId);
            AudioClip clip = FindOverride(normalized, pageIndex);
            if (clip == null)
                clip = Resources.Load<AudioClip>($"JourneyVoice/{normalized}/{pageIndex + 1:00}");

            PlayVoice(clip, $"Journey narration for {normalized} page {pageIndex + 1}");
        }

        public bool HasPageClip(string bookId, int pageIndex)
        {
            string normalized = NormalizeBookId(bookId);
            if (FindOverride(normalized, pageIndex) != null) return true;
            return Resources.Load<AudioClip>($"JourneyVoice/{normalized}/{pageIndex + 1:00}") != null;
        }

        public void PlayPageTurn()
        {
            if (effectsSource == null) return;
            AudioClip clip = pageTurnSound != null
                ? pageTurnSound
                : Resources.Load<AudioClip>("JourneyVoice/Common/page_turn");
            if (clip != null) effectsSource.PlayOneShot(clip, effectsVolume);
        }

        public void StopSpeaking()
        {
            if (voiceSource != null) voiceSource.Stop();
        }

        AudioClip FindOverride(string bookId, int pageIndex)
        {
            if (overrides == null) return null;
            foreach (var entry in overrides)
            {
                if (entry == null || entry.clip == null || entry.pageIndex != pageIndex) continue;
                if (string.Equals(NormalizeBookId(entry.bookId), bookId, StringComparison.OrdinalIgnoreCase))
                    return entry.clip;
            }
            return null;
        }

        void PlayVoice(AudioClip clip, string description)
        {
            if (clip == null)
            {
                Debug.Log($"{description} is not installed yet. Learning with Journey will stay silent rather than use a different voice.");
                return;
            }

            if (voiceSource == null) return;
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();
        }

        static string NormalizeBookId(string id)
        {
            string value = string.IsNullOrWhiteSpace(id) ? "ABC" : id.Trim().ToUpperInvariant();
            switch (value)
            {
                case "NUMBER":
                case "COUNTING":
                    return "NUMBERS";
                case "COLOR":
                case "SHAPES":
                    return "COLORS";
                case "STORYTIME":
                case "STORY TIME":
                    return "STORY";
                default:
                    return value;
            }
        }
    }
}
