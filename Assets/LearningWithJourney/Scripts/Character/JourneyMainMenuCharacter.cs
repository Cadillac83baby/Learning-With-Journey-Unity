using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Character
{
    public class JourneyMainMenuCharacter : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] RawImage characterImage;
        [SerializeField] Texture2D atlas;
        [SerializeField] TMP_Text speechText;
        [SerializeField] GameObject speechBubble;

        [Header("Voice")]
        [SerializeField] AudioSource voiceSource;
        [SerializeField] AudioClip[] greetingClips;

        [Header("Animation")]
        [SerializeField] float idleFps = 2.2f;
        [SerializeField] float actionFps = 5.5f;
        [SerializeField] float greetingDelay = 0.8f;

        static readonly int[] IdleFrames = { 0, 1, 2, 3 };
        static readonly int[] WaveFrames = { 4, 5, 6 };
        static readonly int[] TalkFrames = { 7, 8, 9, 10, 11 };
        static readonly int[] CelebrateFrames = { 12 };
        static readonly int[] PointFrames = { 13 };

        const int AtlasColumns = 5;
        const int AtlasRows = 3;

        Coroutine animationRoutine;
        Coroutine sequenceRoutine;

        void Awake()
        {
            if (characterImage != null && atlas != null)
            {
                characterImage.texture = atlas;
                characterImage.color = Color.white;
                SetFrame(IdleFrames[0]);
            }

            if (speechBubble != null)
                speechBubble.SetActive(false);
        }

        void OnEnable()
        {
            StartIdle();
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        void OnDisable()
        {
            if (animationRoutine != null) StopCoroutine(animationRoutine);
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        }

        IEnumerator OpeningSequence()
        {
            yield return new WaitForSeconds(greetingDelay);

            yield return PlayAction(WaveFrames, 1.15f);
            ShowSpeech("Hi! I’m Journey! Let’s learn and have fun together!");

            AudioClip greeting = greetingClips != null && greetingClips.Length > 0 ? greetingClips[0] : null;
            if (greeting != null)
            {
                yield return SpeakRoutine(greeting, 0f);
            }
            else
            {
                yield return PlayAction(TalkFrames, 2.25f);
            }

            HideSpeech();
            yield return PlayAction(PointFrames, 0.8f);
            StartIdle();
        }

        public void PlayGreeting()
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        public void Speak(AudioClip clip, string caption)
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(SpeakExternalRoutine(clip, caption));
        }

        IEnumerator SpeakExternalRoutine(AudioClip clip, string caption)
        {
            ShowSpeech(caption);
            yield return SpeakRoutine(clip, 1.75f);
            HideSpeech();
            StartIdle();
        }

        IEnumerator SpeakRoutine(AudioClip clip, float fallbackDuration)
        {
            float duration = fallbackDuration;
            if (clip != null && voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.Play();
                duration = clip.length;
            }

            if (duration <= 0f) duration = 1.75f;
            yield return PlayAction(TalkFrames, duration);
        }

        public void Celebrate()
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(CelebrateRoutine());
        }

        IEnumerator CelebrateRoutine()
        {
            yield return PlayAction(CelebrateFrames, 0.85f);
            StartIdle();
        }

        void StartIdle()
        {
            if (animationRoutine != null) StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(LoopFrames(IdleFrames, idleFps));
        }

        IEnumerator PlayAction(int[] frames, float duration)
        {
            if (animationRoutine != null) StopCoroutine(animationRoutine);
            animationRoutine = StartCoroutine(LoopFrames(frames, actionFps));
            yield return new WaitForSeconds(duration);
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }
        }

        IEnumerator LoopFrames(int[] frames, float fps)
        {
            if (frames == null || frames.Length == 0) yield break;
            float wait = 1f / Mathf.Max(1f, fps);
            int index = 0;

            while (true)
            {
                SetFrame(frames[index]);
                index = (index + 1) % frames.Length;
                yield return new WaitForSeconds(wait);
            }
        }

        void SetFrame(int frameIndex)
        {
            if (characterImage == null || atlas == null) return;

            int column = frameIndex % AtlasColumns;
            int rowFromTop = frameIndex / AtlasColumns;
            float width = 1f / AtlasColumns;
            float height = 1f / AtlasRows;
            float u = column * width;
            float v = 1f - ((rowFromTop + 1) * height);

            characterImage.uvRect = new Rect(u, v, width, height);
        }

        void ShowSpeech(string value)
        {
            if (speechText != null) speechText.text = value;
            if (speechBubble != null) speechBubble.SetActive(true);
        }

        void HideSpeech()
        {
            if (speechBubble != null) speechBubble.SetActive(false);
        }
    }
}
