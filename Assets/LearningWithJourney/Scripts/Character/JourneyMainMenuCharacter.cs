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
        [SerializeField] float talkFrameSeconds = 0.9f;
        [SerializeField] float idleBreathSeconds = 2.6f;

        // The temporary atlas is not a true skeletal rig. Some blink/action frames change
        // body shading or contain alpha artifacts around the shorts/legs. To keep Journey's
        // body visually stable on the Main Menu we hold a clean complete-body frame for idle
        // and create most motion with the RectTransform instead of rapidly swapping artwork.
        static readonly int IdleStable = 1;
        static readonly int WaveStable = 4;
        static readonly int TalkOpenA = 7;
        static readonly int TalkOpenB = 8;

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
                SetFrame(IdleStable);
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
            SetFrame(IdleStable);
            StartIdle();

            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(OpeningSequence());
        }

        void OnDisable()
        {
            StopAllCharacterCoroutines();
            ResetVisualTransform();
            SetFrame(IdleStable);
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
            SetFrame(IdleStable);
            yield return GentlePointNudge();
            ResetVisualTransform();
            StartIdle();
        }

        public void PlayGreeting()
        {
            if (!isActiveAndEnabled) return;
            if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
            StopIdle();
            ResetVisualTransform();
            SetFrame(IdleStable);
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
            yield return SpeakRoutine(clip, 2.8f);
            HideSpeech();
            SetFrame(IdleStable);
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

            if (duration <= 0f) duration = 2.8f;
            yield return TalkForDuration(duration);
        }

        IEnumerator TalkForDuration(float duration)
        {
            float elapsed = 0f;
            bool alternate = false;

            while (elapsed < duration)
            {
                SetFrame(alternate ? TalkOpenA : TalkOpenB);
                alternate = !alternate;

                float hold = Mathf.Min(talkFrameSeconds, duration - elapsed);
                yield return GentleSpeakingMotion(hold);
                elapsed += hold;
            }

            SetFrame(IdleStable);
            ResetVisualTransform();
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
            SetFrame(IdleStable);
            yield return GentleBounce(1.1f, 10f);
            ResetVisualTransform();
            StartIdle();
        }

        void StartIdle()
        {
            StopIdle();
            SetFrame(IdleStable);
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
            // No atlas-based blink on this temporary sprite sheet. Switching the entire
            // frame to blink was also changing the shorts/body and creating the fade the
            // user could see. A real facial blink will return when Journey is on the rig.
            while (true)
            {
                SetFrame(IdleStable);
                yield return GentleBounce(idleBreathSeconds, 1.8f);
            }
        }

        IEnumerator GentleWave()
        {
            SetFrame(WaveStable);

            if (rect == null)
            {
                yield return new WaitForSeconds(1.7f);
                SetFrame(IdleStable);
                yield break;
            }

            float duration = 1.7f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float angle = Mathf.Sin(t * Mathf.PI * 2f) * 1.1f;
                float lift = Mathf.Sin(t * Mathf.PI) * 3f;
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, lift);
                yield return null;
            }

            ResetVisualTransform();
            SetFrame(IdleStable);
        }

        IEnumerator GentlePointNudge()
        {
            SetFrame(IdleStable);
            if (rect == null)
            {
                yield return new WaitForSeconds(0.9f);
                yield break;
            }

            float duration = 0.9f;
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
        }

        IEnumerator GentleSpeakingMotion(float duration)
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
                float y = Mathf.Sin(t * Mathf.PI) * 2.4f;
                float angle = Mathf.Sin(t * Mathf.PI * 2f) * 0.45f;
                rect.anchoredPosition = baseAnchoredPosition + new Vector2(0f, y);
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            ResetVisualTransform();
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
