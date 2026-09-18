using System.Collections;
using UnityEngine;

namespace LearningWithJourney.Games
{
    public class JourneyAlphabetMatchSpeech : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] letterClips = new AudioClip[26];
        [SerializeField] AudioClip[] wordClips = new AudioClip[26];
        [SerializeField] AudioClip[] phraseClips = new AudioClip[26];
        // New prompts for the Uppercase-to-Lowercase matching level.
        [SerializeField] AudioClip[] caseClips = new AudioClip[26];
        [SerializeField] AudioClip[] lowercaseClips = new AudioClip[26];
        AudioClip worldCompleteClip;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject tts;
        bool ttsReady;

        sealed class TtsInitListener : AndroidJavaProxy
        {
            readonly JourneyAlphabetMatchSpeech owner;

            public TtsInitListener(JourneyAlphabetMatchSpeech owner)
                : base("android.speech.tts.TextToSpeech$OnInitListener")
            {
                this.owner = owner;
            }

            public void onInit(int status)
            {
                owner.HandleTtsInit(status);
            }
        }
#endif

        void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            LoadCaseClips();
            LoadLowercaseClips();
            worldCompleteClip = Resources.Load<AudioClip>("JourneyVoice/MATCH/Alphabet_Match_World_Complete");

#if UNITY_ANDROID && !UNITY_EDITOR
            InitializeAndroidTts();
#endif
        }

        void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (tts != null)
            {
                tts.Call("stop");
                tts.Call("shutdown");
                tts.Dispose();
                tts = null;
            }
#endif
        }

        public void SpeakPrompt(string text) => SpeakFallback(text);

        public void SpeakLetter(int index, string letter)
        {
            var clip = GetClip(letterClips, index);
            if (clip != null) { PlayClip(clip); return; }
            SpeakFallback("Letter " + letter);
        }

        public void SpeakLowercase(string letter)
        {
            int index = LetterIndex(letter);
            var clip = GetClip(lowercaseClips, index);
            if (clip == null && index >= 0)
                clip = Resources.Load<AudioClip>("JourneyVoice/ABC/Lower_" + letter.ToUpperInvariant());
            if (clip != null) { PlayClip(clip); return; }
            SpeakFallback("Lowercase " + letter.ToLowerInvariant());
        }

        public void SpeakWord(int index, string word)
        {
            var clip = GetClip(wordClips, index);
            if (clip != null) { PlayClip(clip); return; }
            SpeakFallback(word);
        }

        public void SpeakPair(int index, string letter, string word)
        {
            var phrase = GetClip(phraseClips, index);
            if (phrase != null) { PlayClip(phrase); return; }

            var letterClip = GetClip(letterClips, index);
            var wordClip = GetClip(wordClips, index);
            if (letterClip != null || wordClip != null)
            {
                StopAllCoroutines();
                StartCoroutine(PlayLetterWordSequence(letterClip, wordClip));
                return;
            }

            SpeakFallback(letter + " is for " + word);
        }

        public void SpeakCaseMatch(string letter)
        {
            int index = LetterIndex(letter);
            var clip = GetClip(caseClips, index);
            if (clip != null) { PlayClip(clip); return; }
            SpeakFallback("Great match. Uppercase " + letter + " matches lowercase " + letter.ToLowerInvariant());
        }

        void LoadCaseClips()
        {
            if (caseClips == null || caseClips.Length != 26)
                caseClips = new AudioClip[26];

            int loaded = 0;
            for (int i = 0; i < 26; i++)
            {
                if (caseClips[i] != null) continue;
                string letter = ((char)('A' + i)).ToString();
                caseClips[i] = Resources.Load<AudioClip>("JourneyVoice/ABC/Case_" + letter);
                if (caseClips[i] != null) loaded++;
            }
#if UNITY_EDITOR
            if (loaded > 0) Debug.Log("Alphabet Match case prompts loaded: " + loaded + "/26 clips.");
#endif
        }

        void LoadLowercaseClips()
        {
            if (lowercaseClips == null || lowercaseClips.Length != 26)
                lowercaseClips = new AudioClip[26];

            for (int i = 0; i < 26; i++)
            {
                if (lowercaseClips[i] != null) continue;
                string letter = ((char)('A' + i)).ToString();
                lowercaseClips[i] = Resources.Load<AudioClip>("JourneyVoice/ABC/Lower_" + letter);
            }
        }

        static int LetterIndex(string letter)
        {
            if (string.IsNullOrEmpty(letter)) return -1;
            char c = char.ToUpperInvariant(letter[0]);
            return c >= 'A' && c <= 'Z' ? c - 'A' : -1;
        }

        public void SpeakTryAgain() => SpeakFallback("Almost. Try again.");
        public void SpeakRoundComplete() => SpeakFallback("Great job. You matched them all.");
        public void SpeakLevelComplete(int level) => SpeakFallback("Level " + level + " complete.");
        public void SpeakWorldComplete()
        {
            worldCompleteClip ??= Resources.Load<AudioClip>("JourneyVoice/MATCH/Alphabet_Match_World_Complete");
            if (worldCompleteClip != null) { PlayClip(worldCompleteClip); return; }
            SpeakFallback("Amazing. You finished Alphabet Match World.");
        }

        IEnumerator PlayLetterWordSequence(AudioClip letterClip, AudioClip wordClip)
        {
            if (letterClip != null)
            {
                PlayClip(letterClip);
                yield return new WaitForSeconds(letterClip.length + .08f);
            }
            if (wordClip != null)
                PlayClip(wordClip);
        }

        void PlayClip(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            StopAndroidTts();
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }

        static AudioClip GetClip(AudioClip[] clips, int index)
        {
            if (clips == null || index < 0 || index >= clips.Length) return null;
            return clips[index];
        }

        void SpeakFallback(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // A new fallback prompt must never overlap a clip or older TTS voice.
            if (audioSource != null) audioSource.Stop();
            StopAndroidTts();

#if UNITY_ANDROID && !UNITY_EDITOR
            if (ttsReady && tts != null)
            {
                tts.Call<int>("speak", text, 0, null, "LWJ_MATCH");
                return;
            }
#endif

#if UNITY_EDITOR
            Debug.Log("Journey Alphabet Match voice: " + text);
#endif
        }

        void StopAndroidTts()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (tts != null)
            {
                try { tts.Call("stop"); }
                catch (System.Exception) { }
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void InitializeAndroidTts()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    tts = new AndroidJavaObject(
                        "android.speech.tts.TextToSpeech",
                        activity,
                        new TtsInitListener(this));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Alphabet Match speech could not initialize: " + ex.Message);
            }
        }

        void HandleTtsInit(int status)
        {
            if (status != 0 || tts == null) return;
            try
            {
                ttsReady = true;
                using (var locale = new AndroidJavaObject("java.util.Locale", "en", "US"))
                    tts.Call<int>("setLanguage", locale);
                tts.Call<int>("setSpeechRate", .90f);
                tts.Call<int>("setPitch", 1.08f);
            }
            catch (System.Exception ex)
            {
                ttsReady = false;
                Debug.LogWarning("Alphabet Match speech setup failed: " + ex.Message);
            }
        }
#endif
    }
}
