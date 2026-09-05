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
        [SerializeField] float idleBreathCycleSeconds = 3.6f;
        [SerializeField] float talkPulseSeconds = 0.9f;

        // IMPORTANT: The temporary sprite atlas contains inconsistent alpha in several frames,
        // especially around the shorts/legs. Until the final skeletal rig is installed, the
        // main menu keeps one clean full-body frame on screen at all times and animates the
        // whole character transform instead of swapping body sprites.
        static readonly int StableFullBodyFrame = 0;

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
            CacheBaseTransform();

            if (characterImage != null && atlas != null)
            {
                characterImage.texture = atlas;
                characterImage.color = Color.white;
                characterImage.canvasRenderer.SetAlpha(1f);
                SetStableFrame();
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
            CacheBaseTransform();
            ResetVisualTransform();
            SetStableFrame();
            StartIdle();

            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        void OnDisable()
        {
            StopAllCharacterCoroutines();
            ResetVisualTransform();
            SetStableFrame();
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
            SetStableFrame();
            StartIdle();
        }

        public void PlayGreeting()
        {
            if (!isActiveAndEnabled) return;

            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);

            StopIdle();
            ResetVisualTransform();
            SetStableFrame();
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        public void Speak(AudioClip clip, string caption)
        {
            if (!isActiveAndEnabled) return;

            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);

            StopIdle();
            ResetVisualTransform();
            SetStableFrame();
            sequenceRoutine = StartCoroutine(SpeakExternalRoutine(clip, caption));
        }

        IEnumerator SpeakExternalRoutine(AudioClip clip, string caption)
        {
            ShowSpeech(caption);
            yield return SpeakRoutine(clip, 2.6f);
            HideSpeech();
            ResetVisualTransform();
            SetStableFrame();
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

            if (duration <= 0f)
                duration = 2.6f;

            yield return TalkForDuration(duration);
        }

        IEnumerator TalkForDuration(float duration)
        {
            SetStableFrame();

            if (rect == null)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float pulse = Mathf.Sin((elapsed / Mathf.Max(.1f, talkPulseSeconds)) * Mathf.PI * 2f);
                float scale = 1f + (pulse * 0.008f);
                float y = Mathf.Sin((elapsed / 1.8f) * Mathf.PI * 2f) * 1.8f;

                rect.localScale = baseScale * scale;
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, y);
                yield return null;
            }

            ResetVisualTransform();
            SetStableFrame();
        }

        public void Celebrate()
        {
            if (!isActiveAndEnabled) return;

            if (sequenceRoutine != null)
                StopCoroutine(sequenceRoutine);

            StopIdle();
            SetStableFrame();
            sequenceRoutine = StartCoroutine(CelebrateRoutine());
        }

        IEnumerator CelebrateRoutine()
        {
            yield return GentleBounce(1.0f, 7f);
            ResetVisualTransform();
            SetStableFrame();
            StartIdle();
        }

        void StartIdle()
        {
            StopIdle();
            idleRoutine = StartCoroutine(IdleBreathing());
        }

        void StopIdle()
        {
            if (idleRoutine != null)
            {
                StopCoroutine(idleRoutine);
                idleRoutine = null;
            }
        }

        IEnumerator IdleBreathing()
        {
            // No sprite-sheet blinking here. The final rig will blink eyelids independently.
            SetStableFrame();

            while (true)
            {
                if (rect == null)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                float elapsed = 0f;
                while (elapsed < idleBreathCycleSeconds)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / Mathf.Max(.1f, idleBreathCycleSeconds);
                    float y = Mathf.Sin(t * Mathf.PI * 2f) * 1.5f;
                    float scale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.004f;

                    rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, y);
                    rect.localScale = baseScale * scale;
                    yield return null;
                }

                ResetVisualTransform();
            }
        }

        IEnumerator GentleWave()
        {
            SetStableFrame();

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
            SetStableFrame();
        }

        IEnumerator GentlePoint()
        {
            SetStableFrame();

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
            SetStableFrame();
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

        void SetStableFrame()
        {
            SetFrame(StableFullBodyFrame);
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

        void CacheBaseTransform()
        {
            if (rect == null)
                rect = transform as RectTransform;

            if (rect == null) return;

            baseAnchoredPosition = rect.anchoredPosition;
            baseRotation = rect.localRotation;
            baseScale = rect.localScale;
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
            if (speechText != null)
                speechText.text = value;

            if (speechBubble != null)
                speechBubble.SetActive(true);
        }

        void HideSpeech()
        {
            if (speechBubble != null)
                speechBubble.SetActive(false);
        }
    }
}
