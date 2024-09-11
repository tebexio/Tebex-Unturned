namespace Tebex.Util
{

    /// <summary>
    /// Utility class for writing nicely formatted text
    /// </summary>
    public static class Ansi
    {
        public static string Yellow(string text)
        {
            return $"\x1b[33m{text}\x1b[0m";
        }

        public static string Red(string text)
        {
            return $"\x1b[31m{text}\x1b[0m";
        }

        public static string Green(string text)
        {
            return $"\x1b[32m{text}\x1b[0m";
        }

        public static string Blue(string text)
        {
            return $"\x1b[34m{text}\x1b[0m";
        }

        public static string Purple(string text)
        {
            return $"\x1b[35m{text}\x1b[0m";
        }

        public static string White(string text)
        {
            return $"\x1b[37m{text}\x1b[0m";
        }

        public static string Bold(string text)
        {
            return $"\x1b[1m{text}\x1b[0m";
        }

        public static string Clear()
        {
            return "\x1b[2J";
        }

        public static string ResetCursor()
        {
            return "\x1b[1;1H";
        }

        public static string Underline(string text)
        {
            return $"\x1b[4m{text}\x1b[0m";
        }
    }
}