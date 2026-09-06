using System.Collections;
using UnityEngine;

namespace LearningWithJourney.Games
{
    public class JourneyABCSpeech : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] letterClips = new AudioClip[26];
        [SerializeField] AudioClip[] wordClips = new AudioClip[26];
        [SerializeField] AudioClip[] phraseClips = new AudioClip[26];

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
            AudioClip clip = GetClip(letterClips, index);
            if (clip != null)
            {
                PlayClip(clip);
                return;
            }

            SpeakFallback($"Letter {letter}");
        }

        public void SpeakWord(int index, string word)
        {
            AudioClip clip = GetClip(wordClips, index);
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

            AudioClip letterClip = GetClip(letterClips, index);
            AudioClip wordClip = GetClip(wordClips, index);
            if (letterClip != null || wordClip != null)
            {
                StopAllCoroutines();
                StartCoroutine(PlayLetterWordSequence(letterClip, wordClip));
                return;
            }

            SpeakFallback($"{letter} is for {word}");
        }

        public void SpeakPrompt(int index, string letter, string word)
        {
            SpeakFallback($"Can you find the letter {letter}? {letter} is for {word}.");
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
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }

        AudioClip GetClip(AudioClip[] clips, int index)
        {
            if (clips == null || index < 0 || index >= clips.Length) return null;
            return clips[index];
        }

        void SpeakFallback(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (ttsReady && tts != null)
            {
                tts.Call<int>("speak", text, 0, null, "LWJ_ABC");
                return;
            }
#endif

#if UNITY_EDITOR
            Debug.Log("Journey ABC voice: " + text);
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
