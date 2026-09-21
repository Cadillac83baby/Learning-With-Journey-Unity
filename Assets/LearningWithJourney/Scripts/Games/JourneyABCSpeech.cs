using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LearningWithJourney.Games
{
    public class JourneyABCSpeech : MonoBehaviour
    {
        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string VoiceFolder = "JourneyVoice/ABC/";

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] letterClips = new AudioClip[26];
        [SerializeField] AudioClip[] wordClips = new AudioClip[26];
        [SerializeField] AudioClip[] phraseClips = new AudioClip[26];
        AudioClip worldCompleteClip;
        readonly Queue<AudioClip> pendingClips = new Queue<AudioClip>();
        Coroutine playbackRoutine;
        Coroutine fallbackRoutine;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject tts;
        bool ttsReady;

        sealed class TtsInitListener : AndroidJavaProxy
        {
            readonly JourneyABCSpeech owner;

            public TtsInitListener(JourneyABCSpeech owner)
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

        public void SpeakLetter(int index, string letter)
        {
            AudioClip clip = GetClip(letterClips, index) ?? LoadLetterClip(index);
            if (clip != null)
            {
                PlayClip(clip);
                return;
            }

            SpeakFallback($"Letter {letter}");
        }

        public void SpeakWord(int index, string word)
        {
            AudioClip clip = GetClip(wordClips, index) ?? LoadWordClip(index);
            if (clip != null)
            {
                PlayClip(clip);
                return;
            }

            SpeakFallback(word);
        }

        public void SpeakPhrase(int index, string letter, string word)
        {
            AudioClip phrase = GetClip(phraseClips, index);
            if (phrase != null)
            {
                PlayClip(phrase);
                return;
            }

            AudioClip letterClip = GetClip(letterClips, index) ?? LoadLetterClip(index);
            AudioClip wordClip = GetClip(wordClips, index) ?? LoadWordClip(index);
            if (letterClip != null || wordClip != null)
            {
                StopVoicePlayback();
                StartCoroutine(PlayLetterWordSequence(letterClip, wordClip));
                return;
            }

            SpeakFallback($"{letter} is for {word}");
        }

public System.Collections.IEnumerator WaitForVoiceToFinish()
        {
            while (playbackRoutine != null ||
                   (audioSource != null && audioSource.isPlaying))
            {
                yield return null;
            }
        }

        public System.Collections.IEnumerator PlayRetryAndWait()
        {
            AudioClip retryClip =
                Resources.Load<AudioClip>(VoiceFolder + "ABC_Retry");

            if (retryClip == null)
            {
                Debug.LogWarning(
                    "Journey ABC retry audio was not found: Resources/" +
                    VoiceFolder + "ABC_Retry");
                yield break;
            }

            PlayClip(retryClip);
            yield return StartCoroutine(WaitForVoiceToFinish());
        }

        public System.Collections.IEnumerator SpeakPhraseAndWait(
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

            AudioClip letterClip =
                GetClip(letterClips, index) ?? LoadLetterClip(index);

            AudioClip wordClip =
                GetClip(wordClips, index) ?? LoadWordClip(index);

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
                SpeakFallback($"{letter} is for {word}");
        }

        public void SpeakPrompt(int index, string letter, string word)
        {
            SpeakFallback($"Can you find the letter {letter}? {letter} is for {word}.");
        }

        public void SpeakLevelComplete(int level)
        {
            SpeakFallback("Level " + level + " complete.");
        }

        public void SpeakWorldComplete()
        {
            worldCompleteClip ??= Resources.Load<AudioClip>(VoiceFolder + "ABC_level_Complete");
            if (worldCompleteClip != null)
            {
                PlayClip(worldCompleteClip);
                return;
            }

            SpeakFallback("Amazing. You finished all 10 ABC levels.");
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

            GameplayPromptAudioV1.StopScenePrompt();
            StopAndroidTts();
            pendingClips.Clear();

            if (playbackRoutine != null)
            {
                StopCoroutine(playbackRoutine);
                playbackRoutine = null;
            }

            audioSource.Stop();
            pendingClips.Enqueue(clip);
            playbackRoutine = StartCoroutine(DrainVoice());
        }

        System.Collections.IEnumerator DrainVoice()
        {
            while (GameplayPromptAudioV1.IsScenePromptPlaying)
                yield return null;
            while (pendingClips.Count > 0)
            {
                AudioClip clip = pendingClips.Dequeue();
                if (clip.loadState == AudioDataLoadState.Unloaded) clip.LoadAudioData();
                float timeout = 0f;
                while (clip.loadState == AudioDataLoadState.Loading && timeout < 2f)
                { timeout += Time.unscaledDeltaTime; yield return null; }
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.time = 0f;
                audioSource.Play();
                while (audioSource != null && audioSource.isPlaying) yield return null;
            }
            playbackRoutine = null;
        }

        void StopVoicePlayback()
        {
            pendingClips.Clear();

            if (playbackRoutine != null)
            {
                StopCoroutine(playbackRoutine);
                playbackRoutine = null;
            }

            if (audioSource != null)
                audioSource.Stop();
        }

        AudioClip GetClip(AudioClip[] clips, int index)
        {
            if (clips == null || index < 0 || index >= clips.Length) return null;
            return clips[index];
        }

        static AudioClip LoadLetterClip(int index)
        {
            if (index < 0 || index >= Alphabet.Length) return null;
            return Resources.Load<AudioClip>(VoiceFolder + "Letter_" + Alphabet[index]);
        }

        static AudioClip LoadWordClip(int index)
        {
            if (index < 0 || index >= Alphabet.Length) return null;
            return Resources.Load<AudioClip>(VoiceFolder + "Word_" + Alphabet[index]);
        }

        void SpeakFallback(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (fallbackRoutine != null) { StopCoroutine(fallbackRoutine); fallbackRoutine = null; }
            GameplayPromptAudioV1.StopScenePrompt();
            StopVoicePlayback();
            StopAndroidTts();

#if UNITY_ANDROID && !UNITY_EDITOR
            if (ttsReady && tts != null)
            {
                fallbackRoutine = StartCoroutine(SpeakFallbackAfterScenePrompt(text));
                return;
            }
#endif

#if UNITY_EDITOR
            Debug.Log("Journey ABC voice: " + text);
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
                Debug.LogWarning("Journey ABC text-to-speech could not initialize: " + ex.Message);
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
                Debug.LogWarning("Journey ABC text-to-speech setup failed: " + ex.Message);
            }
        }
#endif
    }
}
