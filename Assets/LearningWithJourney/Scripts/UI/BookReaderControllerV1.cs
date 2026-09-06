using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    public class BookReaderControllerV1 : MonoBehaviour
    {
        [Serializable]
        public class BookPage
        {
            public string heading;
            [TextArea(2, 5)] public string body;
            [TextArea(1, 4)] public string journeyLine;
        }

        [Header("Reader UI")]
        [SerializeField] TMP_Text bookTitleText;
        [SerializeField] TMP_Text pageHeadingText;
        [SerializeField] TMP_Text pageBodyText;
        [SerializeField] TMP_Text pageNumberText;
        [SerializeField] TMP_Text journeySpeechText;
        [SerializeField] BookPageArtworkV1 pageArtwork;
        [SerializeField] Button previousButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button readAgainButton;
        [SerializeField] TMP_Text nextButtonText;

        [Header("Journey Voice Only")]
        [SerializeField] JourneyVoicePlayerV1 journeyVoice;
        [SerializeField] bool autoReadOnPageTurn = true;

        string bookId;
        int pageIndex;
        BookPage[] pages = Array.Empty<BookPage>();

        void Start()
        {
            bookId = PlayerPrefs.GetString(LibraryScreenControllerV1.SelectedBookKey, "ABC").ToUpperInvariant();
            LoadBook(bookId);
            pageIndex = 0;
            ShowPage(false);

            if (journeyVoice != null)
                journeyVoice.PlayWelcome();
        }

        public void PreviousPage()
        {
            if (pageIndex <= 0) return;
            pageIndex--;
            journeyVoice?.PlayPageTurn();
            ShowPage(autoReadOnPageTurn);
        }

        public void NextPage()
        {
            if (pageIndex >= pages.Length - 1)
            {
                BackToLibrary();
                return;
            }

            pageIndex++;
            journeyVoice?.PlayPageTurn();
            ShowPage(autoReadOnPageTurn);
        }

        public void ReadAgain()
        {
            journeyVoice?.StopSpeaking();
            journeyVoice?.PlayPage(bookId, pageIndex);
        }

        public void BackToLibrary()
        {
            journeyVoice?.StopSpeaking();
            SceneManager.LoadScene("Library");
        }

        void ShowPage(bool readAloud)
        {
            if (pages == null || pages.Length == 0) return;
            pageIndex = Mathf.Clamp(pageIndex, 0, pages.Length - 1);
            BookPage page = pages[pageIndex];

            if (bookTitleText != null) bookTitleText.text = BookTitle(bookId);
            if (pageHeadingText != null) pageHeadingText.text = page.heading;
            if (pageBodyText != null) pageBodyText.text = page.body;
            if (pageNumberText != null) pageNumberText.text = "PAGE " + (pageIndex + 1) + " / " + pages.Length;
            if (journeySpeechText != null) journeySpeechText.text = page.journeyLine;
            if (pageArtwork != null) pageArtwork.SetPage(bookId, pageIndex);

            if (previousButton != null) previousButton.interactable = pageIndex > 0;
            if (nextButtonText != null) nextButtonText.text = pageIndex >= pages.Length - 1 ? "BACK TO LIBRARY" : "NEXT PAGE";
            if (nextButton != null) nextButton.interactable = true;
            if (readAgainButton != null) readAgainButton.interactable = journeyVoice != null;

            if (readAloud)
                journeyVoice?.PlayPage(bookId, pageIndex);
        }

        void LoadBook(string id)
        {
            switch (id)
            {
                case "NUMBERS": pages = BuildNumbersBook(); break;
                case "COLORS": pages = BuildColorsBook(); break;
                case "STORY": pages = BuildStoryBook(); break;
                default:
                    bookId = "ABC";
                    pages = BuildABCBook();
                    break;
            }
        }

        static string BookTitle(string id)
        {
            switch (id)
            {
                case "NUMBERS": return "NUMBERS & COUNTING";
                case "COLORS": return "COLORS & SHAPES";
                case "STORY": return "STORY TIME";
                default: return "ABC BOOK";
            }
        }

        static BookPage[] BuildABCBook() => new[]
        {
            P("A is for Apple", "A is for apple. Apples can be red, green, or yellow.", "A is for apple. Apple starts with the letter A!"),
            P("B is for Ball", "B is for ball. A ball can bounce, roll, and spin.", "B is for ball. Ball starts with the letter B!"),
            P("C is for Cat", "C is for cat. Cats have whiskers and soft paws.", "C is for cat. Cat starts with the letter C!"),
            P("D is for Dog", "D is for dog. Dogs can bark, run, and wag their tails.", "D is for dog. Dog starts with the letter D!"),
            P("E is for Egg", "E is for egg. Eggs come in smooth shells.", "E is for egg. Egg starts with the letter E!"),
            P("F is for Fish", "F is for fish. Fish swim through the water using fins.", "F is for fish. Fish starts with the letter F!")
        };

        static BookPage[] BuildNumbersBook() => new[]
        {
            P("Number 1", "One star. Touch and count: one.", "One! I see one star. Let's count it together."),
            P("Number 2", "Two stars. Count them slowly: one, two.", "Two! One, two. Great counting!"),
            P("Number 3", "Three stars. Count: one, two, three.", "Three! One, two, three. You did it!"),
            P("Number 4", "Four stars. Count all four from left to right.", "Four! One, two, three, four."),
            P("Number 5", "Five stars. Count: one, two, three, four, five.", "Five! We counted all the way to five!")
        };

        static BookPage[] BuildColorsBook() => new[]
        {
            P("Red Circle", "This shape is a circle. Its color is red.", "Red circle! A circle is round."),
            P("Blue Square", "This shape is a square. It has four equal sides. Its color is blue.", "Blue square! A square has four sides."),
            P("Yellow Triangle", "This shape is a triangle. It has three sides. Its color is yellow.", "Yellow triangle! A triangle has three sides."),
            P("Green Star", "This is a green star. Count its five points.", "Green star! Let's count its points."),
            P("Purple Heart", "This is a purple heart. Hearts can remind us to be kind and loving.", "Purple heart! Kind hearts help us shine.")
        };

        static BookPage[] BuildStoryBook() => new[]
        {
            P("A Bright Morning", "Journey woke up and saw the sunshine peeking through her window.", "Good morning! The sun is shining. Today can be a great day!"),
            P("Ready to Learn", "She packed her little bag and chose something new to learn.", "I love learning new things. What should we discover today?"),
            P("A Kind Choice", "Journey saw someone who needed help, so she stopped and shared a kind smile.", "Kindness is powerful. We can help people with small, caring choices."),
            P("Colors After the Rain", "Later, a rainbow stretched across the sky with bright colors from end to end.", "Look at the rainbow! So many beautiful colors."),
            P("Learn, Grow, Shine", "At the end of the day, Journey remembered that every little thing she learned helped her grow.", "We learned, we grew, and we shined today. I'm proud of you!")
        };

        static BookPage P(string heading, string body, string journeyLine)
        {
            return new BookPage { heading = heading, body = body, journeyLine = journeyLine };
        }
    }
}
