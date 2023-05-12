using System.Text.RegularExpressions;

namespace FFF.Shared.Utilities
{
    public sealed class RegularExpressionUtilities
    {

        public static Regex CSVParser(char delimiter)
        {
            return new Regex($"{delimiter}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        }

        public static Regex CSVParser(string delimiter)
        {
            return new Regex($"{delimiter}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        }

        public static Match MatchDoubleQuotes(string text)
        {
            return new Regex(@"""(\\""|\\\\|[^""\\])*""").Match(text);
        }

        public static string FindNumberInText(string text)
        {
            return Regex.Match(text, @"\d+").Value;
        }

        public static string FindTextOnly(string text)
        {
            return Regex.Match(text, @"(?i)^[a-z]+").Value;
        }

    }
}
