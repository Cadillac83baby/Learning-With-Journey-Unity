using UnityEngine;

namespace LearningWithJourney.Character
{
    public class JourneyAnimatorController : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] AudioSource voiceSource;

        static readonly int Idle = Animator.StringToHash("Idle");
        static readonly int Walk = Animator.StringToHash("Walk");
        static readonly int Wave = Animator.StringToHash("Wave");
        static readonly int Talk = Animator.StringToHash("Talk");
        static readonly int Point = Animator.StringToHash("Point");
        static readonly int Think = Animator.StringToHash("Think");
        static readonly int Clap = Animator.StringToHash("Clap");
        static readonly int Celebrate = Animator.StringToHash("Celebrate");
        static readonly int TryAgain = Animator.StringToHash("TryAgain");
        static readonly int Jump = Animator.StringToHash("Jump");
        static readonly int Talking = Animator.StringToHash("Talking");

        void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            voiceSource = GetComponent<AudioSource>();
        }

        public void PlayIdle() => Trigger(Idle);
        public void PlayWalk() => Trigger(Walk);
        public void PlayWave() => Trigger(Wave);
        public void PlayPoint() => Trigger(Point);
        public void PlayThink() => Trigger(Think);
        public void PlayClap() => Trigger(Clap);
        public void PlayCelebrate() => Trigger(Celebrate);
        public void PlayTryAgain() => Trigger(TryAgain);
        public void PlayJump() => Trigger(Jump);

        public void Speak(AudioClip clip)
        {
            if (clip == null || voiceSource == null)
            {
                Trigger(Talk);
                return;
            }
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
            if (animator != null) animator.SetBool(Talking, true);
            CancelInvoke(nameof(StopTalking));
            Invoke(nameof(StopTalking), clip.length);
        }

        public void StopTalking()
        {
            if (animator != null) animator.SetBool(Talking, false);
        }

        void Trigger(int id)
        {
            if (animator == null) return;
            animator.SetTrigger(id);
        }
    }
}
