using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    public class LibraryAudioControllerV1 : MonoBehaviour
    {
        [SerializeField] TMP_Text speechText;
        [SerializeField] AudioSource voiceSource;

        readonly HashSet<Button> hookedButtons = new HashSet<Button>();

        void Awake()
        {
            if (voiceSource == null)
                voiceSource = GetComponent<AudioSource>();

            if (voiceSource == null)
                voiceSource = gameObject.AddComponent<AudioSource>();

            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.volume = 1f;
        }

        void Start()
        {
            FindSpeechText();
            HookBookButtons();
            PlayWelcome();
        }

        void FindSpeechText()
        {
            if (speechText != null) return;

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                string name = text.gameObject.name.ToLowerInvariant();

                if (name.Contains("speech") || name.Contains("journey"))
                {
                    speechText = text;
                    return;
                }
            }

            foreach (TMP_Text text in texts)
            {
                if (!string.IsNullOrEmpty(text.text) &&
                    text.text.Contains("What should we read"))
                {
                    speechText = text;
                    return;
                }
            }
        }

        void HookBookButtons()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                TMP_Text labelText = button.GetComponentInChildren<TMP_Text>(true);
                string label = labelText == null ? "" : Normalize(labelText.text);

                if (label.Contains("openbook"))
                    continue;

                if (label == "abc" || label.Contains("abcbooks"))
                {
                    Hook(button,
                        "Let's read an ABC book with Mom.",
                        "Library_abc",
                        "LIBRARY_abc");
                }
                else if (label == "123" || label.Contains("numbers"))
                {
                    Hook(button,
                        "Let's read and practice numbers with Mom.",
                        "Library_numbers",
                        "LIBRARY_numbers");
                }
                else if (label.Contains("colors") || label.Contains("shapes"))
                {
                    Hook(button,
                        "Let's read and learn about colors with Mom.",
                        "Library_colors",
                        "LIBRARY_colors");
                }
                else if (label.Contains("story"))
                {
                    Hook(button,
                        "Let's listen to Mom read a story.",
                        "Library_story",
                        "LIBRARY_story");
                }
            }
        }

        void Hook(Button button, string line, params string[] clipNames)
        {
            if (!hookedButtons.Add(button))
                return;

            button.onClick.AddListener(() =>
            {
                if (speechText != null)
                    speechText.text = line;

                PlayLibraryClip(clipNames);
            });
        }

        void PlayWelcome()
        {
            AudioClip clip = FindClip("Library_Welcome", "LIBRARY_welcome");

            if (clip != null)
            {
                if (speechText != null)
                    speechText.text = "What should we read today?";

                PlayClip(clip);
                return;
            }

            clip = FindClip("COMMON_welcome", "welcome");

            if (speechText != null)
                speechText.text = "Hi! I'm Journey. Pick a book and let's read together!";

            PlayClip(clip);
        }

        void PlayLibraryClip(params string[] clipNames)
        {
            AudioClip clip = FindClip(clipNames);

            if (clip != null)
                PlayClip(clip);
            else
                Debug.LogWarning("[LearningWithJourney] Library audio clip was not found.");
        }

        void PlayClip(AudioClip clip)
        {
            if (voiceSource == null || clip == null)
                return;

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();

            Debug.Log("[LearningWithJourney] Library voice playing: " + clip.name);
        }

        AudioClip FindClip(params string[] aliases)
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>("");

            foreach (AudioClip clip in clips)
            {
                string clipName = Normalize(clip.name);

                foreach (string alias in aliases)
                {
                    string wanted = Normalize(alias);

                    if (clipName == wanted)
                        return clip;
                }
            }

            foreach (AudioClip clip in clips)
            {
                string clipName = Normalize(clip.name);

                foreach (string alias in aliases)
                {
                    string wanted = Normalize(alias);

                    if (clipName.StartsWith(wanted))
                        return clip;
                }
            }

            return null;
        }

        static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            StringBuilder result = new StringBuilder();

            foreach (char character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                    result.Append(character);
            }

            return result.ToString();
        }
    }
}
