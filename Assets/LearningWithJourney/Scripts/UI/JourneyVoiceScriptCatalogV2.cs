using System;
using System.Collections.Generic;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Canonical narration copy for the Library. These lines are the source text
    /// used when producing Journey's child-friendly synthetic narration clips.
    /// The intended direction is natural, bright, warm, playful and expressive,
    /// without attempting to exactly imitate any real child's voice.
    /// </summary>
    public static class JourneyVoiceScriptCatalogV2
    {
        public const string Welcome = "Hi! I'm Journey. Pick a book and let's read together!";
        public const string Finish = "We finished the book! Great reading. Let's choose another one!";

        static readonly string[] ABC =
        {
            "A is for apple. Apple starts with the letter A. Can you say A?",
            "B is for ball. Ball starts with the letter B. Can you say B?",
            "C is for cat. Cat starts with the letter C. Can you say C?",
            "D is for dog. Dog starts with the letter D. Can you say D?",
            "E is for egg. Egg starts with the letter E. Can you say E?",
            "F is for fish. Fish starts with the letter F. Can you say F?",
            "G is for grapes. Grapes starts with the letter G. Can you say G?",
            "H is for hat. Hat starts with the letter H. Can you say H?",
            "I is for ice cream. Ice cream starts with the letter I. Can you say I?",
            "J is for juice. Juice starts with the letter J. Can you say J?",
            "K is for kite. Kite starts with the letter K. Can you say K?",
            "L is for lion. Lion starts with the letter L. Can you say L?",
            "M is for moon. Moon starts with the letter M. Can you say M?",
            "N is for nest. Nest starts with the letter N. Can you say N?",
            "O is for orange. Orange starts with the letter O. Can you say O?",
            "P is for pig. Pig starts with the letter P. Can you say P?",
            "Q is for queen. Queen starts with the letter Q. Can you say Q?",
            "R is for rainbow. Rainbow starts with the letter R. Can you say R?",
            "S is for sun. Sun starts with the letter S. Can you say S?",
            "T is for turtle. Turtle starts with the letter T. Can you say T?",
            "U is for umbrella. Umbrella starts with the letter U. Can you say U?",
            "V is for van. Van starts with the letter V. Can you say V?",
            "W is for whale. Whale starts with the letter W. Can you say W?",
            "X is for xylophone. Xylophone starts with the letter X. Can you say X?",
            "Y is for yo-yo. Yo-yo starts with the letter Y. Can you say Y?",
            "Z is for zebra. Zebra starts with the letter Z. Can you say Z?"
        };

        static readonly string[] Numbers =
        {
            "One. I see one star. Let's count: one.",
            "Two. I see two stars. Let's count: one, two.",
            "Three. I see three stars. Let's count: one, two, three.",
            "Four. Let's count together: one, two, three, four.",
            "Five. Let's count together: one, two, three, four, five.",
            "Six. Count with me: one, two, three, four, five, six.",
            "Seven. Count with me all the way to seven.",
            "Eight. Great job! Let's count eight objects together.",
            "Nine. We can count nine. Keep going nice and slow.",
            "Ten. One, two, three, four, five, six, seven, eight, nine, ten!",
            "Eleven. Ten and one more makes eleven.",
            "Twelve. Ten and two more makes twelve.",
            "Thirteen. Let's count thirteen objects together.",
            "Fourteen. Keep counting. You made it to fourteen!",
            "Fifteen. Let's count all the way to fifteen.",
            "Sixteen. Nice counting! This number is sixteen.",
            "Seventeen. Let's count and find seventeen.",
            "Eighteen. Great work! This number is eighteen.",
            "Nineteen. We're almost at twenty. This is nineteen.",
            "Twenty. We did it! We counted all the way to twenty!"
        };

        static readonly string[] Colors =
        {
            "Red circle. A circle is round, and this circle is red.",
            "Blue square. A square has four equal sides, and this square is blue.",
            "Yellow triangle. A triangle has three sides, and this triangle is yellow.",
            "Green star. This star is green. Can you count its five points?",
            "Purple heart. This heart is purple. Hearts can remind us to be kind.",
            "Orange rectangle. A rectangle has four sides, and this one is orange.",
            "Pink oval. An oval is round and stretched out. This oval is pink.",
            "Brown diamond. A diamond has four sides and pointed corners. This one is brown.",
            "Black crescent. A crescent looks like a curved moon. This crescent is black.",
            "White cloud. This cloud is white, soft, and fluffy looking.",
            "Teal hexagon. A hexagon has six sides. Let's count them together.",
            "Gold starburst. Look at all those points. This bright shape is gold!"
        };

        static readonly string[] Story =
        {
            "Journey woke up and saw sunshine peeking through the window. Good morning! Today is a brand-new day to learn.",
            "Journey got ready and packed her little bag. She wondered what new thing she would discover today.",
            "On the way, Journey spotted a tiny flower. She stopped to look closely at its bright petals.",
            "A friend needed help picking up some blocks. Journey smiled and helped because kind choices can make a big difference.",
            "At learning time, Journey practiced letters and numbers. Some were easy, and some took another try.",
            "Journey remembered that it is okay to try again. Every try helps our brains learn and grow.",
            "After a little rain, bright colors appeared across the sky. Journey pointed up and said, look at the rainbow!",
            "Journey counted the rainbow colors and named the ones she knew. Learning can happen everywhere we look.",
            "At the end of the day, Journey thought about everything she had learned and every kind choice she had made.",
            "Journey smiled. We learned, we grew, and we shined today. I'm proud of you. See you for our next adventure!"
        };

        public static string GetNarration(string bookId, int pageIndex)
        {
            string[] book = GetBook(bookId);
            if (book.Length == 0) return string.Empty;
            int index = Math.Max(0, Math.Min(pageIndex, book.Length - 1));
            return book[index];
        }

        public static int GetPageCount(string bookId) => GetBook(bookId).Length;

        public static IEnumerable<(string id, string text)> EnumerateAll()
        {
            yield return ("COMMON_welcome", Welcome);
            yield return ("COMMON_finish", Finish);
            foreach (var row in EnumerateBook("ABC", ABC)) yield return row;
            foreach (var row in EnumerateBook("NUMBERS", Numbers)) yield return row;
            foreach (var row in EnumerateBook("COLORS", Colors)) yield return row;
            foreach (var row in EnumerateBook("STORY", Story)) yield return row;
        }

        static IEnumerable<(string id, string text)> EnumerateBook(string id, string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
                yield return ($"{id}_{i + 1:00}", lines[i]);
        }

        static string[] GetBook(string bookId)
        {
            switch ((bookId ?? "ABC").Trim().ToUpperInvariant())
            {
                case "NUMBERS": return Numbers;
                case "COLORS": return Colors;
                case "STORY": return Story;
                default: return ABC;
            }
        }
    }
}
