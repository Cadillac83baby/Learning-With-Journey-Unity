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
        static bool openingPlayedThisSession;

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

            // Main Menu can be reloaded after every game. The welcome should
            // introduce Journey once per app session, not repeat on every return.
            if (openingPlayedThisSession)
            {
                if (sequenceRoutine != null)
                    StopCoroutine(sequenceRoutine);
                sequenceRoutine = StartCoroutine(ReturnMenuSequence());
                return;
            }

            openingPlayedThisSession = true;

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
            AudioClip greeting = greetingClips != null && greetingClips.Length > 0
                ? greetingClips[0]
                : Resources.Load<AudioClip>("JourneyVoice/UI/MENU_welcome");

            if (greeting != null)
                yield return SpeakRoutine(greeting, 0f);
            else
                yield return TalkForDuration(3.2f);

            AudioClip choose = Resources.Load<AudioClip>("JourneyVoice/UI/Menu_choose");
            if (choose != null)
            {
                ShowSpeech("Choose a game, and let’s play!");
                yield return SpeakRoutine(choose, 0f);
            }

            HideSpeech();
            yield return GentlePoint();
            ResetVisualTransform();
            SetStableFrame();
            StartIdle();
        }

        IEnumerator ReturnMenuSequence()
        {
            yield return new WaitForSecondsRealtime(.15f);

            StopIdle();
            AudioClip choose = Resources.Load<AudioClip>("JourneyVoice/UI/Menu_choose");
            if (choose != null)
            {
                ShowSpeech("Choose a game, and let’s play!");
                yield return SpeakRoutine(choose, 0f);
            }

            HideSpeech();
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
