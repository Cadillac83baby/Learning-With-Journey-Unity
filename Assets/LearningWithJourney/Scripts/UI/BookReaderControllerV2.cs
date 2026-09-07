using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    public class BookReaderControllerV2 : MonoBehaviour
    {
        [Serializable]
        public class BookPage
        {
            public string heading;
            [TextArea(2, 5)] public string body;
            [TextArea(1, 3)] public string journeyLine;
        }

        [Header("Reader UI")]
        [SerializeField] TMP_Text bookTitleText;
        [SerializeField] TMP_Text pageHeadingText;
        [SerializeField] TMP_Text pageBodyText;
        [SerializeField] TMP_Text pageNumberText;
        [SerializeField] TMP_Text journeySpeechText;
        [SerializeField] BookPageArtworkV2 pageArtwork;
        [SerializeField] Button previousButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button readAgainButton;
        [SerializeField] TMP_Text nextButtonText;

        [Header("Journey Voice Only")]
        [SerializeField] JourneyVoicePlayerV2 journeyVoice;
        [SerializeField] bool autoReadFirstPage = true;
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
            {
                journeyVoice.PlayWelcome();
                if (autoReadFirstPage)
                    Invoke(nameof(ReadCurrentPage), 1.25f);
            }
        }

        void ReadCurrentPage() => journeyVoice?.PlayPage(bookId, pageIndex);

        public void PreviousPage()
        {
            if (pageIndex <= 0) return;
            pageIndex--;
            journeyVoice?.StopSpeaking();
            journeyVoice?.PlayPageTurn();
            ShowPage(autoReadOnPageTurn);
        }

        public void NextPage()
        {
            if (pageIndex >= pages.Length - 1)
            {
                journeyVoice?.StopSpeaking();
                journeyVoice?.PlayFinish();
                Invoke(nameof(BackToLibrary), 1.6f);
                return;
            }

            pageIndex++;
            journeyVoice?.StopSpeaking();
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
            CancelInvoke();
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
            if (nextButtonText != null) nextButtonText.text = pageIndex >= pages.Length - 1 ? "FINISH BOOK" : "NEXT PAGE";
            if (nextButton != null) nextButton.interactable = true;
            if (readAgainButton != null)
                readAgainButton.interactable = journeyVoice != null && journeyVoice.HasPageClip(bookId, pageIndex);

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

        static BookPage[] BuildABCBook()
        {
            string[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(c => c.ToString()).ToArray();
            string[] words =
            {
                "Apple","Ball","Cat","Dog","Egg","Fish","Grapes","Hat","Ice Cream","Juice","Kite","Lion","Moon",
                "Nest","Orange","Pig","Queen","Rainbow","Sun","Turtle","Umbrella","Van","Whale","Xylophone","Yo-Yo","Zebra"
            };
            string[] facts =
            {
                "Apples can be red, green, or yellow.","Balls can bounce, roll, and spin.","Cats have whiskers and soft paws.","Dogs can bark and wag their tails.",
                "Eggs have smooth shells.","Fish swim using fins.","Grapes grow in bunches.","Hats go on our heads.","Ice cream is a cold treat.","Juice is a drink made from fruit.",
                "Kites can fly high in the wind.","Lions are big cats.","The moon shines in the night sky.","Birds can build nests.","Oranges are round fruit.","Pigs can make an oink sound.",
                "A queen may wear a crown.","Rainbows can appear after rain.","The sun gives us light.","Turtles carry shells on their backs.","Umbrellas help keep us dry.","A van can carry people and things.",
                "Whales are very large ocean animals.","A xylophone makes music when we tap its bars.","A yo-yo goes down and comes back up.","Zebras have black and white stripes."
            };

            var result = new BookPage[26];
            for (int i = 0; i < result.Length; i++)
            {
                string letter = letters[i];
                string word = words[i];
                result[i] = P($"{letter} is for {word}", $"{letter} is for {word.ToLowerInvariant()}. {facts[i]}", $"{letter} is for {word}! Say {letter} with me!");
            }
            return result;
        }

        static BookPage[] BuildNumbersBook()
        {
            string[] numberWords = { "One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten","Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen","Twenty" };
            var result = new BookPage[20];
            for (int i = 0; i < result.Length; i++)
            {
                int number = i + 1;
                result[i] = P($"Number {number}", $"This is the number {number}. Count the objects on the page all the way to {number}.", $"Let's count to {numberWords[i].ToLowerInvariant()} together!");
            }
            return result;
        }

        static BookPage[] BuildColorsBook() => new[]
        {
            P("Red Circle", "This is a red circle. A circle is round and has no corners.", "Red circle! Round and bright."),
            P("Blue Square", "This is a blue square. A square has four equal sides.", "Blue square! Four equal sides."),
            P("Yellow Triangle", "This is a yellow triangle. A triangle has three sides.", "Yellow triangle! Count three sides."),
            P("Green Star", "This is a green star. This star has five points.", "Green star! Count the points."),
            P("Purple Heart", "This is a purple heart. Hearts can remind us to be kind and loving.", "Purple heart! Kind hearts shine."),
            P("Orange Rectangle", "This is an orange rectangle. A rectangle has four sides.", "Orange rectangle! Two long sides and two short sides."),
            P("Pink Oval", "This is a pink oval. An oval is round and stretched out.", "Pink oval! Smooth and round."),
            P("Brown Diamond", "This is a brown diamond. It has four sides and pointed corners.", "Brown diamond! Look at the points."),
            P("Black Crescent", "This is a black crescent. A crescent looks like a curved moon.", "Black crescent! It looks like the moon."),
            P("White Cloud", "This is a white cloud. Clouds can look soft and fluffy.", "White cloud! Soft and fluffy."),
            P("Teal Hexagon", "This is a teal hexagon. A hexagon has six sides.", "Teal hexagon! Let's count six sides."),
            P("Gold Starburst", "This is a gold starburst. It has many bright points spreading outward.", "Gold starburst! Look how it shines!")
        };

        static BookPage[] BuildStoryBook() => new[]
        {
            P("A Bright Morning", "Journey woke up and saw sunshine peeking through her window. A brand-new day was beginning.", "Good morning! Today is a new day to learn."),
            P("Ready to Learn", "Journey got ready and packed her little bag. She wondered what she would discover today.", "I wonder what we'll learn today!"),
            P("A Tiny Flower", "On the way, Journey spotted a tiny flower and stopped to look at its bright petals.", "Look at that pretty flower!"),
            P("A Kind Choice", "A friend needed help picking up blocks, so Journey smiled and helped.", "Helping is a kind choice."),
            P("Learning Time", "Journey practiced letters and numbers. Some were easy, and some took another try.", "We can learn one little step at a time."),
            P("Try Again", "Journey remembered that trying again helps our brains learn and grow.", "It's okay to try again!"),
            P("Colors After Rain", "After a little rain, a bright rainbow stretched across the sky.", "Look at the rainbow!"),
            P("Learning Everywhere", "Journey counted colors and named the ones she knew. Learning can happen everywhere.", "We can learn wherever we go!"),
            P("A Proud Moment", "At the end of the day, Journey thought about everything she had learned and every kind choice she had made.", "I learned so much today!"),
            P("Learn, Grow, Shine", "Journey smiled because every little thing she learned helped her grow.", "We learned, we grew, and we shined!")
        };

        static BookPage P(string heading, string body, string journeyLine)
        {
            return new BookPage { heading = heading, body = body, journeyLine = journeyLine };
        }
    }
}
