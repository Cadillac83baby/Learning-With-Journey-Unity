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
        [SerializeField] float greetingDelay = 1.0f;
        [SerializeField] float talkFrameSeconds = 0.68f;
        [SerializeField] float idlePauseMin = 6.0f;
        [SerializeField] float idlePauseMax = 9.0f;
        [SerializeField] float blinkHalfSeconds = 0.15f;
        [SerializeField] float blinkClosedSeconds = 0.11f;

        // Only frames with a complete torso, shorts and legs are used on the menu.
        // The temporary atlas has missing-alpha holes in several other frames.
        static readonly int IdleOpen = 1;
        static readonly int BlinkClosed = 2;
        static readonly int BlinkHalf = 3;
        static readonly int WavePose = 5;
        static readonly int[] TalkFrames = { 10, 11, 10, 11 };
        static readonly int PointLikePose = 11;
        static readonly int CelebratePose = 10;

        const int AtlasColumns = 5;
        const int AtlasRows = 3;

        Coroutine idleRoutine;
        Coroutine sequenceRoutine;
        RectTransform rect;
        Vector2 baseAnchoredPosition;
        Quaternion baseRotation;
        Vector3 baseScale;

        void Awake()
        {
            rect = transform as RectTransform;
            if (rect != null)
            {
                baseAnchoredPosition = rect.anchoredPosition;
                baseRotation = rect.localRotation;
                baseScale = rect.localScale;
            }

            if (characterImage != null && atlas != null)
            {
                characterImage.texture = atlas;
                characterImage.color = Color.white;
                characterImage.canvasRenderer.SetAlpha(1f);
                SetFrame(IdleOpen);
            }

            if (voiceSource != null)
            {
                voiceSource.playOnAwake = false;
                voiceSource.loop = false;
            }

            if (speechBubble != null)
                speechBubble.SetActive(false);
        }

        void OnEnable()
        {
            ResetVisualTransform();
            StartIdle();
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        void OnDisable()
        {
            StopAllCharacterCoroutines();
            ResetVisualTransform();
        }

        IEnumerator OpeningSequence()
        {
            yield return new WaitForSeconds(greetingDelay);

            StopIdle();
            yield return GentleWave();

            ShowSpeech("Hi! I’m Journey! Let’s learn and have fun together!");
            AudioClip greeting = greetingClips != null && greetingClips.Length > 0 ? greetingClips[0] : null;

            if (greeting != null)
                yield return SpeakRoutine(greeting, 0f);
            else
                yield return TalkForDuration(3.2f);

            HideSpeech();
            yield return GentlePoint();
            ResetVisualTransform();
            StartIdle();
        }

        public void PlayGreeting()
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            StopIdle();
            ResetVisualTransform();
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        public void Speak(AudioClip clip, string caption)
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            StopIdle();
            ResetVisualTransform();
            sequenceRoutine = StartCoroutine(SpeakExternalRoutine(clip, caption));
        }

        IEnumerator SpeakExternalRoutine(AudioClip clip, string caption)
        {
            ShowSpeech(caption);
            yield return SpeakRoutine(clip, 2.6f);
            HideSpeech();
            ResetVisualTransform();
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

            if (duration <= 0f) duration = 2.6f;
            yield return TalkForDuration(duration);
        }

        IEnumerator TalkForDuration(float duration)
        {
            float elapsed = 0f;
            int index = 0;

            while (elapsed < duration)
            {
                SetFrame(TalkFrames[index]);
                index = (index + 1) % TalkFrames.Length;

                float hold = Mathf.Min(talkFrameSeconds, duration - elapsed);
                yield return new WaitForSeconds(hold);
                elapsed += hold;
            }

            SetFrame(IdleOpen);
        }

        public void Celebrate()
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            StopIdle();
            sequenceRoutine = StartCoroutine(CelebrateRoutine());
        }

        IEnumerator CelebrateRoutine()
        {
            SetFrame(CelebratePose);
            yield return GentleBounce(1.0f, 7f);
            SetFrame(IdleOpen);
            ResetVisualTransform();
            StartIdle();
        }

        void StartIdle()
        {
            StopIdle();
            idleRoutine = StartCoroutine(IdleBreathingAndBlinking());
        }

        void StopIdle()
        {
            if (idleRoutine != null)
            {
                StopCoroutine(idleRoutine);
                idleRoutine = null;
            }
        }

        IEnumerator IdleBreathingAndBlinking()
        {
            while (true)
            {
                SetFrame(IdleOpen);

                // Long calm pause so Journey feels alive without constantly blinking.
                float pause = Random.Range(idlePauseMin, idlePauseMax);
                float elapsed = 0f;
                while (elapsed < pause)
                {
                    float segment = Mathf.Min(1.8f, pause - elapsed);
                    yield return GentleBounce(segment, 1.6f);
                    elapsed += segment;
                }

                // One deliberate blink. No rapid multi-frame cycling.
                SetFrame(BlinkHalf);
                yield return new WaitForSeconds(blinkHalfSeconds);
                SetFrame(BlinkClosed);
                yield return new WaitForSeconds(blinkClosedSeconds);
                SetFrame(BlinkHalf);
                yield return new WaitForSeconds(blinkHalfSeconds);
                SetFrame(IdleOpen);
            }
        }

        IEnumerator GentleWave()
        {
            SetFrame(WavePose);
            if (rect == null)
            {
                yield return new WaitForSeconds(1.65f);
                yield break;
            }

            float duration = 1.65f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float angle = Mathf.Sin(t * Mathf.PI * 2f) * 1.2f;
                float lift = Mathf.Sin(t * Mathf.PI) * 3f;
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, lift);
                yield return null;
            }

            ResetVisualTransform();
            SetFrame(IdleOpen);
        }

        IEnumerator GentlePoint()
        {
            SetFrame(PointLikePose);
            if (rect == null)
            {
                yield return new WaitForSeconds(1.15f);
                yield break;
            }

            float duration = 1.15f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float nudge = Mathf.Sin(t * Mathf.PI) * 5f;
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(nudge, 0f);
                yield return null;
            }

            ResetVisualTransform();
            SetFrame(IdleOpen);
        }

        IEnumerator GentleBounce(float duration, float amount)
        {
            if (rect == null)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float y = Mathf.Sin(t * Mathf.PI * 2f) * amount;
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, y);
                yield return null;
            }

            rect.anchoredPosition = baseAnchoredPosition;
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
            characterImage.color = Color.white;
            characterImage.canvasRenderer.SetAlpha(1f);
        }

        void ResetVisualTransform()
        {
            if (rect == null) return;
            rect.anchoredPosition = baseAnchoredPosition;
            rect.localRotation = baseRotation;
            rect.localScale = baseScale;
        }

        void StopAllCharacterCoroutines()
        {
            StopIdle();
            if (sequenceRoutine != null)
            {
                StopCoroutine(sequenceRoutine);
                sequenceRoutine = null;
            }
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
