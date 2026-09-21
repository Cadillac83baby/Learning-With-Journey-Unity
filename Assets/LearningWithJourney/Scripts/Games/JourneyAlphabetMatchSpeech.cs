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
        AudioClip correctFeedbackClip;
        AudioClip retryClip;

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
            correctFeedbackClip = Resources.Load<AudioClip>("JourneyVoice/MATCH/MATCH_correct");
            if (correctFeedbackClip == null)
                correctFeedbackClip = Resources.Load<AudioClip>("JourneyVoice/MATCH/MATCH-correct");
            if (correctFeedbackClip == null)
                correctFeedbackClip = Resources.Load<AudioClip>("JourneyVoice/ABC/Praise_Great");
            retryClip = Resources.Load<AudioClip>("JourneyVoice/MATCH/MATCH_retry");
            if (retryClip == null)
                retryClip = Resources.Load<AudioClip>("JourneyVoice/ABC/ABC_Retry");
            LoadLetterWordClips();
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

void LoadLetterWordClips()
        {
            if (letterClips == null || letterClips.Length != 26)
                letterClips = new AudioClip[26];

            if (wordClips == null || wordClips.Length != 26)
                wordClips = new AudioClip[26];

            if (phraseClips == null || phraseClips.Length != 26)
                phraseClips = new AudioClip[26];

            int lettersLoaded = 0;
            int wordsLoaded = 0;
            int phrasesLoaded = 0;

            for (int i = 0; i < 26; i++)
            {
                string letter = ((char)('A' + i)).ToString();

                if (letterClips[i] == null)
                    letterClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/ABC/Letter_" + letter);

                if (letterClips[i] == null)
                    letterClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/MATCH/Letter_" + letter);

                if (wordClips[i] == null)
                    wordClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/ABC/Word_" + letter);

                if (wordClips[i] == null)
                    wordClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/MATCH/Word_" + letter);

                if (phraseClips[i] == null)
                    phraseClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/ABC/Phrase_" + letter);

                if (phraseClips[i] == null)
                    phraseClips[i] =
                        Resources.Load<AudioClip>(
                            "JourneyVoice/MATCH/Phrase_" + letter);

                if (letterClips[i] != null)
                    lettersLoaded++;

                if (wordClips[i] != null)
                    wordsLoaded++;

                if (phraseClips[i] != null)
                    phrasesLoaded++;
            }

            retryClip =
                Resources.Load<AudioClip>(
                    "JourneyVoice/MATCH/MATCH_Retry") ??
                Resources.Load<AudioClip>(
                    "JourneyVoice/MATCH/MATCH_retry") ??
                Resources.Load<AudioClip>(
                    "JourneyVoice/ABC/ABC_Retry");

#if UNITY_EDITOR
            Debug.Log(
                "Alphabet Match audio loaded: letters " +
                lettersLoaded + "/26, words " +
                wordsLoaded + "/26, phrases " +
                phrasesLoaded + "/26, retry " +
                (retryClip != null ? "yes" : "no"));
#endif
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

public System.Collections.IEnumerator WaitForVoiceToFinish()
        {
            while (audioSource != null && audioSource.isPlaying)
                yield return null;
        }

        IEnumerator WaitForFallbackVoice(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            yield return StartCoroutine(WaitForVoiceToFinish());
        }

        public System.Collections.IEnumerator SpeakCaseMatchAndWait(
            string letter)
        {
            int index = LetterIndex(letter);
            AudioClip clip = GetClip(caseClips, index);

            if (clip != null)
            {
                PlayClip(clip);
                yield return StartCoroutine(WaitForVoiceToFinish());
                yield break;
            }

            SpeakCaseMatch(letter);
            yield return StartCoroutine(WaitForFallbackVoice(1.4f));
        }

        public System.Collections.IEnumerator SpeakLowercaseAndWait(
            string letter)
        {
            int index = LetterIndex(letter);
            AudioClip clip = GetClip(lowercaseClips, index);

            if (clip == null && index >= 0)
            {
                clip = Resources.Load<AudioClip>(
                    "JourneyVoice/ABC/Lower_" +
                    letter.ToUpperInvariant());
            }

            if (clip != null)
            {
                PlayClip(clip);
                yield return StartCoroutine(WaitForVoiceToFinish());
                yield break;
            }

            SpeakLowercase(letter);
            yield return StartCoroutine(WaitForFallbackVoice(1.35f));
        }

        public System.Collections.IEnumerator SpeakPairAndWait(
            int index,
            string letter,
            string word)
        {
            AudioClip phrase = GetClip(phraseClips, index);

            if (phrase != null)
            {
                PlayClip(phrase);
                yield return StartCoroutine(WaitForVoiceToFinish());
                yield break;
            }

            AudioClip letterClip = GetClip(letterClips, index);
            AudioClip wordClip = GetClip(wordClips, index);

            if (letterClip != null)
            {
                PlayClip(letterClip);
                yield return StartCoroutine(WaitForVoiceToFinish());
            }

            if (wordClip != null)
            {
                PlayClip(wordClip);
                yield return StartCoroutine(WaitForVoiceToFinish());
            }

            if (letterClip == null && wordClip == null)
            {
                SpeakFallback(letter + " is for " + word);
                yield return StartCoroutine(WaitForFallbackVoice(1.5f));
            }
        }

public System.Collections.IEnumerator SpeakCorrectAndWait()
{
    if (correctFeedbackClip != null)
    {
        PlayClip(correctFeedbackClip);
        yield return StartCoroutine(WaitForVoiceToFinish());
        yield break;
    }

    SpeakFallback("Great job! You found a match.");
    yield return StartCoroutine(WaitForFallbackVoice(.75f));
}

public System.Collections.IEnumerator SpeakTryAgainAndWait()
        {
            SpeakTryAgain();

            if (retryClip != null)
            {
                yield return StartCoroutine(
                    WaitForVoiceToFinish());
            }
            else
            {
                yield return new WaitForSecondsRealtime(.85f);
            }
        }

        public System.Collections.IEnumerator SpeakRoundCompleteAndWait()
        {
            SpeakRoundComplete();
            yield return StartCoroutine(WaitForFallbackVoice(1.65f));
        }

        public System.Collections.IEnumerator SpeakLevelCompleteAndWait(
            int level)
        {
            SpeakLevelComplete(level);
            yield return StartCoroutine(WaitForFallbackVoice(1.15f));
        }

public void SpeakTryAgain()
        {
            if (retryClip != null)
            {
                PlayClip(retryClip);
                return;
            }

            SpeakFallback("Almost. Try again.");
        }
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
